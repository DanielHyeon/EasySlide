using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Axicultus.Wpf.Rendering;
using NetOffice.OfficeApi.Enums;
using NetOffice.PowerPointApi;
using NetOffice.PowerPointApi.Enums;
using OfficeLib;

namespace Axicultus.Wpf.Interop;

public sealed partial class OfficePptSession
{
    // NetOffice disposes child proxies with their parent. Keep both until this owned copy leaves the show.
    private sealed class ShowDeck(Presentations collection, Presentation presentation,
        SlideImageManifest.SourceVersion version, string backingPath) : IDisposable
    {
        public Presentation Presentation { get; } = presentation;
        public SlideImageManifest.SourceVersion Version { get; } = version;
        public int Count { get; } = CountSlides(presentation);
        public long NextFileCheck { get; set; }
        public long NextHandleCheck { get; set; }

        private static int CountSlides(Presentation deck)
        {
            using var slides = deck.Slides;
            return slides.Count;
        }

        public void Dispose()
        {
            try { Presentation.Close(); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
            finally
            {
                collection.Dispose();
                OfficeLib.PowerPoint.DeleteDiskCopy(backingPath);
            }
        }
    }

    private ShowDeck OpenShowDeck(string path, SlideImageManifest.SourceVersion version)
    {
        var collection = _ppt!.Presentations;
        Presentation? copy = null;
        string? backingPath = null;
        try
        {
            copy = OpenWithMacrosDisabled(_ppt, () => OfficeLib.PowerPoint.OpenDiskCopy(
                collection, path, MsoTriState.msoFalse, MsoTriState.msoFalse, out backingPath));
            return new(collection, copy, version, backingPath!);
        }
        catch
        {
            try { copy?.Close(); }
            finally
            {
                collection.Dispose();
                if (backingPath != null) OfficeLib.PowerPoint.DeleteDiskCopy(backingPath);
            }
            throw;
        }
    }

    private ShowDeck GetShowDeck(string path)
    {
        if (_showDecks.TryGetValue(path, out var existing)) return existing;
        var version = SlideImageManifest.ReadSource(path) ?? throw new FileNotFoundException("PowerPoint file unavailable.", path);
        return _showDecks[path] = OpenShowDeck(path, version);
    }

    private static bool IsShowingSlide(PpSlideShowState state) =>
        state is PpSlideShowState.ppSlideShowRunning or PpSlideShowState.ppSlideShowPaused;

    internal Task<PowerPointOutputPosition?> PollOutputAsync(OutputShow expected, Func<bool> stillCurrent) => RunOnStaAsync<PowerPointOutputPosition?>(() =>
    {
        if (!stillCurrent() || !ReferenceEquals(expected, _outputShow) || !_showDecks.TryGetValue(expected.Path, out var deck)) return null;
        using var windows = _ppt!.SlideShowWindows;
        using var window = FindShowWindow(windows, deck.Presentation.FullName);
        using var view = window?.View;
        if (view == null || !IsShowingSlide(view.State)) return null;
        using var current = view.Slide;
        var position = new PowerPointOutputPosition(current.SlideIndex, deck.Count, deck.Version);
        long now = Environment.TickCount64;
        if (now < deck.NextFileCheck) return position;
        deck.NextFileCheck = now + 2000;
        if (expected.Handle == 0 && now >= deck.NextHandleCheck)
        {
            deck.NextHandleCheck = now + 10000;
            var handle = OfficeLib.PowerPoint.FindSlideShowWindow(window, () => deck.Presentation.Name, out _);
            if (handle != 0 && stillCurrent())
            {
                if (expected.Bounds is { } bounds)
                {
                    Platform.DesktopWindowPlacement.Apply(handle, bounds);
                    Platform.DesktopWindowPlacement.SetTopmost(handle, expected.Topmost);
                }
                _outputShow = expected with { Handle = handle };
            }
        }
        var revision = SlideImageManifest.ReadSource(expected.Path);
        if (revision == null || deck.Version.Matches(revision)) return position;

        ShowDeck? replacement = null;
        try
        {
            replacement = OpenShowDeck(expected.Path, revision);
            if (replacement.Count == 0 || !revision.Matches(SlideImageManifest.ReadSource(expected.Path))) return position;
            var state = view.State;
            if (!stillCurrent() || !IsShowingSlide(state)) return null;
            using var lastSlide = view.Slide;
            int target = Math.Clamp(lastSlide.SlideIndex, 1, replacement.Count);
            ConfigureSlideShowForLive(replacement.Presentation, 1);
            using var settings = replacement.Presentation.SlideShowSettings;
            using var next = settings.Run();
            using var nextView = next.View;
            nextView.GotoSlide(target, MsoTriState.msoFalse);
            if (state == PpSlideShowState.ppSlideShowPaused) nextView.State = state;
            using var displayed = nextView.Slide;
            if (displayed.SlideIndex != target || !stillCurrent()) return null;
            ActivateShow(replacement.Presentation, next, expected.Path, true, expected.Bounds, expected.Topmost, window);
            _showDecks[expected.Path] = replacement;
            position = new(target, replacement.Count, revision);
            replacement = null;
            deck.Dispose();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[PPT] Reload deferred: {ex.GetBaseException().Message}");
            deck.NextFileCheck = Environment.TickCount64 + 10000;
        }
        finally { replacement?.Dispose(); }
        return position;
    });
}
