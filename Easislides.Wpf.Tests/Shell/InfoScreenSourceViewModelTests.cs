using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public sealed class InfoScreenSourceViewModelTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"EasiSlides_ISS_{Guid.NewGuid():N}");

    [Fact]
    public async Task Load_PopulatesSavedInfoScreenNames()
    {
        var store = new InfoScreenStore(_dir);
        await store.SaveAsync("환영", new InfoScreenDto("환영합니다"));
        await store.SaveAsync("광고", new InfoScreenDto("광고 안내"));
        var sut = CreateSut(store, out _);

        sut.Load();

        sut.Screens.Select(s => s.Name).Should().Equal("광고", "환영");
        sut.StatusText.Should().Contain("2");
    }

    [Fact]
    public async Task AddSelected_LoadsSavedTextAndFormatting()
    {
        var store = new InfoScreenStore(_dir);
        await store.SaveAsync(
            "주차 안내",
            new InfoScreenDto(
                "  예배 후 차량 이동 안내  ",
                FontSize: 60,
                Alignment: 2,
                ColorArgb: unchecked((int)0xFFFFFF00),
                BackgroundColorArgb: unchecked((int)0xFF000000),
                Bold: true,
                Italic: true,
                Underline: true,
                FontName: "맑은 고딕"));
        var sut = CreateSut(store, out var added);
        sut.Load();
        sut.SelectedScreen = sut.Screens.Single();

        await sut.AddSelectedCommand.ExecuteAsync(null);

        added.Should().ContainSingle();
        var selection = added[0];
        selection.Name.Should().Be("주차 안내");
        selection.Text.Should().Be("예배 후 차량 이동 안내");
        selection.Options.FontSizePt.Should().Be(60);
        selection.Options.Alignment.Should().Be(2);
        selection.Options.ColorArgb.Should().Be(unchecked((int)0xFFFFFF00));
        selection.Options.BackgroundColorArgb.Should().Be(unchecked((int)0xFF000000));
        selection.Options.Bold.Should().BeTrue();
        selection.Options.Italic.Should().BeTrue();
        selection.Options.Underline.Should().BeTrue();
        selection.Options.FontName.Should().Be("맑은 고딕");
    }

    [Fact]
    public async Task DeleteScreens_RemovesSelectedScreensAndReloadsList()
    {
        var store = new InfoScreenStore(_dir, settings: null, deleteFile: path =>
        {
            File.Delete(path);
            return true;
        });
        await store.SaveAsync("First notice", new InfoScreenDto("first"));
        await store.SaveAsync("Second notice", new InfoScreenDto("second"));
        await store.SaveAsync("Keep notice", new InfoScreenDto("keep"));
        var sut = CreateSut(store, out _);
        sut.Load();
        var selected = sut.Screens
            .Where(screen => screen.Name.Contains("notice", StringComparison.OrdinalIgnoreCase)
                && screen.Name != "Keep notice")
            .ToArray();

        var deleted = sut.DeleteScreens(selected);

        deleted.Should().Be(2);
        sut.Screens.Select(screen => screen.Name).Should().Equal("Keep notice");
        sut.StatusText.Should().Contain("2");
        (await store.LoadAsync("First notice")).Should().BeNull();
        (await store.LoadAsync("Second notice")).Should().BeNull();
    }

    [Fact]
    public async Task LoadSelectionAsync_RejectsEmptySavedText()
    {
        var store = new InfoScreenStore(_dir);
        await store.SaveAsync("빈 공지", new InfoScreenDto("   "));
        var sut = CreateSut(store, out var added);
        sut.Load();
        sut.SelectedScreen = sut.Screens.Single();

        var selection = await sut.LoadSelectionAsync();

        selection.Should().BeNull();
        added.Should().BeEmpty();
        sut.StatusText.Should().Contain("empty");
    }

    [Fact]
    public async Task LoadSelectionAsync_WithExplicitItem_LoadsThatSavedInfoScreen()
    {
        var store = new InfoScreenStore(_dir);
        await store.SaveAsync("첫 공지", new InfoScreenDto("첫 내용"));
        await store.SaveAsync("둘째 공지", new InfoScreenDto("둘째 내용"));
        var sut = CreateSut(store, out _);
        sut.Load();

        var selection = await sut.LoadSelectionAsync(sut.Screens.Single(screen => screen.Name == "둘째 공지"));

        selection.Should().NotBeNull();
        selection!.Name.Should().Be("둘째 공지");
        selection.Text.Should().Be("둘째 내용");
    }

    private static InfoScreenSourceViewModel CreateSut(InfoScreenStore store, out List<InfoScreenSelection> added)
    {
        var addedLocal = new List<InfoScreenSelection>();
        added = addedLocal;
        return new InfoScreenSourceViewModel(store, selection =>
        {
            addedLocal.Add(selection);
            return true;
        });
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, recursive: true);
        }
    }
}
