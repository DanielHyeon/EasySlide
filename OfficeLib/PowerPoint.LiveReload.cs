using System;
using System.IO;
using NetOffice.OfficeApi.Enums;
using NetOffice.PowerPointApi;
using NetOffice.PowerPointApi.Enums;
using static Axicultus.Interop.DesktopWindows;

namespace OfficeLib;

public partial class PowerPoint
{
    // PowerPoint pauses a slideshow while the operator is working in the editing window.
    private static bool IsSlideVisible(PpSlideShowState state) =>
        state is PpSlideShowState.ppSlideShowRunning or PpSlideShowState.ppSlideShowPaused;

    // PowerPoint can return the user's open document even for an Untitled open of the same path.
    // A distinct on-disk name also ensures the copy contains precisely the saved edits.
    internal static Presentation OpenShowCopy(Presentations documents, string path, MsoTriState withWindow, bool sourceIsOpen)
    {
        if (!sourceIsOpen)
        {
            var copy = documents.Open(path, MsoTriState.msoTrue, MsoTriState.msoTrue, withWindow);
            if (!string.Equals(copy.FullName, Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase)) return copy;
            // The editor opened the file between our check and Open. Release only our reference to it.
            ReleaseProxy(copy);
        }
        return OpenDiskCopy(documents, path, withWindow, MsoTriState.msoTrue, out _);
    }

    // A named, read-only copy has a stable slideshow caption on localized Office installations.
    // The caller retains its backing file until PowerPoint releases the presentation.
    internal static Presentation OpenDiskCopy(Presentations documents, string path, MsoTriState withWindow, MsoTriState untitled, out string snapshot)
    {
        snapshot = Path.Combine(Path.GetTempPath(), "Axicultus-" + Guid.NewGuid().ToString("N") + Path.GetExtension(path));
        bool opened = false;
        try
        {
            File.Copy(path, snapshot);
            var attributes = File.GetAttributes(snapshot);
            if ((attributes & FileAttributes.ReadOnly) != 0)
                File.SetAttributes(snapshot, attributes & ~FileAttributes.ReadOnly);
            var copy = documents.Open(snapshot, MsoTriState.msoTrue, untitled, withWindow);
            opened = true;
            return copy;
        }
        finally
        {
            if (!opened || untitled == MsoTriState.msoTrue) DeleteDiskCopy(snapshot);
        }
    }

    internal static void DeleteDiskCopy(string snapshot)
    {
        try { File.Delete(snapshot); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            System.Diagnostics.Trace.WriteLine($"Removing the temporary PowerPoint copy: {ex.Message}");
        }
    }

    private static void MarkShowCopy(_Presentation copy, string path)
    {
        var tags = copy.Tags;
        try { tags.Add(ShowCopyTagName, ShowCopyMark(path)); }
        finally { ReleaseProxy(tags); }
        copy.Saved = MsoTriState.msoTrue;
    }

    private void PositionRunWindow(SlideShowWindow window, string fullName)
    {
        if (!KeepSlideShowOnTop && !TargetBounds.HasValue) return;
        currentSlideShowWindow = FindSlideShowWindow(window, () => fullName);
        currentSlideShowProcessId = 0;
        PositionedOnTarget = false;
        if (currentSlideShowWindow != IntPtr.Zero)
        {
            GetWindowThreadProcessId(currentSlideShowWindow, out currentSlideShowProcessId);
            if (TargetBounds is { } bounds && IsSameWindow(currentSlideShowWindow, currentSlideShowProcessId))
                PositionedOnTarget = SetWindowPos(currentSlideShowWindow, IntPtr.Zero, bounds.Left, bounds.Top,
                    bounds.Width, bounds.Height, SWP_NOZORDER | SWP_NOACTIVATE);
        }
        if (KeepSlideShowOnTop) RaiseCurrentSlideShowWindow();
    }

    public SlideImageManifest.SourceVersion GetRunSourceVersion(string path, ShowCopyList copies)
    {
        int row = FindListedEntry(path, copies);
        return row > 0 && string.Equals(copies[row].CopyName, CurrentRunPresentationName, StringComparison.OrdinalIgnoreCase)
            ? copies[row].OpenedVersion : null;
    }

    /// <summary>Swaps only this run's marked copy after the replacement can display the old native slide number.</summary>
    public (int Slide, int Count)? ReloadChangedRun(string path, ShowCopyList copies,
        SlideImageManifest.SourceVersion revision, Func<bool> stillLive)
    {
        var opened = GetRunSourceVersion(path, copies);
        if (opened == null || revision == null || opened.Matches(revision) || !stillLive()) return null;
        using var gate = PowerPointGate.Enter(PowerPointCallWeight.Heavy, "reload the edited live presentation");
        if (gate == null) return null;

        _Presentation replacement = null;
        Presentations documents = null;
        try
        {
            using var previous = FindRunSlideShow(out var previousWindow);
            if (previous == null || !IsSlideVisible(previousWindow.View.State)) return null;
            documents = prePowerPointApp.Presentations;
            replacement = OpenShowCopy(documents, path, MsoTriState.msoFalse, IsFileOpenInPowerPoint(path));
            MarkShowCopy(replacement, path);
            int count = replacement.Slides.Count;
            if (count == 0 || !revision.Matches(SlideImageManifest.ReadSource(path)) || !stillLive()) return null;

            var oldView = previousWindow.View;
            var state = oldView.State;
            if (!IsSlideVisible(state)) return null;
            int slide = Math.Clamp(oldView.Slide.SlideIndex, 1, count);
            string fullName = replacement.FullName;
            var settings = replacement.SlideShowSettings;
            DisablePresenterView(settings);
            if (!stillLive()) return null;
            var window = settings.Run();
            var newView = window.View;
            newView.GotoSlide(slide);
            if (state == PpSlideShowState.ppSlideShowPaused) newView.State = state;
            if (newView.Slide.SlideIndex != slide || !stillLive() || GetRunSourceVersion(path, copies) != opened)
                return null;

            PositionRunWindow(window, fullName);
            if (!PositionedOnTarget)
            {
                window.Left = previousWindow.Left;
                window.Top = previousWindow.Top;
                window.Width = previousWindow.Width;
                window.Height = previousWindow.Height;
            }
            if (!stillLive() || GetRunSourceVersion(path, copies) != opened) return null;

            int row = FindListedEntry(path, copies);
            // If the old copy is busy, keep it listed for show-end cleanup. Lookups use the last matching row.
            var oldCopyName = copies[row].CopyName;
            copies[row].CopyName = fullName;
            copies[row].OpenedVersion = revision;
            RememberRunPresentation(fullName, 0, ShowCopyMark(path), prePowerPointApp);
            var running = replacement;
            replacement = null;
            ReleaseProxy(running);
            try { previous.Presentation.Close(); }
            catch (Exception ex)
            {
                Console.WriteLine($"Closing the previous live copy: {ex.Message}");
                // Keep the retired copy before the active row so future runs resolve to the replacement.
                if (GetRunSourceVersion(path, copies) == revision)
                {
                    copies[row].CopyName = oldCopyName;
                    copies[row].OpenedVersion = opened;
                    copies[copies.Add(path, fullName)].OpenedVersion = revision;
                }
            }
            return (slide, count);
        }
        finally
        {
            // An incomplete file, an ended show, or a switch to another item keeps the original run.
            if (replacement != null) ClosePresentation(ref replacement);
            ReleaseProxy(documents);
        }
    }
}
