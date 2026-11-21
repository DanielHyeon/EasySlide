using System;
using System.IO;
using System.Linq;
using Axicultus.Module;
using NetOffice.OfficeApi.Enums;
using NetOffice.PowerPointApi.Enums;
using OfficeLib;
using Xunit;

namespace Axicultus.Tests;

public class LivePowerPointReloadTests
{
    [Fact]
    public void RevisionDistinguishesAnEditFromAMissingOrUnchangedFile()
    {
        var dir = Directory.CreateTempSubdirectory();
        try
        {
            string path = Path.Combine(dir.FullName, "deck.pptx");
            File.WriteAllText(path, "one");
            var original = SlideImageManifest.ReadSource(path);
            Assert.True(original.Matches(SlideImageManifest.ReadSource(path)));
            File.AppendAllText(path, "two");
            File.SetLastWriteTimeUtc(path, new DateTime(original.LastWriteTicks, DateTimeKind.Utc));
            Assert.False(original.Matches(SlideImageManifest.ReadSource(path)));
            File.Delete(path);
            Assert.Null(SlideImageManifest.ReadSource(path));
        }
        finally { dir.Delete(true); }
    }

    [Fact]
    public void MatchingPanesKeepSeparateCursorsAndVerseMarkersAfterAShrink()
    {
        var images = new SongSettingsBase { Slide = new int[4, 8] };
        images.SongVerses[1] = 3;
        images.Slide[3, 0] = 1;
        var preview = new SongSettingsBase { Type = "P", Path = "DECK.pptx", CurSlide = 2 };
        var output = new SongSettingsBase { Type = "P", Path = "deck.pptx", CurSlide = 7 };
        output.Slide[0, 3] = 2;
        Assert.True(FrmMain.IsPowerPointFile(preview, output.Path));
        Assert.False(FrmMain.IsPowerPointFile(preview, "other.pptx"));
        FrmMain.ApplyReloadedSlides(output, images, output.CurSlide, 3);
        FrmMain.ApplyReloadedSlides(preview, images, preview.CurSlide, 3);
        Assert.Equal((3, 3), (output.CurSlide, output.TotalSlides));
        Assert.Equal(2, output.Slide[0, 3]);
        Assert.Equal((2, 3), (preview.CurSlide, preview.TotalSlides));
        preview.Slide[3, 0] = -1;
        preview.SongVerses[1] = 0;
        Assert.Equal(1, output.Slide[3, 0]);
        Assert.Equal(3, output.SongVerses[1]);
    }

    // Set AXICULTUS_TEST_POWERPOINT_DESKTOP=1 on a Windows desktop with PowerPoint installed.
    private sealed class DesktopPowerPointTheoryAttribute : TheoryAttribute
    {
        public DesktopPowerPointTheoryAttribute()
        {
            if (Environment.GetEnvironmentVariable("AXICULTUS_TEST_POWERPOINT_DESKTOP") != "1")
                Skip = "Requires an interactive PowerPoint desktop.";
        }
    }

    [DesktopPowerPointTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void ExternallySavedDeckReplacesOnlyTheLiveCopyAndResumesAtTheNativePosition(bool editorOpenAtStart)
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            var dir = Directory.CreateTempSubdirectory("axicultus-live-ppt-");
            string path = Path.Combine(dir.FullName, "live.pptx");
            var wrapper = new PowerPoint();
            var copies = new ShowCopyList();
            wrapper.Init();
            var app = wrapper.prePowerPointApp;
            using var documents = app.Presentations;
            NetOffice.PowerPointApi._Presentation editor = documents.Add(MsoTriState.msoTrue);
            try
            {
                for (int i = 1; i <= 3; ++i)
                    editor.Slides.Add(i, PpSlideLayout.ppLayoutTitleOnly).Shapes.Title.TextFrame.TextRange.Text = $"original {i}";
                editor.SaveAs(path, PpSaveAsFileType.ppSaveAsOpenXMLPresentation);
                if (!editorOpenAtStart) wrapper.ClosePresentation(ref editor);
                Assert.Equal("", wrapper.Open(path, ref copies));
                wrapper.Run(path, ref copies);
                Assert.True(wrapper.LastRunStarted);
                wrapper.GotoSlide(3);
                string oldName = wrapper.GetPresentationName(path, copies);
                Assert.NotEqual(path, oldName, StringComparer.OrdinalIgnoreCase);
                Assert.Equal(3, wrapper.ReadRunSlideNumber());
                var mainWindow = new PowerPoint { prePowerPointApp = app };
                mainWindow.FollowRunOf(wrapper, path, copies);
                var opened = SlideImageManifest.ReadSource(path);
                Assert.Null(wrapper.ReloadChangedRun(path, copies, opened, () => true));
                string output = Path.Combine(dir.FullName, "~OUTPPPreview$test$");
                string preview = Path.Combine(dir.FullName, "~PREPPPreview$test$");
                var images = new SongSettingsBase();
                void ExportSlides() => Assert.True(wrapper.BuildScreenOutDumps(path, output, ref images.TotalSlides,
                    9, 1000, ref images.SongVerses, ref images.Slide, new string[10]));
                ExportSlides();
                var oldImage = File.ReadAllBytes(SlideImageManifest.ImagePath(preview, 3));

                editor ??= documents.Open(path, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoTrue);
                editor.Slides[3].Shapes.Title.TextFrame.TextRange.Text = "external edit";
                editor.Slides.Add(4, PpSlideLayout.ppLayoutBlank);
                editor.Save();
                var revision = SlideImageManifest.ReadSource(path);
                editor.Slides[3].Shapes.Title.TextFrame.TextRange.Text = "unsaved editor text";
                Assert.False(opened.Matches(revision));
                Assert.NotNull(wrapper.GetRunSourceVersion(path, copies));
                Assert.Null(wrapper.ReloadChangedRun(path, copies, revision, () => false));
                Assert.Equal(oldName, wrapper.GetPresentationName(path, copies));
                int checks = 0;
                Assert.Null(wrapper.ReloadChangedRun(path, copies, revision, () => ++checks < 3));
                Assert.Equal(oldName, wrapper.GetPresentationName(path, copies));
                Assert.Equal(3, wrapper.ReadRunSlideNumber());
                ExportSlides();
                Assert.Equal(4, images.TotalSlides);
                byte[] newImage = File.ReadAllBytes(SlideImageManifest.ImagePath(output, 3));
                Assert.False(oldImage.SequenceEqual(newImage));
                Assert.Equal(newImage, File.ReadAllBytes(SlideImageManifest.ImagePath(preview, 3)));
                Assert.Equal((3, 4), wrapper.ReloadChangedRun(path, copies, revision, () => true));
                Assert.NotEqual(oldName, wrapper.GetPresentationName(path, copies));
                Assert.Equal(3, wrapper.ReadRunSlideNumber());
                string currentName = wrapper.GetPresentationName(path, copies);
                using (var live = Enumerable.Range(1, documents.Count).Select(i => documents[i]).Single(p => p.FullName == currentName))
                    Assert.Equal("external edit", live.SlideShowWindow.View.Slide.Shapes.Title.TextFrame.TextRange.Text);
                Assert.Equal("unsaved editor text", editor.Slides[3].Shapes.Title.TextFrame.TextRange.Text);
                Assert.Equal(MsoTriState.msoFalse, editor.Saved);
                mainWindow.FollowRunOf(wrapper, path, copies);
                mainWindow.GotoSlide(4);
                Assert.Equal(4, wrapper.ReadRunSlideNumber());

                editor.Slides[4].Delete();
                editor.Slides[3].Delete();
                editor.Save();
                Assert.Equal((2, 2), wrapper.ReloadChangedRun(path, copies, SlideImageManifest.ReadSource(path), () => true));
                Assert.Equal(2, wrapper.ReadRunSlideNumber());
                Assert.Null(wrapper.ReloadChangedRun(path, copies, SlideImageManifest.ReadSource(path), () => true));
                Assert.Equal(2, editor.Slides.Count);
            }
            finally
            {
                wrapper.ClearUpPowerpointWindows(ref copies, quitWhenEmpty: false);
                wrapper.ClosePresentation(ref editor);
                wrapper.QuitPowerPointApp(app);
                dir.Delete(true);
            }
        }, timeoutSeconds: 120);
    }
}
