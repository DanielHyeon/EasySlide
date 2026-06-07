using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Easislides.Wpf.Library;
using Easislides.Wpf.Rendering;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class PowerPointLibraryViewModelTests
{
    private sealed class FakePptService : IPowerPointLibraryService
    {
        private readonly IReadOnlyList<string> _paths;
        private readonly IReadOnlyList<PowerPointFolderItem> _folders;

        public FakePptService(IReadOnlyList<PowerPointFolderItem>? folders = null, params string[] paths)
        {
            _paths = paths;
            _folders = folders ?? new[] { new PowerPointFolderItem("Powerpoint Items", @"C:\decks") };
        }

        public string? LastEnumeratedFolder { get; private set; }

        public IReadOnlyList<string> EnumeratePresentations(string folderPath, bool includeSubfolders) => _paths;

        public IReadOnlyList<PowerPointFolderItem> EnumerateFolders(string rootFolder) => _folders;
    }

    private sealed class FakeRenderService : IPowerPointRenderService
    {
        public List<PowerPointRenderRequest> Requests { get; } = new();

        public Task<PowerPointRenderResult> RenderSlideAsync(
            PowerPointRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var slide = new PowerPointSlideSnapshot(
                request.FilePath,
                request.SlideNumber,
                SlideCount: 4,
                request.PixelWidth,
                request.PixelHeight,
                ImageBytes: new byte[] { 1, 2, 3 },
                ContentType: "image/png",
                GeneratedAtUtc: System.DateTimeOffset.UnixEpoch);

            return Task.FromResult(new PowerPointRenderResult(
                PowerPointRenderErrorKind.None,
                slide,
                ErrorMessage: null,
                FromCache: false,
                Elapsed: System.TimeSpan.Zero));
        }

        public void ClearCache()
        {
        }
    }

    private static readonly ImageSource DummyImage = CreateDummyImage();

    private static PowerPointLibraryViewModel CreateSut(out List<string> added, params string[] paths)
        => CreateSut(out added, null, paths);

    private static PowerPointLibraryViewModel CreateSut(
        out List<string> added,
        IReadOnlyList<PowerPointFolderItem>? folders,
        params string[] paths)
        => CreateSut(out added, folders, paths, render: null, decoder: null);

    private static PowerPointLibraryViewModel CreateSut(
        out List<string> added,
        IReadOnlyList<PowerPointFolderItem>? folders,
        string[] paths,
        IPowerPointRenderService? render = null,
        Func<byte[], ImageSource>? decoder = null)
    {
        var addedLocal = new List<string>();
        added = addedLocal;
        return new PowerPointLibraryViewModel(
            new FakePptService(folders, paths),
            path => addedLocal.Add(path),
            initialFolder: @"C:\decks",
            render,
            decoder);
    }

    [Fact]
    public void Load_PopulatesPresentations()
    {
        var sut = CreateSut(out _, @"C:\decks\a.pptx", @"C:\decks\b.ppt");

        sut.LoadCommand.Execute(null);

        sut.Presentations.Select(p => p.FileName).Should().Equal("a.pptx", "b.ppt");
        sut.StatusText.Should().Contain("2");
    }

    [Fact]
    public async Task LoadThumbnailsAsync_RendersFirstSlideForVisiblePresentations()
    {
        var render = new FakeRenderService();
        var sut = CreateSut(
            out _,
            null,
            new[] { @"C:\decks\a.pptx", @"C:\decks\b.pptx" },
            render,
            _ => DummyImage);
        sut.LoadCommand.Execute(null);

        await sut.LoadThumbnailsAsync();

        render.Requests.Should().HaveCount(2);
        render.Requests.Select(r => r.FilePath).Should().Equal(@"C:\decks\a.pptx", @"C:\decks\b.pptx");
        render.Requests.Should().OnlyContain(r => r.SlideNumber == 1);
        render.Requests.Should().OnlyContain(r => r.PixelWidth == 1920 && r.PixelHeight == 1440);
        sut.Presentations.Should().OnlyContain(p => ReferenceEquals(p.ThumbnailImage, DummyImage));
        sut.Presentations.Should().OnlyContain(p => p.ThumbnailStatus == "슬라이드 1/4");
    }

    [Fact]
    public void Load_EmptyFolder_SetsEmptyStatus()
    {
        var sut = CreateSut(out _);

        sut.LoadCommand.Execute(null);

        sut.Presentations.Should().BeEmpty();
        sut.StatusText.Should().Contain("없습니다");
    }

    [Fact]
    public void AddSelected_WithSelection_InvokesCallback()
    {
        var sut = CreateSut(out var added, @"C:\decks\a.pptx");
        sut.LoadCommand.Execute(null);
        sut.SelectedFile = sut.Presentations[0];

        sut.AddSelectedCommand.Execute(null);

        added.Should().ContainSingle().Which.Should().Be(@"C:\decks\a.pptx");
    }

    [Fact]
    public void AddFiles_WithSeveralSelections_InvokesCallbackForEachInOrder()
    {
        var sut = CreateSut(out var added, @"C:\decks\a.pptx", @"C:\decks\b.ppt", @"C:\decks\c.pptx");
        sut.LoadCommand.Execute(null);

        var count = sut.AddFiles(new[] { sut.Presentations[0], sut.Presentations[2] });

        count.Should().Be(2);
        added.Should().Equal(@"C:\decks\a.pptx", @"C:\decks\c.pptx");
        sut.StatusText.Should().Contain("2개 PowerPoint");
    }

    [Fact]
    public void AddFiles_WithEmptySelection_DoesNotInvokeCallback()
    {
        var sut = CreateSut(out var added, @"C:\decks\a.pptx");

        var count = sut.AddFiles(Enumerable.Empty<PowerPointFileItem>());

        count.Should().Be(0);
        added.Should().BeEmpty();
    }

    [Fact]
    public void AddSelected_WithoutSelection_CannotExecute()
    {
        var sut = CreateSut(out var added, @"C:\decks\a.pptx");
        sut.LoadCommand.Execute(null);

        sut.AddSelectedCommand.CanExecute(null).Should().BeFalse();
        added.Should().BeEmpty();
    }

    [Fact]
    public void ListingStyle_DefaultsToList_AndCanSwitchToPreview()
    {
        var changed = new List<PowerPointListingStyle>();
        var sut = new PowerPointLibraryViewModel(
            new FakePptService(paths: new[] { @"C:\decks\a.pptx" }),
            _ => { },
            initialFolder: @"C:\decks",
            listingStyleChanged: changed.Add);

        sut.ListingStyle.Should().Be(PowerPointListingStyle.List);
        sut.IsListStyle.Should().BeTrue();
        sut.IsPreviewStyle.Should().BeFalse();
        sut.ListingStyleLabel.Should().Be("List");

        sut.UsePreviewStyleCommand.Execute(null);

        sut.ListingStyle.Should().Be(PowerPointListingStyle.Preview);
        sut.IsListStyle.Should().BeFalse();
        sut.IsPreviewStyle.Should().BeTrue();
        sut.ListingStyleLabel.Should().Be("Preview");
        changed.Should().ContainSingle().Which.Should().Be(PowerPointListingStyle.Preview);

        sut.UseListStyleCommand.Execute(null);
        sut.ListingStyle.Should().Be(PowerPointListingStyle.List);
        changed.Should().Equal(PowerPointListingStyle.Preview, PowerPointListingStyle.List);
    }

    [Fact]
    public void Constructor_CanStartInLegacyPreviewStyle()
    {
        var sut = new PowerPointLibraryViewModel(
            new FakePptService(paths: new[] { @"C:\decks\a.pptx" }),
            _ => { },
            initialFolder: @"C:\decks",
            initialListingStyle: PowerPointListingStyle.Preview);

        sut.ListingStyle.Should().Be(PowerPointListingStyle.Preview);
        sut.IsPreviewStyle.Should().BeTrue();
        sut.ListingStyleLabel.Should().Be("Preview");
    }

    [Fact]
    public void Constructor_BuildsFrmMainPowerpointFolderGroups_WithoutLoadingFiles()
    {
        var folders = new[]
        {
            new PowerPointFolderItem("Powerpoint Items", @"C:\decks"),
            new PowerPointFolderItem(@"\찬양", @"C:\decks\찬양"),
        };

        var sut = CreateSut(out _, folders, @"C:\decks\찬양\a.pptx");

        sut.FolderGroups.Select(f => f.DisplayName).Should().Equal("Powerpoint Items", @"\찬양");
        sut.SelectedFolder.Should().Be(folders[0]);
        sut.FolderPath.Should().Be(@"C:\decks");
        sut.IncludeSubfolders.Should().BeTrue("FrmMain Powerpoint folder contents include nested folders");
        sut.Presentations.Should().BeEmpty("constructor should not load files before the source tab is opened");
    }

    [Fact]
    public void SelectingPowerpointFolder_UpdatesFolderPath_AndReloadsList()
    {
        var folders = new[]
        {
            new PowerPointFolderItem("Powerpoint Items", @"C:\decks"),
            new PowerPointFolderItem(@"\찬양", @"C:\decks\찬양"),
        };
        var sut = CreateSut(out _, folders, @"C:\decks\찬양\a.pptx");

        sut.SelectedFolder = folders[1];

        sut.FolderPath.Should().Be(@"C:\decks\찬양");
        sut.Presentations.Select(p => p.FileName).Should().Equal("a.pptx");
    }

    [Fact]
    public void TogglingIncludeSubfolders_ReloadsList()
    {
        var sut = CreateSut(out _, @"C:\decks\a.pptx", @"C:\decks\sub\b.pptx");
        sut.Presentations.Should().BeEmpty("아직 Load 전");

        sut.IncludeSubfolders = false;

        sut.Presentations.Should().HaveCount(2);
    }

    [Fact]
    public void FilterText_NarrowsListByFileName_WithoutReloading()
    {
        var sut = CreateSut(out _, @"C:\decks\찬양.pptx", @"C:\decks\공지사항.pptx", @"C:\decks\intro.ppt");
        sut.LoadCommand.Execute(null);
        sut.Presentations.Should().HaveCount(3);

        sut.FilterText = "공지";

        sut.Presentations.Select(p => p.FileName).Should().Equal("공지사항.pptx");
        sut.StatusText.Should().Contain("1/3");
    }

    [Fact]
    public void FilterText_Cleared_RestoresFullList()
    {
        var sut = CreateSut(out _, @"C:\decks\a.pptx", @"C:\decks\b.pptx");
        sut.LoadCommand.Execute(null);
        sut.FilterText = "a";
        sut.Presentations.Should().HaveCount(1);

        sut.FilterText = "";

        sut.Presentations.Should().HaveCount(2, "검색어 지우면 전체 복원");
    }

    [Fact]
    public void FilterText_ExcludingSelected_DeselectsIt_AndDisablesAdd()
    {
        // 선택한 PPT 가 검색에 걸러져 사라지면 선택을 해제한다 — 숨은 항목이 선택된 채 추가되는 사고 방지.
        var sut = CreateSut(out var added, @"C:\decks\a.pptx", @"C:\decks\b.pptx");
        sut.LoadCommand.Execute(null);
        sut.SelectedFile = sut.Presentations.First(p => p.FileName == "a.pptx");

        sut.FilterText = "b";

        sut.SelectedFile.Should().BeNull("걸러져 사라진 항목은 선택 해제");
        sut.AddSelectedCommand.CanExecute(null).Should().BeFalse();
        added.Should().BeEmpty();
    }

    private static ImageSource CreateDummyImage()
    {
        var image = new DrawingImage(new GeometryDrawing(
            Brushes.Transparent,
            null,
            new RectangleGeometry(new System.Windows.Rect(0, 0, 1, 1))));
        image.Freeze();
        return image;
    }
}
