using System;
using System.Threading.Tasks;
using Axicultus.Module;
using OfficeLib;

namespace Axicultus;

public partial class FrmMain
{
    private bool checkingLivePowerPoint;
    private long nextPowerPointFileCheck;

    private async Task RefreshEditedLivePowerPointAsync()
    {
        if (checkingLivePowerPoint || Environment.TickCount64 < nextPowerPointFileCheck ||
            Gf.LivePowerPointRunState != Gf.LivePowerPointRun.Started || Gf.LiveItem.Type != "P" ||
            Gf.ShowLiveCam || Gf.LivePP.RunShowLost || PowerPointCallLimit.Current != null) return;

        var wrapper = Gf.LivePP;
        string path = Gf.LiveItem.Path;
        var opened = wrapper.GetRunSourceVersion(path, Gf.PowerpointList);
        if (opened == null) return;
        int serial = Gf.LiveLoadSerial;
        bool StillLive() => !IsDisposed && !Disposing && Gf.ShowRunning && !Gf.ShowLiveCam &&
            serial == Gf.LiveLoadSerial && Gf.LivePowerPointRunState == Gf.LivePowerPointRun.Started &&
            IsPowerPointFile(Gf.LiveItem, path) && ReferenceEquals(wrapper, Gf.LivePP);

        checkingLivePowerPoint = true;
        int nextCheckDelay = 2000;
        try
        {
            // A worship list may point to a network share. Only PowerPoint and UI work requires the STA.
            var revision = await Task.Run(() => SlideImageManifest.ReadSource(path));
            if (revision == null || opened.Matches(revision) || !StillLive() ||
                PowerPointCallLimit.Current != null || wrapper.GetRunSourceVersion(path, Gf.PowerpointList) != opened) return;

            var slides = new SongSettingsBase();
            // Give a file still being saved, or an unresponsive PowerPoint, time before retrying an export.
            nextCheckDelay = 10000;
            string outputPrefix = Gf.PowerpointImagePrefix(IsPowerPointFile(Gf.OutputItem, path) ? Gf.OutputItem : Gf.LiveItem, Gf.OUTPPPrefix);
            if (!wrapper.BuildScreenOutDumps(path, outputPrefix, ref slides.TotalSlides, Gf.MAX_VERSES, Gf.DB_MAXSLIDES,
                ref slides.SongVerses, ref slides.Slide, Gf.SequenceSymbol) || !StillLive()) return;
            // The preview can list the same file under a different title and therefore use a different cache prefix.
            string previewPrefix = Gf.PREPPFullPath;
            if (IsPowerPointFile(Gf.PreviewItem, path) && !wrapper.BuildScreenDumps(path, previewPrefix, ref slides.TotalSlides,
                Gf.MAX_VERSES, Gf.DB_MAXSLIDES, ref slides.SongVerses, ref slides.Slide, Gf.SequenceSymbol)) return;

            var resumed = wrapper.ReloadChangedRun(path, Gf.PowerpointList, revision, StillLive);
            if (resumed is not { } live || !StillLive()) return;
            ApplyReloadedSlides(Gf.LiveItem, slides, live.Slide, live.Count);
            nextCheckDelay = 2000;
            LiveShow?.KeepAboveSlideShow();
            if (IsPowerPointFile(Gf.OutputItem, path) && Gf.OUTPPFullPath == outputPrefix)
            {
                MainPPT.FollowRunOf(wrapper, path, Gf.PowerpointList);
                ApplyReloadedSlides(Gf.OutputItem, slides, live.Slide, live.Count);
                flowLayoutOutputPowerPoint.ResetSelection();
                DisplayOutputPowerPointThumbnails(live.Slide);
                ShowDualMonitorPP_Preview(ref Gf.OutputItem);
                BuildVerseButtons(Gf.OutputItem);
            }
            if (IsPowerPointFile(Gf.PreviewItem, path) && Gf.PREPPFullPath == previewPrefix)
            {
                ApplyReloadedSlides(Gf.PreviewItem, slides, Gf.PreviewItem.CurSlide, live.Count);
                flowLayoutPreviewPowerPoint.ResetSelection();
                DisplayPreviewPowerPointThumbnails(Gf.PreviewItem.CurSlide);
                PP_Preview(ref Gf.PreviewItem);
                BuildVerseButtons(Gf.PreviewItem);
            }
            ShowStatusBarSummary();
        }
        catch (Exception ex)
        {
            WriteDebugLogThrottled("Reload the edited live PowerPoint", ex);
        }
        finally
        {
            checkingLivePowerPoint = false;
            nextPowerPointFileCheck = Environment.TickCount64 + nextCheckDelay;
        }
    }

    internal static bool IsPowerPointFile(SongSettingsBase item, string path) =>
        item.Type == "P" && string.Equals(item.Path, path, StringComparison.OrdinalIgnoreCase);

    internal static void ApplyReloadedSlides(SongSettingsBase item, SongSettingsBase images, int position, int count)
    {
        item.TotalSlides = count;
        item.CurSlide = Math.Clamp(position, 1, count);
        item.SongVerses = (int[])images.SongVerses.Clone();
        var markers = (int[,])images.Slide.Clone();
        // Row zero belongs to this pane's layout. The numbered rows contain the new slide markers.
        Array.Copy(item.Slide, markers, item.Slide.GetLength(1));
        item.Slide = markers;
    }
}
