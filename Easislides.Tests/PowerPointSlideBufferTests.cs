using System;
using System.IO;
using Easislides.Module;
using OfficeLib;
using Xunit;

namespace Easislides.Tests;

public class PowerPointSlideBufferTests
{
    [Fact]
    public void CachedVerseMarkersIncludeTheLastSlideAndClearWhenTheFileChanges()
    {
        string path = Path.GetTempFileName();
        var wrapper = new PowerPoint();
        var song = new SongSettingsBase();
        try
        {
            const int lastSlide = 1001;
            var markers = new int[lastSlide + 1, 8];
            var verses = new int[10];
            PowerPoint.ResetVerses(9, 1000, ref verses, ref markers);
            markers[1000, 0] = 0; // Chorus
            markers[lastSlide, 0] = 7;
            verses[7] = lastSlide;
            var originalWriteTime = File.GetLastWriteTimeUtc(path);
            PowerPoint.StoreCachedVerses(path, originalWriteTime, true, lastSlide, 9, verses, markers);

            song.Slide[0, 3] = 2;
            Assert.True(wrapper.LoadVerses(path, 9, 1000, ref song.SongVerses, ref song.Slide, [], allowOpen: false));
            Assert.Equal(-1, song.Slide[999, 0]);
            Assert.Equal(0, song.Slide[1000, 0]);
            Assert.Equal(7, song.Slide[lastSlide, 0]);
            Assert.Equal(lastSlide, song.SongVerses[7]);
            Assert.Equal(2, song.Slide[0, 3]);

            File.SetLastWriteTimeUtc(path, originalWriteTime.AddMinutes(1));
            Assert.False(wrapper.LoadVerses(path, 9, 1000, ref song.SongVerses, ref song.Slide, [], allowOpen: false));
            Assert.Equal(-1, song.Slide[1000, 0]);
            Assert.Equal(-1, song.Slide[lastSlide, 0]);
            Assert.Equal(0, song.SongVerses[7]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ImageCacheSuppliesEnoughRowsEvenWhenNotesCannotBeOpened()
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            string path = Path.Combine(directory.FullName, "source.pptx");
            string prefix = Path.Combine(directory.FullName, "preview-");
            File.WriteAllText(path, "source identity");
            for (int slide = 1; slide <= 3; slide++)
                File.WriteAllText(SlideImageManifest.ImagePath(prefix, slide), "");
            SlideImageManifest.Commit(prefix, SlideImageManifest.ReadSource(path), 3);

            var song = new SongSettingsBase();
            var wrapper = new PowerPoint();
            Assert.True(wrapper.IsBuildedFileCheck(path, prefix, ref song.TotalSlides, ref song.Slide));
            Assert.Equal(3, song.TotalSlides);
            Assert.False(wrapper.LoadVerses(path, 9, 1000, ref song.SongVerses, ref song.Slide, [], allowOpen: false));
            for (int slide = 1; slide <= song.TotalSlides; slide++)
                Assert.Equal(-1, song.Slide[slide, 0]);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
