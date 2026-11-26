using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Library;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Library;

public class SongEditorViewModelTests
{
    [Fact]
    public void Load_WithExistingSong_PopulatesFieldsAndTracksChanges()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var folder = new SongFolderSummary(3, "Evening", IsEnabled: true, SongCount: 1);
        var song = Song(31, "Amazing Grace", folderNo: 3, alternateTitle: "나 같은 죄인", category: "Opening", key: "G", lyrics: "Amazing grace");
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());

        sut.Load(fixture.AdminDatabasePath, folder, song);

        sut.IsNew.Should().BeFalse();
        sut.SongId.Should().Be(31);
        sut.FolderNo.Should().Be(3);
        sut.FolderName.Should().Be("Evening");
        sut.Title.Should().Be("Amazing Grace");
        sut.AlternateTitle.Should().Be("나 같은 죄인");
        sut.Category.Should().Be("Opening");
        sut.Key.Should().Be("G");
        sut.Lyrics.Should().Be("Amazing grace");
        sut.HasChanges.Should().BeFalse();

        sut.Title = "Amazing Grace Revised";

        sut.HasChanges.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_WhenExistingSongIsValid_UsesRepositoryAndConfiguredBackupRoot()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        fixture.Settings.Set(AxicultusSettingKeys.DataBackupRoot, fixture.BackupRoot);
        var repository = new FakeAdminDatabaseRepository();
        var folder = new SongFolderSummary(2, "Morning", IsEnabled: true, SongCount: 1);
        repository.Details[10] = Detail(10, "Old", folderNo: 2);
        var sut = new SongEditorViewModel(fixture.Settings, repository);
        await sut.LoadAsync(fixture.AdminDatabasePath, folder, Song(10, "Old", folderNo: 2));
        sut.Title = "  Revised  ";
        sut.AlternateTitle = "  Alt  ";
        sut.Category = "  Communion  ";
        sut.Key = "  D  ";
        sut.Lyrics = "Line 1\r\nLine 2";

        await sut.SaveAsync();

        repository.LastDatabasePath.Should().Be(fixture.AdminDatabasePath);
        repository.LastBackupRoot.Should().Be(fixture.BackupRoot);
        repository.LastSong.Should().NotBeNull();
        repository.LastSong!.SongId.Should().Be(10);
        repository.LastSong.Title.Should().Be("Revised");
        repository.LastSong.AlternateTitle.Should().Be("Alt");
        repository.LastSong.FolderNo.Should().Be(2);
        repository.LastSong.Category.Should().Be("Communion");
        repository.LastSong.Key.Should().Be("D");
        repository.LastSong.Lyrics.Should().Be("Line 1\nLine 2", "FrmEditItem stores the lyric lines with \\n");
        sut.StatusMessage.Should().Be("저장되었습니다.");
        sut.ValidationMessage.Should().Be("");
        sut.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void Preview_UsesFormatDataColors_WhenPresent()
    {
        // 곡의 FormatData(레거시 v32)에 글자/배경색이 있으면 미리보기가 그 색(region1)을 쓴다.
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);

        sut.FormatData = "29=-16777216>26=-1>"; // 글자색 검정, 배경색 흰색.

        sut.PreviewForegroundHex.Should().Be("#FF000000");
        sut.PreviewBackgroundHex.Should().Be("#FFFFFFFF");
    }

    [Fact]
    public void Transpose_ShiftsPreviewChords_WithoutChangingLyrics()
    {
        // 조옮김(±반음)은 미리보기 코드만 옮기고 저장 가사(Lyrics)는 바꾸지 않는다(레거시 Transpose Semi-Tone).
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);
        sut.Lyrics = "Amazing grace » G  C";
        var originalLyrics = sut.Lyrics;

        sut.TransposeUpCommand.Execute(null); // +1 반음
        sut.TransposeUpCommand.Execute(null); // +2 반음

        sut.TransposeLabel.Should().Be("+2");
        sut.PreviewLyricLines.Should().Contain(line => line.Notation.Contains("A") && line.Notation.Contains("D"),
            "G→A, C→D 로 미리보기 코드가 +2 반음 이동");
        sut.Lyrics.Should().Be(originalLyrics, "저장 가사는 불변(미리보기 전용)");

        sut.ResetTransposeCommand.Execute(null);
        sut.TransposeLabel.Should().Be("원조");
    }

    [Fact]
    public void ToCapoZero_TransposesUpByCapoAmount_AndShowsTransposedKey()
    {
        // To Capo 0 = 카포(2)만큼 올려 카포 없이 치는 실제 코드/키를 보여 준다. 미리보기 메타에 옮긴 키도 표시.
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);
        sut.Key = "G";
        sut.Capo = 2;
        sut.Lyrics = "Amazing » G";

        sut.ToCapoZeroCommand.Execute(null);

        sut.TransposeSemitones.Should().Be(2, "카포 2 → +2 반음");
        sut.TransposeLabel.Should().Be("+2");
        sut.PreviewMetadata.Should().Contain("키 G → A", "옮긴 키 표시");
    }

    [Fact]
    public void ToCapoZero_DisabledWhenNoCapo()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);

        sut.Capo = 0;

        sut.ToCapoZeroCommand.CanExecute(null).Should().BeFalse("카포가 없으면 To Capo 0 비활성");
    }

    [Fact]
    public void Lyrics_DuplicateSectionLabels_ShowsSectionWarning_NonBlocking()
    {
        // Sequence 모델: 절은 1회 정의. 같은 라벨([C]) 두 번 정의 시 비차단 경고를 띄운다(저장은 막지 않음).
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);

        sut.LyricsRegion1 = "[1]\nVerse\n[chorus]\nChorus\n[chorus]\nDup chorus";

        sut.SectionWarning.Should().Contain("chorus").And.Contain("중복").And.Contain("한 절로 합쳐져 표시됩니다");
    }

    [Fact]
    public void Lyrics_NoDuplicateSectionLabels_ClearsSectionWarning()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);

        sut.Lyrics = "[1]\nVerse\n[C]\nChorus\n[2]\nVerse two";

        sut.SectionWarning.Should().BeEmpty("중복 라벨이 없으면 경고 없음");
    }

    [Fact]
    public async Task SaveAsync_WhenTitleMissing_ShowsValidationAndDoesNotWrite()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var sut = new SongEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);
        sut.Title = "   ";

        await sut.SaveAsync();

        sut.ValidationMessage.Should().Be("제목을 입력하세요.");
        repository.LastSong.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_WhenCapoBelowNoCapo_ShowsKoreanValidation_AndDoesNotWrite()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var sut = new SongEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);
        sut.Title = "은혜";
        sut.Lyrics = "가사";
        sut.Capo = -2;

        await sut.SaveAsync();

        sut.ValidationMessage.Should().Be("카포는 -1(없음)보다 작을 수 없습니다.", "이식된 곡 편집기의 검증 메시지도 한글이어야 함");
        repository.LastSong.Should().BeNull("검증 실패 시 저장하지 않는다");
    }

    [Fact]
    public async Task SaveAsync_NewSong_KeepsNoNumberAndNoCapo()
    {
        // WinForms stores "no number" as 0 and "no capo" as -1; a folder that already holds songs must not change that.
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var sut = new SongEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 7), null);
        sut.Title = "은혜";
        sut.Lyrics = "가사";

        await sut.SaveAsync();

        sut.ValidationMessage.Should().Be("");
        repository.LastSong!.SongNumber.Should().Be(0);
        repository.LastSong.Capo.Should().Be(-1);
    }

    [Fact]
    public async Task SaveAsync_WhenBackupRootNotConfigured_UsesDatabaseSiblingBackupsFolder()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var sut = new SongEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);
        sut.Title = "New Song";
        sut.Lyrics = "Lyrics";

        await sut.SaveAsync();

        repository.LastSong.Should().NotBeNull();
        repository.LastSong!.SongId.Should().BeNull();
        repository.LastBackupRoot.Should().Be(Path.Combine(fixture.Root, "Backups"));
        sut.SongId.Should().Be(44);
        sut.IsNew.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_WithExistingSong_LoadsDetailFieldsAndBuildsPreview()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        repository.Details[10] = Detail(
            10,
            "Detailed",
            folderNo: 2,
            lyrics: "Verse one\r\nLine two\r\n\r\nVerse two",
            sequence: "\u0001\u0002",
            writer: "Writer",
            copyright: "Copyright",
            capo: 2,
            timing: "4/4",
            key: "D",
            notations: "(0;C;1;)",
            category: "Praise",
            licenceAdmin1: "CCLI",
            licenceAdmin2: "Admin2",
            bookReference: "Book 12",
            userReference: "User 7",
            settings: "legacy-settings",
            formatData: "legacy-format");
        fixture.Settings.Set(AxicultusSettingKeys.LyricsMonitorTextColorArgb, unchecked((int)0xFF102030));
        fixture.Settings.Set(AxicultusSettingKeys.LyricsMonitorBackgroundColorArgb, unchecked((int)0xFFEFE8DD));
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, null, null);

        await sut.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(2, "Morning", true, 1), Song(10, "Summary", folderNo: 2));

        sut.Title.Should().Be("Detailed");
        sut.Sequence.Should().Be("1,2", "WinForms verse codes are shown as readable tokens");
        sut.Writer.Should().Be("Writer");
        sut.Copyright.Should().Be("Copyright");
        sut.Capo.Should().Be(2);
        sut.Timing.Should().Be("4/4");
        sut.Notations.Should().Be("(0;C;1;)");
        sut.LyricsRegion1.Should().Be(" C »\nVerse one\nLine two\n\nVerse two", "the chord is shown above the character it is stored for");
        sut.LicenceAdmin1.Should().Be("CCLI");
        sut.LicenceAdmin2.Should().Be("Admin2");
        sut.BookReference.Should().Be("Book 12");
        sut.UserReference.Should().Be("User 7");
        sut.SongSettingsData.Should().Be("legacy-settings");
        sut.FormatData.Should().Be("legacy-format");
        sut.PreviewTitle.Should().Be("Detailed");
        sut.PreviewLyrics.Should().Contain("Verse one").And.Contain("Line two");
        sut.PreviewMetadata.Should().Contain("D").And.Contain("4/4").And.Contain("Writer");
        sut.PreviewFormatStatus.Should().Contain("포맷").And.Contain("코드").And.Contain("절");
        sut.PreviewForegroundHex.Should().Be("#FF102030");
        sut.PreviewBackgroundHex.Should().Be("#FFEFE8DD");
        sut.PreviewFontFamilyOptions.Should().Contain("Malgun Gothic");
        sut.PreviewMainFontSize.Should().Be(18);
        sut.PreviewNotationFontSize.Should().Be(14);
        sut.HasChanges.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_DecodesRegionStyleInspectorFromFormatData()
    {
        // FormatData 의 영역별 코드(29/30 색·31/32 정렬·41 효과비트)가 인스펙터 속성으로 디코드돼야 한다.
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        // 41=39 = 1|2|4|32 = region1 Bold+Italic+Underline, region2 Underline.
        repository.Details[10] = Detail(10, "Styled", folderNo: 2,
            formatData: "29=-16777216>30=-1>31=2>32=1>41=39>43=Arial>47=40>");
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, null, null);

        await sut.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(2, "Morning", true, 1), Song(10, "Summary", folderNo: 2));

        // 인스펙터 편집 대상(색·정렬·굵게·기울임·밑줄)이 디코드된다.
        sut.Region1ColorHex.Should().Be("#FF000000");
        sut.Region2ColorHex.Should().Be("#FFFFFFFF");
        sut.Region1Alignment.Should().Be(2);
        sut.Region2Alignment.Should().Be(1);
        sut.Region1Bold.Should().BeTrue();       // bit0
        sut.Region1Italic.Should().BeTrue();      // bit1
        sut.Region1Underline.Should().BeTrue();   // bit2
        sut.Region2Bold.Should().BeFalse();       // bit3
        sut.Region2Italic.Should().BeFalse();     // bit4
        sut.Region2Underline.Should().BeTrue();   // bit5
        sut.HasChanges.Should().BeFalse("로드는 변경이 아니다");
    }

    [Fact]
    public async Task ChangingRegionUnderline_EncodesIntoFormatData()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        repository.Details[10] = Detail(10, "Styled", folderNo: 2, formatData: "29=-1>");
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, null, null);
        await sut.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(2, "Morning", true, 1), Song(10, "Summary", folderNo: 2));

        sut.Region1Underline = true;

        sut.HasChanges.Should().BeTrue();
        SongFormatData.Parse(sut.FormatData)!.Underline1.Should().BeTrue();
    }

    [Fact]
    public void Load_NewSong_RegionStyleInspectorAllInherit()
    {
        using var fixture = TempSongEditorSettings.Create();
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());

        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(2, "Morning", true, 1), song: null);

        sut.Region1ColorHex.Should().BeEmpty();
        sut.Region2ColorHex.Should().BeEmpty();
        sut.Region1Alignment.Should().Be(0); // 상속
        sut.Region2Alignment.Should().Be(0);
        sut.FormatData.Should().BeEmpty();
        sut.HasChanges.Should().BeFalse();
    }

    [Fact]
    public async Task ChangingRegionStyle_EncodesIntoFormatData_PreservingOtherFields_AndMarksDirty()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        // 폰트(43/47)는 인스펙터가 안 건드리는 보존 대상.
        repository.Details[10] = Detail(10, "Styled", folderNo: 2, formatData: "29=-16777216>43=Arial>47=40>");
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, null, null);
        await sut.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(2, "Morning", true, 1), Song(10, "Summary", folderNo: 2));
        sut.HasChanges.Should().BeFalse();

        sut.Region2Alignment = 3; // 보조 언어 정렬 = 오른쪽.
        sut.Region2Italic = true; // 보조 언어 기울임 켜기(이제 출력이 영역별로 렌더).

        sut.HasChanges.Should().BeTrue();
        var format = SongFormatData.Parse(sut.FormatData)!;
        format.Alignment2.Should().Be(3, "새로 정한 영역2 정렬");
        format.Italic2.Should().BeTrue("새로 켠 영역2 기울임");
        format.TextColorArgb1.Should().Be(-16777216, "기존 영역1 색 보존");
        format.FontName1.Should().Be("Arial", "인스펙터가 안 건드린 폰트 보존");
        format.FontSize1.Should().Be(40, "인스펙터가 안 건드린 크기 보존");
    }

    [Fact]
    public async Task ClearingRegionColor_DropsColorCode_ButKeepsPreservedFontAndBackground()
    {
        // 색을 비우면(상속) 그 코드만 빠지고, 인스펙터가 안 건드리는 폰트(43)·배경 이미지(61)는 그대로 유지된다.
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        repository.Details[10] = Detail(10, "Styled", folderNo: 2, formatData: @"29=-1>43=Arial>61=C:\bg.jpg>");
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, null, null);
        await sut.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(2, "Morning", true, 1), Song(10, "Summary", folderNo: 2));

        sut.Region1ColorHex = ""; // 영역1 색을 상속으로.

        var format = SongFormatData.Parse(sut.FormatData)!;
        format.TextColorArgb1.Should().BeNull("색을 비우면 29 코드가 빠진다");
        format.FontName1.Should().Be("Arial", "폰트 보존");
        format.BackgroundImagePath.Should().Be(@"C:\bg.jpg", "배경 이미지 보존");
    }

    [Fact]
    public async Task SaveAsync_WhenExistingSongLoadedFromDetail_PreservesLegacyFields()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        repository.Details[10] = Detail(
            10,
            "Detailed",
            folderNo: 2,
            lyrics: "Original",
            sequence: "\u0001",
            writer: "Writer",
            copyright: "Copyright",
            capo: 3,
            timing: "6/8",
            key: "E",
            notations: "(0;D;2;)",
            category: "Offering",
            licenceAdmin1: "Lic1",
            licenceAdmin2: "Lic2",
            bookReference: "Book",
            userReference: "User",
            settings: "settings",
            formatData: "format");
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, null, null);
        await sut.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(2, "Morning", true, 1), Song(10, "Summary", folderNo: 2));
        sut.Title = "Revised";
        sut.Lyrics = "Changed";

        await sut.SaveAsync();

        repository.LastSong.Should().NotBeNull();
        repository.LastSong!.Title.Should().Be("Revised");
        repository.LastSong.Lyrics.Should().Be("Changed");
        repository.LastSong.Sequence.Should().Be("1", "the repository stores the tokens as WinForms verse codes");
        repository.LastSong.Writer.Should().Be("Writer");
        repository.LastSong.Copyright.Should().Be("Copyright");
        repository.LastSong.Capo.Should().Be(3);
        repository.LastSong.Timing.Should().Be("6/8");
        repository.LastSong.Key.Should().Be("E");
        repository.LastSong.Notations.Should().Be("(0;D;2;)");
        repository.LastSong.Category.Should().Be("Offering");
        repository.LastSong.LicenceAdmin1.Should().Be("Lic1");
        repository.LastSong.LicenceAdmin2.Should().Be("Lic2");
        repository.LastSong.BookReference.Should().Be("Book");
        repository.LastSong.UserReference.Should().Be("User");
        repository.LastSong.Settings.Should().Be("settings");
        repository.LastSong.FormatData.Should().Be("format");
    }

    [Fact]
    public async Task SaveAsync_WhenLiveSessionActiveAndPromptDeclines_DoesNotWrite()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var liveSession = new FakeLiveSessionService
        {
            Current = new LiveSessionSnapshot(LiveState.Active, "Live song", "Monitor 2", IsBlackout: false, "Song")
        };
        var prompt = new FakeLiveSafetyPrompt { NextResult = false };
        repository.Details[10] = Detail(10, "Old", folderNo: 1, lyrics: "Lyrics");
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, liveSession, prompt);
        await sut.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), Song(10, "Old", folderNo: 1));
        sut.Title = "Revised";

        await sut.SaveAsync();

        prompt.LastRequest.Should().NotBeNull();
        prompt.LastRequest!.ActionName.Should().Be("라이브 중 곡 저장");
        repository.LastSong.Should().BeNull();
        sut.IsLiveEditWarningVisible.Should().BeTrue();
        sut.StatusMessage.Should().Contain("취소");
    }

    [Fact]
    public async Task SaveAsync_WhenLiveAndSafetyPromptUnavailable_ShowsKoreanStatus_AndDoesNotWrite()
    {
        // 라이브 중인데 안전 확인 수단(prompt)이 없으면 저장을 막고, 한글로 안내한다(이식분 메시지 한글화).
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var liveSession = new FakeLiveSessionService
        {
            Current = new LiveSessionSnapshot(LiveState.Active, "Live song", "Monitor 2", IsBlackout: false, "Song")
        };
        repository.Details[10] = Detail(10, "Old", folderNo: 1, lyrics: "Lyrics");
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, liveSession, liveSafetyPrompt: null);
        await sut.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), Song(10, "Old", folderNo: 1));
        sut.Title = "Revised";

        await sut.SaveAsync();

        repository.LastSong.Should().BeNull("안전 확인 불가 시 저장하지 않는다");
        sut.StatusMessage.Should().Contain("사용할 수 없습니다", "한글 안내");
    }

    [Fact]
    public async Task SaveAsync_WhenLiveSessionActiveAndPromptApproves_Writes()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var liveSession = new FakeLiveSessionService
        {
            Current = new LiveSessionSnapshot(LiveState.Hidden, "Live song", "Monitor 2", IsBlackout: true, "Song")
        };
        var prompt = new FakeLiveSafetyPrompt { NextResult = true };
        repository.Details[10] = Detail(10, "Old", folderNo: 1, lyrics: "Lyrics");
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, liveSession, prompt);
        await sut.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), Song(10, "Old", folderNo: 1));
        sut.Title = "Revised";

        await sut.SaveAsync();

        prompt.LastRequest.Should().NotBeNull();
        repository.LastSong.Should().NotBeNull();
        repository.LastSong!.Title.Should().Be("Revised");
    }

    // ── 1. A song whose stored fields could not be read is not saveable ───────────────────────────

    public static TheoryData<Exception> DetailFailures { get; } = new()
    {
        new InvalidOperationException("AdminDB schema is not compatible"),
        new System.Data.SQLite.SQLiteException("database is locked"),
        new IOException("disk error"),
    };

    [Theory]
    [MemberData(nameof(DetailFailures))]
    public async Task LoadAsync_WhenTheDetailCannotBeRead_SaveIsOff_AndNothingIsWritten(Exception failure)
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository { DetailFailure = failure };
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, null, null);

        await sut.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(2, "Morning", true, 1), Song(10, "Old", folderNo: 2, lyrics: "Lyrics"));

        sut.SaveBlockedReason.Should().Contain("저장할 수 없습니다").And.Contain(failure.Message);
        sut.SaveCommand.CanExecute(null).Should().BeFalse("the sequence, MSC, settings and format were not read and would be saved empty");
        sut.Title = "Changed";
        sut.SaveCommand.CanExecute(null).Should().BeFalse();
        await sut.SaveAsync();
        repository.LastSong.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_WhenTheSongRowIsGone_SaveIsOff()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, null, null);

        await sut.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(2, "Morning", true, 1), Song(10, "Old", folderNo: 2, lyrics: "Lyrics"));

        sut.SaveBlockedReason.Should().Contain("찾을 수 없어");
        sut.SaveCommand.CanExecute(null).Should().BeFalse();
        await sut.SaveAsync();
        repository.LastSong.Should().BeNull();
    }

    [Fact]
    public async Task Load_ExistingSongWithoutItsDetail_SaveIsOff_AndANewSongIsNot()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var sut = new SongEditorViewModel(fixture.Settings, repository);

        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(2, "Morning", true, 1), Song(10, "Old", folderNo: 2, lyrics: "Lyrics"));

        sut.SaveBlockedReason.Should().NotBeEmpty();
        await sut.SaveAsync();
        repository.LastSong.Should().BeNull();

        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(2, "Morning", true, 1), null);

        sut.SaveBlockedReason.Should().BeEmpty();
    }

    // ── 2. Chords follow their lyric lines ─────────────────────────────────────────────────────

    [Fact]
    public async Task Chords_StayWithTheirLines_WhenLyricLinesAreInsertedOrRemoved()
    {
        using var fixture = TempSongEditorSettings.Create();
        var repository = new FakeAdminDatabaseRepository();
        var sut = await OpenAsync(fixture, repository, Detail(10, "Song", folderNo: 2,
            lyrics: "Line one\nLine two\nLine three", notations: "(1;G;0;)(2;D;5;)"));
        sut.LyricsRegion1.Should().Be("Line one\nG »\nLine two\n     D »\nLine three");
        sut.Notations.Should().Be("(1;G;0;)(2;D;5;)");
        sut.HasChanges.Should().BeFalse();

        sut.LyricsRegion1 = "New first line\r\n" + sut.LyricsRegion1;

        sut.Lyrics.Should().Be("New first line\nLine one\nLine two\nLine three");
        sut.Notations.Should().Be("(2;G;0;)(3;D;5;)", "the records name the lines the chords are over now");

        sut.LyricsRegion1 = sut.LyricsRegion1.Replace("Line one\n", "");
        await sut.SaveAsync();

        repository.LastSong!.Lyrics.Should().Be("New first line\nLine two\nLine three");
        repository.LastSong.Notations.Should().Be("(1;G;0;)(2;D;5;)");
    }

    // ── 3. Capo ───────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Capo_NoCapoIsAnEntryOfTheList_AndIsStoredAsMinusOne()
    {
        using var fixture = TempSongEditorSettings.Create();
        var repository = new FakeAdminDatabaseRepository();
        var sut = await OpenAsync(fixture, repository, Detail(10, "Song", folderNo: 2, lyrics: "Lyrics", capo: -1));

        // FrmEditItem: "" then "Capo 0" to "Capo 11"; it stores SelectedIndex - 1.
        sut.CapoOptions.Select(option => option.Key).Should().Equal(Enumerable.Range(-1, 13));
        sut.CapoOptions[0].Value.Should().Be("없음");
        sut.CapoOptions[1].Value.Should().Be("카포 0");
        sut.CapoOptions[12].Value.Should().Be("카포 11");
        sut.Capo.Should().Be(-1);

        await sut.SaveAsync();
        repository.LastSong!.Capo.Should().Be(-1);

        sut.Capo = sut.CapoOptions[1].Key;
        await sut.SaveAsync();
        repository.LastSong!.Capo.Should().Be(0, "Capo 0 is a capo setting of its own");
    }

    [Theory]
    [InlineData(-2)] // FrmEditItem wrote it for a new song whose capo list was never touched
    [InlineData(-7)]
    public async Task Capo_AnyNegativeStoredCapoIsNoCapo_AndIsSavedAsMinusOne(int stored)
    {
        using var fixture = TempSongEditorSettings.Create();
        var repository = new FakeAdminDatabaseRepository();
        var sut = await OpenAsync(fixture, repository, Detail(10, "Song", folderNo: 2, lyrics: "Lyrics", capo: stored));

        sut.Capo.Should().Be(-1, "the list shows 없음");
        sut.HasChanges.Should().BeFalse();
        await sut.SaveAsync();

        sut.ValidationMessage.Should().BeEmpty();
        repository.LastSong!.Capo.Should().Be(-1, "FrmEditItem stores -1 when it saves such a song again");
    }

    // ── 4. Folder ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Folders_ListTheFoldersInUse_AndSavingMovesTheSong()
    {
        using var fixture = TempSongEditorSettings.Create();
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.AddRange(
        [
            new SongFolderSummary(0, "Deleted", true, 3),
            new SongFolderSummary(1, "Morning", true, 5),
            new SongFolderSummary(2, "Evening", true, 1),
            new SongFolderSummary(3, "Unused", false, 0),
            new SongFolderSummary(41, "Beyond the WinForms folders", true, 0),
        ]);
        SongEditorSavedEventArgs? saved = null;
        var sut = await OpenAsync(fixture, repository, Detail(10, "Song", folderNo: 2, lyrics: "Lyrics"));
        sut.Saved += (_, e) => saved = e;

        sut.Folders.Select(folder => folder.FolderNo).Should().Equal(1, 2);
        sut.SelectedFolder!.FolderNo.Should().Be(2);
        sut.FolderName.Should().Be("Evening", "the name comes from the database, not from the list the editor was opened from");
        sut.HasChanges.Should().BeFalse();

        sut.SelectedFolder = sut.Folders[0];

        sut.HasChanges.Should().BeTrue();
        sut.FolderName.Should().Be("Morning");
        await sut.SaveAsync();
        repository.LastSong!.FolderNo.Should().Be(1);
        saved.Should().Be(new SongEditorSavedEventArgs(10, 1));
    }

    [Fact]
    public async Task Folders_KeepTheSongsOwnFolder_WhenItIsNotInUse()
    {
        using var fixture = TempSongEditorSettings.Create();
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.AddRange([new SongFolderSummary(1, "Morning", true, 5), new SongFolderSummary(3, "Unused", false, 1)]);
        var sut = await OpenAsync(fixture, repository, Detail(10, "Song", folderNo: 3, lyrics: "Lyrics"));

        sut.Folders.Select(folder => folder.FolderNo).Should().Equal(1, 3);
        sut.SelectedFolder!.FolderNo.Should().Be(3);
        await sut.SaveAsync();

        repository.LastSong!.FolderNo.Should().Be(3, "saving must not move the song by itself");
    }

    [Fact]
    public async Task Folders_WhenNoneIsInUse_OfferFolderOne_AndWhenTheListCannotBeRead_TheOpenedFolder()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.AddRange([new SongFolderSummary(1, "Songs", false, 0), new SongFolderSummary(2, "Other", false, 0)]);
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, null, null);

        await sut.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(1, "Songs", false, 0), null);

        sut.Folders.Select(folder => folder.FolderNo).Should().Equal(1);

        repository.FoldersFailure = new System.Data.SQLite.SQLiteException("database is locked");
        await sut.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(2, "Other", false, 0), null);

        sut.Folders.Select(folder => folder.FolderNo).Should().Equal(2);
        sut.SelectedFolder!.Name.Should().Be("Other");
        sut.SaveBlockedReason.Should().BeEmpty("a new song has no stored fields to lose");
    }

    // ── 5. Region 2 ────────────────────────────────────────────────────────────────────────
    // The same cases and expected values as Axicultuss.Tests/SongEditorSaveTests.cs, which runs FrmEditItem's code.

    [Theory]
    [InlineData("Line 1\nLine 2", "", "", "", "Line 1\nLine 2", "")]
    [InlineData("Line 1\r\nLine 2\r\n", "", "", "", "Line 1\nLine 2", "")]
    [InlineData("Amazing grace\nHow sweet", "(0;G;0;C;8;)(1;D;4;)", "", "", "Amazing grace\nHow sweet", "(0;G;0;C;8;)(1;D;4;)")]
    [InlineData("[1]\nverse a\n\nverse b\n[chorus]\nchorus a\n[2]\nverse two", "(1;G;0;)(5;C;2;)", "", "",
        "[1]\nverse a\n\nverse b\n[chorus]\nchorus a\n[2]\nverse two", "(1;G;0;)(5;C;2;)")]
    [InlineData("[chorus]\nchorus a\n[1]\nverse a", "(1;C;0;)(3;G;1;)", "", "", "[chorus]\nchorus a\n[1]\nverse a", "(1;C;0;)(3;G;1;)")]
    [InlineData("[1]\nverse a\n[chorus]\nchorus a", "(1;G;0;)", "[1]\nsecond a\n[chorus]\nsecond c\n[2]\nonly second", "(1;Am;2;)(5;F;0;)",
        "[1]\nverse a\n[region 2]\nsecond a\n[chorus]\nchorus a\n[region 2]\nsecond c\n[2]\n[region 2]\nonly second", "(1;G;0;)(3;Am;2;)(10;F;0;)")]
    [InlineData("first a\nfirst b", "(1;G;0;)", "second a", "(0;Am;2;)", "first a\nfirst b\n[region 2]\nsecond a", "(1;G;0;)(3;Am;2;)")]
    [InlineData("one\ntwo", "", "[1]\nuno\ndos", "", "[1]\none\ntwo\n[region 2]\nuno\ndos", "")]
    // Verses that only the second pane has follow in the order of that pane, under their own headings.
    [InlineData("", "", "[chorus]\nbar\n[1]\nfoo", "", "[chorus]\n[region 2]\nbar\n[1]\n[region 2]\nfoo", "")]
    [InlineData("[chorus]\n[1]", "", "[chorus]\nbar\n[1]\nfoo", "(1;Am;0;)", "[chorus]\n[region 2]\nbar\n[1]\n[region 2]\nfoo", "(2;Am;0;)")]
    [InlineData("[1]\na", "", "[1]\nx\n[bridge]\nbb\n[2]\ny", "", "[1]\na\n[region 2]\nx\n[bridge]\n[region 2]\nbb\n[2]\n[region 2]\ny", "")]
    [InlineData("", "", "foo\nbar", "", "[region 2]\nfoo\nbar", "")]
    // A verse with nothing but empty lines in the second pane gets no region marker.
    [InlineData("[1]\na\n[2]\nb", "", "[1]\n\n", "", "[1]\na\n[2]\nb", "")]
    // A heading of the first pane places its verse even when all the lines of that verse are in the second pane.
    [InlineData("[1]\nzh\n[2]\n[chorus]\nzc\n[3]", "(1;G;0;)", "[1]\npy\n[2]\nen\n[chorus]\npc\n[3]\nec", "(3;D;1;)",
        "[1]\nzh\n[region 2]\npy\n[2]\n[region 2]\nen\n[chorus]\nzc\n[region 2]\npc\n[3]\n[region 2]\nec", "(1;G;0;)(6;D;1;)")]
    [InlineData("[1]\na\n\n\n\nb\n\n[2]\n\nc\n\n", "", "", "", "[1]\na\n\nb\n\n[2]\nc", "")]
    [InlineData("[1]\n[2]\nb", "", "", "", "[2]\nb", "")]
    [InlineData("[1]\na\n[bridge]\nb\n[ending]\ne\n[prechorus]\np\n[chorus 2]\nt", "", "[bridge 2]\nw", "",
        "[1]\na\n[bridge]\nb\n[ending]\ne\n[prechorus]\np\n[chorus 2]\nt\n[bridge 2]\n[region 2]\nw", "")]
    [InlineData("  ", "", "", "", "  ", "")]
    [InlineData("", "", "", "", "", "")]
    public void TheTwoLyricPanes_AreStoredAsFrmEditItemStoresThem(string first, string firstChords, string second, string secondChords,
        string lyrics, string chords)
    {
        SongEditorLyrics.Merge(first, firstChords, second, secondChords).Should().Be((lyrics, chords));

        // The same through the editor: the panes hold the chords as chord lines.
        using var fixture = TempSongEditorSettings.Create();
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());
        var panes = new SongEditorChords();
        sut.LyricsRegion1 = panes.Combine(first, firstChords);
        sut.LyricsRegion2 = panes.Combine(second, secondChords);

        (sut.Lyrics, sut.Notations).Should().Be((lyrics, chords));
    }

    [Theory]
    [InlineData("[1]\nverse a\n[region 2]\nsecond a\n[chorus]\nchorus a\n[region 2]\nsecond c", "(1;G;0;)(3;Am;2;)(7;F;1;)",
        "[1]\nverse a\n[chorus]\nchorus a", "(1;G;0;)", "[1]\nsecond a\n[chorus]\nsecond c", "(1;Am;2;)(3;F;1;)")]
    [InlineData("[1]\na\n[2]\nb\n[region 2]\n[1]\nx\n[2]\ny", "(1;C;0;)(6;D;1;)", "[1]\na\n[2]\nb", "(1;C;0;)", "[1]\nx\n[2]\ny", "(1;D;1;)")]
    [InlineData("first a\nfirst b\n[region 2]\nsecond a", "(1;G;0;)(3;Am;2;)", "first a\nfirst b", "(1;G;0;)", "second a", "(0;Am;2;)")]
    [InlineData("[1]\na\nb\nc\nd\ne\nf", "(1;C;0;)(1;D;4;)(3;G;2;)()(5;F;8;)(unfinished", "[1]\na\nb\nc\nd\ne\nf", "(1;C;0;)(3;G;2;)(5;F;8;)", "", "")]
    [InlineData("Line 1\r\nLine 2\r\n\r\n", "", "Line 1\nLine 2", "", "", "")]
    [InlineData("[chorus]\nc line\n[region 2]\nc second\n\n[1]\nv line", "(3;Em;0;)", "[chorus]\nc line\n[1]\nv line", "", "[chorus]\nc second", "(1;Em;0;)")]
    [InlineData("", "", "", "", "", "")]
    public void AStoredSong_IsShownInTwoPanesAsFrmEditItemShowsIt(string lyrics, string chords, string first, string firstChords,
        string second, string secondChords)
        => SongEditorLyrics.Split(lyrics, chords).Should().Be(new SongEditorRegions(first, firstChords, second, secondChords));

    [Fact]
    public async Task Region2_IsEditedInItsOwnPane_AndSavedUnderTheVersesOfRegion1()
    {
        using var fixture = TempSongEditorSettings.Create();
        var repository = new FakeAdminDatabaseRepository();
        const string stored = "[1]\nverse a\n[region 2]\nsecond a\n[chorus]\nchorus a\n[region 2]\nsecond c";
        var sut = await OpenAsync(fixture, repository, Detail(10, "Song", folderNo: 2, lyrics: stored, notations: "(1;G;0;)(3;Am;2;)"));

        sut.LyricsRegion1.Should().Be("[1]\nG »\nverse a\n[chorus]\nchorus a");
        sut.LyricsRegion2.Should().Be("[1]\n  Am »\nsecond a\n[chorus]\nsecond c");
        sut.Lyrics.Should().Be(stored);
        sut.HasChanges.Should().BeFalse();

        await sut.SaveAsync();
        repository.LastSong!.Lyrics.Should().Be(stored, "an untouched song is saved as it was stored");
        repository.LastSong.Notations.Should().Be("(1;G;0;)(3;Am;2;)");

        sut.LyricsRegion2 += "\r\n[2]\r\nonly in the second language";
        await sut.SaveAsync();

        repository.LastSong!.Lyrics.Should().Be(stored + "\n[2]\n[region 2]\nonly in the second language");
        repository.LastSong.Notations.Should().Be("(1;G;0;)(3;Am;2;)");
    }

    // ── 6. Chords on the lyric text ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("G       C »\nAmazing grace", "Amazing grace", "(0;G;0;C;8;)")]
    [InlineData("First\n\n    D   Em7 »\nSecond line", "First\n\nSecond line", "(2;D;4;Em7;8;)")]
    [InlineData("                    D »\nAmen", "Amen", "(0;D;4;)")] // right of the last character: the end of the line
    [InlineData("G/B Am(add9) »\r\nline\r\n", "line\n", "(0;G/B;0;Am(add9);4;)")]
    [InlineData(" »\nNo chords", "No chords", "")]
    public void Chords_TypedAboveALyricLine_AreStoredWithTheCharacterTheyStandOver(string pane, string lyrics, string chords)
    {
        using var fixture = TempSongEditorSettings.Create();
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);

        sut.LyricsRegion1 = pane;

        (sut.Lyrics, sut.Notations).Should().Be((lyrics.TrimEnd('\n'), chords));
        sut.HasChanges.Should().BeTrue();
    }

    [Fact]
    public async Task Chords_AreMeasuredInTheFontOfThePane_AndUntouchedLinesKeepTheirStoredPositions()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        // Chords too close to fit, and one over a narrow letter: positions a proportional font cannot show exactly.
        const string stored = "(0;Gmaj7;0;C;1;D;2;)(1;Em;3;)";
        repository.Details[10] = Detail(10, "Song", folderNo: 2, lyrics: "illicit\nwide line", notations: stored);
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, null, null)
        {
            MeasureEditorText = text => text.Sum(c => c is 'i' or 'l' ? 0.25 : c == ' ' ? 0.5 : 1.0),
        };

        await sut.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(2, "Morning", true, 1), Song(10, "Song", folderNo: 2));

        sut.LyricsRegion1.Should().Be("Gmaj7 C D »\nillicit\n     Em »\nwide line", "'wid' is 2.25 wide, five spaces 2.5");
        sut.Notations.Should().Be(stored);
        sut.HasChanges.Should().BeFalse();

        // A chord typed by the operator: two spaces are 1.0 wide, as are the four narrow letters before 'c'.
        sut.LyricsRegion1 = sut.LyricsRegion1.Replace("Gmaj7 C D »\nillicit", "  A »\nillicit");

        sut.Notations.Should().Be("(0;A;4;)(1;Em;3;)", "the line that was not touched keeps its stored position");
    }

    [Fact]
    public void InsertChord_PutsTheChordOverTheCaret_AndKeepsTheOtherChordsInPlace()
    {
        using var fixture = TempSongEditorSettings.Create();
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);
        sut.LyricsRegion1 = "Amazing grace";

        int caret = sut.InsertChord(1, "Amazing ".Length, "C");

        sut.LyricsRegion1.Should().Be("        C »\nAmazing grace");
        sut.LyricsRegion1[caret..].Should().Be("grace", "the caret stays on the character");
        sut.Notations.Should().Be("(0;C;8;)");

        sut.InsertChord(1, sut.LyricsRegion1.IndexOf("Amazing", StringComparison.Ordinal), "G");

        sut.LyricsRegion1.Should().Be("G       C »\nAmazing grace");
        sut.Notations.Should().Be("(0;G;0;C;8;)");

        // Inside a chord line the chord goes where the caret is.
        sut.InsertChord(1, 2, "D7");

        sut.LyricsRegion1.Should().Be("G D7       C »\nAmazing grace");

        sut.LyricsRegion2 = "first\r\nsecond";
        sut.InsertChord(2, "first\r\nsec".Length, "Am");

        sut.LyricsRegion2.Should().Be("first\r\n   Am »\r\nsecond");
        sut.Lyrics.Should().Be("Amazing grace\n[region 2]\nfirst\nsecond");
        sut.Notations.Should().Be("(0;G;0;D7;2;C;11;)(3;Am;3;)");
    }

    [Fact]
    public void ChordMenu_OffersTheChordsOfFrmEditItem()
    {
        using var fixture = TempSongEditorSettings.Create();
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());

        sut.ChordMenu.Select(group => group.Group).Should().Equal("", "Minor", "Minor 7th", "Major 7th", "Bass");
        sut.ChordMenu[0].Chords.Should().Equal("G", "G#", "Ab", "A", "A#", "Bb", "B", "C", "C#", "Db", "D", "D#", "Eb", "E", "F", "F#", "Gb");
        sut.ChordMenu[1].Chords.Should().StartWith(["Gm", "Abm", "G#m", "Am"]);
        sut.ChordMenu[2].Chords.Should().StartWith(["Gm7", "Abm7", "G#m7"]);
        sut.ChordMenu[3].Chords.Should().StartWith(["G7", "G#7", "Ab7"]);
        sut.ChordMenu[4].Chords.Should().StartWith(["/G", "/G#", "/A"]).And.HaveCount(12);
    }

    // FrmEditItem "Transpose chord up/down" (LyricsProcessingTests.EditorTransposeMenusKeepBothRegionsAndTheFinalLyric).
    [Theory]
    [InlineData("G", 0, "Ab/C", "Bbm(add9)", "Ab", 11)]
    [InlineData("", -1, "G#/C", "A#m(add9)", "", -1)]
    public void TransposeSong_MovesTheChordsOfBothPanes_TheKeyAndTheCapo(string key, int capo, string bassChord, string minorChord,
        string keyAfter, int capoAfter)
    {
        using var fixture = TempSongEditorSettings.Create();
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);
        sut.Key = key;
        sut.Capo = capo;
        sut.LyricsRegion1 = "첫 줄\n\nG/B »\n마지막 줄";
        sut.LyricsRegion2 = "Am(add9)            G/B »\ntranslation continues into the next phrase";
        string notations = sut.Notations;
        notations.Should().Be("(2;G/B;0;)(4;Am(add9);0;G/B;20;)");

        sut.TransposeSongUpCommand.Execute(null);

        sut.LyricsRegion1.Should().Be($"첫 줄\n\n{bassChord} »\n마지막 줄");
        sut.Notations.Should().Be(notations.Replace("Am(add9)", minorChord).Replace("G/B", bassChord), "the chords move, their positions do not");
        sut.Lyrics.Should().Be("첫 줄\n\n마지막 줄\n[region 2]\ntranslation continues into the next phrase");
        sut.Key.Should().Be(keyAfter);
        sut.Capo.Should().Be(capoAfter);

        sut.TransposeSongDownCommand.Execute(null);

        sut.LyricsRegion1.Should().Be("첫 줄\n\nG/B »\n마지막 줄");
        sut.LyricsRegion2.Should().Be("Am(add9)            G/B »\ntranslation continues into the next phrase");
        sut.Notations.Should().Be(notations);
        sut.Key.Should().Be(key);
        sut.Capo.Should().Be(capo);
    }

    // The same cases and expected values as Axicultuss.Tests/SongEditorSaveTests.cs (gfMusic / gfLyrics).
    [Theory]
    [InlineData("(0;G/B;0;Am(add9);4;)(2;F#m7;1;[C;3;E#;5;C/E/G;7;N.C.;9;)", 1, 0, "(0;Ab/C;0;Bbm(add9);4;)(2;Gm7;1;[Db;3;F#;5;Db/F;7;N.C.;9;)")]
    [InlineData("(0;G/B;0;Am(add9);4;)(2;F#m7;1;[C;3;E#;5;C/E/G;7;N.C.;9;)", 1, 1, "(0;G#/C;0;A#m(add9);4;)(2;Gm7;1;[C#;3;F#;5;C#/F;7;N.C.;9;)")]
    [InlineData("(0;G/B;0;Am(add9);4;)(2;F#m7;1;[C;3;E#;5;C/E/G;7;N.C.;9;)", -1, -1, "(0;Gb/Bb;0;Abm(add9);4;)(2;Fm7;1;[B;3;Eb#;5;B/Eb;7;N.C.;9;)")]
    [InlineData("(0;G/B;0;Am(add9);4;)(2;F#m7;1;[C;3;E#;5;C/E/G;7;N.C.;9;)", 14, -1, "(0;A/C#;0;Bm(add9);4;)(2;G#m7;1;[D;3;F##;5;D/F#;7;N.C.;9;)")]
    [InlineData("(1;Bb;2;)()(3;C;1;)", 2, 1, "(1;C;2;)")]
    [InlineData("(0;G)", 1, 1, "(0;G#;-1;)")]
    [InlineData("junk", 1, 1, "")]
    public void Transposing_MovesEveryChordAsFrmEditItemDoes(string chords, int steps, int spelling, string expected)
        => SongEditorChords.Transpose(chords, steps, spelling).Should().Be(expected);

    [Theory]
    [InlineData("G", 1, "Ab", 0)]
    [InlineData("Ab", 1, "A", 1)]
    [InlineData("", 1, "", 1)]
    [InlineData("", -1, "", 0)]
    [InlineData("Xyz", 1, "Xyz", -1)]
    [InlineData("Am", -1, "G#m", 1)]
    [InlineData("F#", 1, "G", 1)]
    [InlineData("Gb", 1, "Gb", -1)]
    [InlineData("C#m", 2, "D#m", 1)]
    [InlineData("E", -13, "Eb", 0)]
    [InlineData("Dbm", 1, "Dbm", -1)]
    public void Transposing_MovesTheKeyAndChoosesSharpsOrFlatsAsFrmEditItemDoes(string key, int steps, string expected, int spelling)
    {
        SongEditorChords.TransposeKey(ref key, steps).Should().Be(spelling);
        key.Should().Be(expected);
    }

    [Fact]
    public async Task Preview_ShowsTheChordsOfTheFirstScreen_FromTheChordRecords()
    {
        using var fixture = TempSongEditorSettings.Create();
        var repository = new FakeAdminDatabaseRepository();
        var sut = await OpenAsync(fixture, repository, Detail(10, "Song", folderNo: 2,
            lyrics: "[1]\nAmazing grace\nHow sweet", notations: "(1;G;0;C;8;)"));

        sut.PreviewLyricLines.Select(line => (line.Text, line.Notation))
            .Should().Equal(("[1]", ""), ("Amazing grace", "G  C"), ("How sweet", ""));

        sut.TransposeUpCommand.Execute(null);

        sut.PreviewLyricLines[1].Notation.Should().Be("G#  C#");
        sut.Notations.Should().Be("(1;G;0;C;8;)", "the preview transposition does not touch the song");
        sut.HasChanges.Should().BeFalse();
    }

    // ── 7. What FrmEditItem checks before it saves ────────────────────────────────────────────────────

    [Theory]
    [InlineData("Song [live]", "", "", "", "", "제목에는 ] [ * \" < > 문자를 쓸 수 없습니다.")]
    [InlineData("What * is", "", "", "", "", "제목에는")]
    [InlineData("Song", "Alt <2>", "", "", "", "다른 제목에는")]
    [InlineData("Song", "", "\"Publisher\"", "", "", "저작권에는")]
    [InlineData("Song", "", "", "Book [3]", "", "교단 참조에는")]
    [InlineData("Song", "", "", "", "A > B", "작사·작곡에는")]
    public async Task SaveAsync_TitleFieldsMayNotHoldTheCharactersFrmEditItemRefuses(string title, string alternateTitle, string copyright,
        string bookReference, string writer, string message)
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var sut = new SongEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);
        sut.Lyrics = "Lyrics";
        (sut.Title, sut.AlternateTitle, sut.Copyright, sut.BookReference, sut.Writer) = (title, alternateTitle, copyright, bookReference, writer);

        await sut.SaveAsync();

        sut.ValidationMessage.Should().StartWith(message);
        repository.LastSong.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_ATitleOfMoreThan100CharactersIsRefused_NotCut()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var sut = new SongEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);
        sut.Lyrics = "Lyrics";
        sut.Title = new string('가', 101);

        await sut.SaveAsync();

        sut.ValidationMessage.Should().Contain("너무 깁니다(101자)");
        repository.LastSong.Should().BeNull();

        sut.Title = new string('가', 100) + "  ";
        await sut.SaveAsync();

        repository.LastSong!.Title.Should().HaveLength(100);
    }

    [Theory]
    // FrmEditItem.ValidateTitles: "There are no Lyrics to save!"
    [InlineData("", "", "", "저장할 가사가 없습니다.")]
    // FrmEditItem.ValidateSequence: "Sequence contains ... which is not in the lyrics!"
    [InlineData("[1]\nverse", "", "1,2", "절 순서에 있는 [2] 이(가) 가사에 없습니다.")]
    [InlineData("verse", "", "1,c", "절 순서에 있는 [chorus] 이(가) 가사에 없습니다.")]
    [InlineData("[1]\nverse", "", "1,x", "절 순서 'x'은(는) 쓸 수 없습니다.")]
    [InlineData("[1]\nverse", "", "01", "절 순서 '01'은(는) 쓸 수 없습니다.")]
    // A tab or a line break inside the text would be stored as a verse code of its own.
    [InlineData("[1]\nverse\n[2]\nmore", "", "1\t2", "절 순서는 한 줄에 쓰고 쉼표나 공백으로 구분하세요.")]
    [InlineData("[1]\nverse\n[2]\nmore", "", "1\r\n2", "절 순서는 한 줄에 쓰고 쉼표나 공백으로 구분하세요.")]
    // SongExchangeSequence.ToDatabase would store these tokens as text, which WinForms reads as other verses.
    // "q" alone is the one printable code that cannot be told from the token "p" when read back.
    [InlineData("[prechorus 2]\nq", "", "q", "이 절 순서는 저장 형식(절 코드)으로 바꿀 수 없습니다.")]
    // FrmEditItem.ValidateContents
    [InlineData("[1]\nverse\n[region 2]\nsecond", "", "", "기본 가사: [region 2] 표시는 쓸 수 없습니다.")]
    [InlineData("[1]\nverse", "[1]\nsecond\n[region 2]", "", "보조 가사: [region 2] 표시는 쓸 수 없습니다.")]
    [InlineData("[1]\nverse\n[2]\nmore\n[1]\nagain", "", "", "기본 가사: 절 표시 [1] 이(가) 두 번 있습니다.")]
    [InlineData("[chorus]\nc", "[chorus]\nx\n[chorus]\ny", "", "보조 가사: 절 표시 [chorus] 이(가) 두 번 있습니다.")]
    [InlineData("[1]\nverse\n[3]\nthird", "", "", "기본 가사: 2절 표시([2])가 없습니다.")]
    [InlineData("intro\n[2]\nverse", "", "", "기본 가사: [2] 앞의 가사에도 절 표시가 필요합니다.")]
    [InlineData("[1]\nverse", "translation\n[1]\nsecond", "", "보조 가사: [1] 앞의 가사에도 절 표시가 필요합니다.")]
    // The empty first line is not stored, so it does not stand for a heading (FrmEditItem let this through and
    // refused the text it had stored the next time).
    [InlineData("\nintro\n[2]\nverse", "", "", "기본 가사: [2] 앞의 가사에도 절 표시가 필요합니다.")]
    // Stricter than FrmEditItem, which rewrites the text or drops the chords in these cases.
    [InlineData("[1] verse", "", "", "기본 가사: 절 표시 [1] 은(는) 한 줄에 단독으로 써야 합니다.")]
    [InlineData("[1]  \nverse", "", "", "기본 가사: 절 표시 [1] 의 앞이나 뒤에 공백이 있습니다. 공백을 지우세요.")]
    [InlineData("[1]\nfirst\nG   D »\n[2]\nsecond", "", "", "기본 가사: 코드 줄 바로 아래에 절 표시 [2] 이(가) 있습니다. 절 표시를 코드 줄 위로 옮기세요.")]
    [InlineData("lyric » G C", "", "", "기본 가사: 코드 줄은 » 로 끝나고 가사 줄 바로 위에 있어야 합니다.")]
    [InlineData("G »\n\nlyric", "", "", "기본 가사: 코드 줄은 » 로 끝나고 가사 줄 바로 위에 있어야 합니다.")]
    [InlineData("G » C\nlyric", "", "", "기본 가사: 코드 줄은 » 로 끝나고 가사 줄 바로 위에 있어야 합니다.")]
    [InlineData("G;1 »\nlyric", "", "", "기본 가사: 코드 'G;1'에 쓸 수 없는 문자")]
    [InlineData("A) »\nlyric", "", "", "기본 가사: 코드 'A)'에 쓸 수 없는 문자")]
    public async Task SaveAsync_RefusesWhatFrmEditItemRefuses(string region1, string region2, string sequence, string message)
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var sut = new SongEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);
        sut.Title = "Song";
        (sut.LyricsRegion1, sut.LyricsRegion2, sut.Sequence) = (region1, region2, sequence);

        await sut.SaveAsync();

        sut.ValidationMessage.Should().StartWith(message);
        repository.LastSong.Should().BeNull();
    }

    [Theory]
    [InlineData("[1]\nverse\n[chorus]\nc\n[2]\nmore", "", "1,C,2,c", "\u0001\u0000\u0002\u0000")]
    [InlineData("verse without a heading", "", "1", "\u0001")] // FrmEditItem lists verse 1 for lyrics without headings
    [InlineData("[chorus]\nc only", "", "c,1", "\u0000\u0001")]
    [InlineData("[1]\nfirst", "[1]\nsecond\n[bridge]\nonly second", "1,b", "\u0001d")]
    [InlineData("\n[1]\nverse", "", "1", "\u0001")] // empty lines above the first heading are not lyrics above it
    [InlineData("[1]\nverse", "", "1 \r\n", "\u0001")] // the line break of a pasted sequence is not part of it
    [InlineData("[chorus]\nc\n[prechorus]\np\n[prechorus 2]\nq\n[chorus 2]\nt\n[bridge 2]\nw\n[ending]\ne", "", "c p q t w e", "\u0000opfge")]
    [InlineData("[1]\nverse", "", " \r\n", "")] // a stray line break in the sequence box is an empty sequence
    public async Task SaveAsync_AcceptsASequenceOfVersesThatAreInTheLyrics(string region1, string region2, string sequence, string stored)
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var repository = new FakeAdminDatabaseRepository();
        var sut = new SongEditorViewModel(fixture.Settings, repository);
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);
        sut.Title = "Song";
        (sut.LyricsRegion1, sut.LyricsRegion2, sut.Sequence) = (region1, region2, sequence);

        await sut.SaveAsync();

        sut.ValidationMessage.Should().BeEmpty();
        // The repository writes SEQUENCE with this conversion (AdminDatabaseRepository.BuildSongValues).
        Axicultus.Module.SongExchangeSequence.ToDatabase(repository.LastSong!.Sequence).Should().Be(stored);
    }

    [Fact]
    public async Task SaveAsync_AStoredSequenceTheBoxCannotShowAsTokens_IsSavedAsItWasStored()
    {
        // Bridge and ending only: verse codes 100 and 101, "de", which FromWpfDatabase leaves as it is.
        using var fixture = TempSongEditorSettings.Create();
        var repository = new FakeAdminDatabaseRepository();
        var sut = await OpenAsync(fixture, repository, Detail(10, "Song", folderNo: 2, lyrics: "[bridge]\nb\n[ending]\ne", sequence: "de"));
        sut.Title = "Renamed";

        await sut.SaveAsync();

        sut.ValidationMessage.Should().BeEmpty();
        Axicultus.Module.SongExchangeSequence.ToDatabase(repository.LastSong!.Sequence).Should().Be("de");

        sut.LyricsRegion1 = "[bridge]\nb";
        await sut.SaveAsync();

        sut.ValidationMessage.Should().Be("절 순서에 있는 [ending] 이(가) 가사에 없습니다.");
    }

    // ── Every stored column, through the real repository ───────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_StoresEveryColumnAsFrmEditItemDoes()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateSongDatabase("songs.db");
        const string lyrics = "[1]\nverse a\n[region 2]\nsecond a\n[chorus]\nchorus a\n[2]\nverse two";
        var stored = new Dictionary<string, object>
        {
            ["TITLE_1"] = "주 은혜", ["TITLE_2"] = "Grace", ["SONG_NUMBER"] = 0, ["FOLDERNO"] = 2, ["LYRICS"] = lyrics,
            ["SEQUENCE"] = "\u0001\u0000\u0002\u0000", ["WRITER"] = "Writer", ["COPYRIGHT"] = "Copyright", ["CAPO"] = -1,
            ["TIMING"] = "4/4", ["KEY"] = "G", ["MSC"] = "(1;G;0;)(3;Am;2;)(5;C;7;)", ["CATEGORY"] = "",
            ["LICENCE_ADMIN1"] = "CCLI", ["LICENCE_ADMIN2"] = "Admin 2", ["BOOK_REFERENCE"] = "Book 12",
            ["USER_REFERENCE"] = "User 7", ["SETTINGS"] = "11=1>12=0>", ["FORMATDATA"] = "29=-1>43=Arial>",
        };
        int songId = fixture.InsertSong(stored);
        var repository = new AdminDatabaseRepository();
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, null, null);
        var song = (await repository.GetSongsAsync(fixture.AdminDatabasePath, 2)).Single();

        await sut.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(2, "Evening", true, 1), song);

        sut.SaveBlockedReason.Should().BeEmpty();
        sut.Sequence.Should().Be("1,c,2,c");
        sut.Folders.Select(folder => folder.Name).Should().Equal("Morning", "Evening");

        // Saved as opened: the row FrmEditItem wrote is the row this editor writes.
        await sut.SaveAsync();

        sut.StatusMessage.Should().Be("저장되었습니다.");
        fixture.ReadSong(songId).Should().Equal(stored);

        // Edited: a line above the chords, a chord, the key and capo with the chords, the folder, the sequence.
        sut.LyricsRegion1 = sut.LyricsRegion1.Replace("[1]\n", "[1]\nnew line\n");
        sut.InsertChord(1, sut.LyricsRegion1.IndexOf("new line", StringComparison.Ordinal) + 4, "D");
        sut.Capo = 3;
        sut.TransposeSongUpCommand.Execute(null);
        sut.Sequence = "1 2 c";
        sut.SelectedFolder = sut.Folders[0];
        sut.Title = "  주 은혜 2  ";
        await sut.SaveAsync();

        sut.StatusMessage.Should().Be("저장되었습니다.");
        var expected = new Dictionary<string, object>(stored)
        {
            ["TITLE_1"] = "주 은혜 2", ["FOLDERNO"] = 1, ["KEY"] = "Ab", ["CAPO"] = 2, ["SEQUENCE"] = "\u0001\u0002\u0000",
            ["LYRICS"] = "[1]\nnew line\nverse a\n[region 2]\nsecond a\n[chorus]\nchorus a\n[2]\nverse two",
            ["MSC"] = "(1;Eb;4;)(2;Ab;0;)(4;Bbm;2;)(6;Db;7;)",
        };
        fixture.ReadSong(songId).Should().Equal(expected);

        // And the stored song opens again as the same two panes.
        var reopened = new SongEditorViewModel(fixture.Settings, repository, repository, null, null);
        song = (await repository.GetSongsAsync(fixture.AdminDatabasePath, 1)).Single();
        await reopened.LoadAsync(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 1), song);

        reopened.LyricsRegion1.Should().Be(sut.LyricsRegion1);
        reopened.LyricsRegion2.Should().Be(sut.LyricsRegion2);
        (reopened.Capo, reopened.Key, reopened.Sequence, reopened.FolderNo).Should().Be((2, "Ab", "1,2,c", 1));
    }

    // ── Songs as FrmEditItem left them over the years ─────────────────────────────────────────────

    [Fact]
    public async Task Chords_OfARepeatedLineThatLookTheSame_KeepTheirOwnStoredPositions()
    {
        using var fixture = TempSongEditorSettings.Create();
        var repository = new FakeAdminDatabaseRepository();
        // "Em" stored over the second and over the fourth character: both are shown right after "B7".
        const string stored = "(0;B7;0;Em;1;)(2;B7;0;Em;3;)";
        var sut = await OpenAsync(fixture, repository, Detail(10, "Song", folderNo: 2, lyrics: "Same line\nOther\nSame line", notations: stored));

        sut.LyricsRegion1.Should().Be("B7 Em »\nSame line\nOther\nB7 Em »\nSame line");
        sut.Notations.Should().Be(stored);
        await sut.SaveAsync();
        repository.LastSong!.Notations.Should().Be(stored, "an untouched song keeps MSC as it is stored");

        sut.LyricsRegion1 = "New first line\n" + sut.LyricsRegion1;

        sut.Notations.Should().Be("(1;B7;0;Em;1;)(3;B7;0;Em;3;)");
    }

    [Fact]
    public async Task SaveAsync_AStoredSongIsNotHeldToTheStricterRules_UntilItsTextIsChanged()
    {
        using var fixture = TempSongEditorSettings.Create();
        var repository = new FakeAdminDatabaseRepository();
        // A heading that shares its line with text, and a chord marker inside a lyric line: FrmEditItem saves both.
        const string stored = "[1]\nfirst\n[2]glued to its line\nlyric » G";
        var sut = await OpenAsync(fixture, repository, Detail(10, "Song", folderNo: 2, lyrics: stored));
        sut.Title = "Renamed";

        await sut.SaveAsync();

        sut.ValidationMessage.Should().BeEmpty();
        repository.LastSong!.Lyrics.Should().Be(stored);

        // The heading line is a line the song already had; the chord marker is asked about once the pane changed.
        sut.LyricsRegion1 += "\nmore";
        await sut.SaveAsync();

        sut.ValidationMessage.Should().Be("기본 가사: 코드 줄은 » 로 끝나고 가사 줄 바로 위에 있어야 합니다.");

        sut.LyricsRegion1 = sut.LyricsRegion1.Replace("lyric » G", "lyric");
        await sut.SaveAsync();

        sut.ValidationMessage.Should().BeEmpty();
        repository.LastSong!.Lyrics.Should().Be("[1]\nfirst\n[2]glued to its line\nlyric\nmore");

        sut.LyricsRegion1 += "\n[3]typed now";
        await sut.SaveAsync();

        sut.ValidationMessage.Should().Be("기본 가사: 절 표시 [3] 은(는) 한 줄에 단독으로 써야 합니다.");
    }

    [Fact]
    public async Task SaveAsync_AHeadingStoredWithSpacesAfterIt_IsKeptAsStored_AndATypedOneIsRefusedForTheSpaces()
    {
        using var fixture = TempSongEditorSettings.Create();
        var repository = new FakeAdminDatabaseRepository();
        // Both apps show "[1]  " as a line of the verse: saving it as a heading would change the screens.
        const string stored = "[1]  \nfirst\n[chorus]\nrefrain";
        var sut = await OpenAsync(fixture, repository, Detail(10, "Song", folderNo: 2, lyrics: stored));

        sut.LyricsRegion1 = sut.LyricsRegion1.Replace("refrain", "refrain!");
        await sut.SaveAsync();

        sut.ValidationMessage.Should().BeEmpty();
        repository.LastSong!.Lyrics.Should().Be("[1]  \nfirst\n[chorus]\nrefrain!");

        sut.LyricsRegion1 += "\n[2] \nsecond";
        await sut.SaveAsync();

        sut.ValidationMessage.Should().Be("기본 가사: 절 표시 [2] 의 앞이나 뒤에 공백이 있습니다. 공백을 지우세요.");
    }

    [Theory]
    // Stored by FrmEditItem through a pane that started with an empty line, or before it had the rule at all.
    [InlineData("intro\n[2]\nverse", "intro!\n[2]\nverse")]
    // The lines above [1] are lines of verse 1 in both apps; the save puts them under its heading.
    [InlineData("intro\n[1]\nfirst\n[2]\nverse", "[1]\nintro!\nfirst\n[2]\nverse")]
    [InlineData("[region 2]\nintro\n[1]\nfirst\n[region 2]\nmore", "[1]\nfirst\n[region 2]\nintro!\nmore")]
    public async Task SaveAsync_ASongStoredWithLyricsAboveItsFirstHeading_CanBeEditedAndSaved(string stored, string edited)
    {
        using var fixture = TempSongEditorSettings.Create();
        var repository = new FakeAdminDatabaseRepository();
        var sut = await OpenAsync(fixture, repository, Detail(10, "Song", folderNo: 2, lyrics: stored));

        sut.Title = "Renamed";
        await sut.SaveAsync();
        sut.ValidationMessage.Should().BeEmpty();

        if (sut.LyricsRegion1.Contains("intro", StringComparison.Ordinal))
        {
            sut.LyricsRegion1 = sut.LyricsRegion1.Replace("intro", "intro!");
        }
        else
        {
            sut.LyricsRegion2 = sut.LyricsRegion2.Replace("intro", "intro!");
        }

        await sut.SaveAsync();

        sut.ValidationMessage.Should().BeEmpty();
        repository.LastSong!.Lyrics.Should().Be(edited);
    }

    [Fact]
    public async Task SaveAsync_KeepsATextFieldTheOperatorLeftAlone_AndTrimsTheOneThatWasEdited()
    {
        using var fixture = TempSongEditorSettings.Create();
        var repository = new FakeAdminDatabaseRepository();
        // FrmEditItem stores the fields as typed: a title can end with a space.
        var sut = await OpenAsync(fixture, repository, Detail(10, "Stored title ", folderNo: 2, lyrics: "line") with { Writer = " Writer", Key = "G " });

        sut.Capo = 3;
        sut.Key = " A ";
        await sut.SaveAsync();

        sut.ValidationMessage.Should().BeEmpty();
        (repository.LastSong!.Title, repository.LastSong.Writer, repository.LastSong.Key).Should().Be(("Stored title ", " Writer", "A"));
        // What the song list is given after the save (MainWindow reads these) is what was stored.
        (sut.Title, sut.Writer, sut.Key).Should().Be(("Stored title ", " Writer", "A"));
    }

    [Fact]
    public async Task SaveAsync_WhenAPaneEditCouldNotBeRead_RefusesInsteadOfWritingTheTextFromBeforeTheEdit()
    {
        using var fixture = TempSongEditorSettings.Create();
        var repository = new FakeAdminDatabaseRepository();
        var sut = await OpenAsync(fixture, repository, Detail(10, "Song", folderNo: 2, lyrics: "Old line"));
        sut.MeasureEditorText = _ => throw new InvalidOperationException("no font");

        // The binding of the pane swallows what the setter throws.
        var edit = () => sut.LyricsRegion1 = "G »\nNew line";
        edit.Should().Throw<InvalidOperationException>();
        await sut.SaveAsync();

        sut.ValidationMessage.Should().StartWith("마지막으로 고친 가사를 저장 형식으로 바꾸지 못했습니다.");
        repository.LastSong.Should().BeNull();

        sut.MeasureEditorText = text => text.Length;
        sut.LyricsRegion1 = "G »\nNew line!";
        await sut.SaveAsync();

        (repository.LastSong!.Lyrics, repository.LastSong.Notations).Should().Be(("New line!", "(0;G;0;)"));
    }

    [Theory]
    // FrmEditItem looks for the headings anywhere in the text, so these stored songs are refused as it refuses them.
    [InlineData("[1]\nfirst\nsee [1] again", "기본 가사: 절 표시 [1] 이(가) 두 번 있습니다.")]
    [InlineData("[1]\nfirst\n[3]\nthird", "기본 가사: 2절 표시([2])가 없습니다.")]
    public async Task SaveAsync_AStoredSongThatFrmEditItemRefuses_IsRefused(string stored, string message)
    {
        using var fixture = TempSongEditorSettings.Create();
        var repository = new FakeAdminDatabaseRepository();
        var sut = await OpenAsync(fixture, repository, Detail(10, "Song", folderNo: 2, lyrics: stored));

        await sut.SaveAsync();

        sut.ValidationMessage.Should().Be(message);
        repository.LastSong.Should().BeNull();
    }

    // ── A real song database: every song, opened and saved untouched, against FrmEditItem's own code ─────────

    /// <summary>Runs when AXICULTUS_REAL_SONG_DB names a song database and the WinForms app is built; skipped otherwise.</summary>
    private sealed class RealSongDatabaseFactAttribute : FactAttribute
    {
        public RealSongDatabaseFactAttribute()
        {
            if (!File.Exists(RealSongDatabase))
            {
                Skip = "Set AXICULTUS_REAL_SONG_DB to a song database (it is copied, never written).";
            }
            else if (!File.Exists(WinFormsAssembly))
            {
                Skip = $"Build the WinForms app first: {WinFormsAssembly}";
            }
        }
    }

    private static string RealSongDatabase => Environment.GetEnvironmentVariable("AXICULTUS_REAL_SONG_DB") ?? "";

    // The WinForms app built beside this test assembly: <root>/Axicultuss/bin/<configuration>/<framework>/Axicultuss.dll.
    private static string WinFormsAssembly
    {
        get
        {
            var output = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
            return Path.Combine(
                Axicultus.Wpf.Tests.Accessibility.XamlAccessibilityScanner.FindRepoRoot(),
                "Axicultuss", "bin", output.Parent!.Name, output.Name, "Axicultuss.dll");
        }
    }

    [RealSongDatabaseFact]
    public async Task EveryRealSong_SavedUntouched_IsStoredAsFrmEditItemStoresIt()
    {
        using var fixture = TempSongEditorSettings.Create();
        Directory.CreateDirectory(fixture.Root);
        string database = Path.Combine(fixture.Root, "songs.db");
        File.Copy(RealSongDatabase, database);
        var songs = ReadRealSongs(database);
        var winForms = StaHelper.RunOnSta(() => SaveWithFrmEditItem(songs));

        var repository = new FakeAdminDatabaseRepository();
        repository.Folders.AddRange(songs.Select(song => song.FolderNo).Distinct().Order()
            .Select(folderNo => new SongFolderSummary(folderNo, $"Folder {folderNo}", true, 0)));
        var differing = new List<string>();
        var unstable = new List<int>();
        var changedByBoth = new SortedDictionary<string, List<int>>();
        var refused = new SortedDictionary<string, List<int>>();
        foreach (var stored in songs)
        {
            // Folder 0 holds the deleted songs, which the editor never saves; here they count as songs of folder 1.
            var song = stored with { FolderNo = Math.Max(stored.FolderNo, 1) };
            repository.Details[song.SongId] = song;
            var sut = new SongEditorViewModel(fixture.Settings, repository, repository, null, null);
            await sut.LoadAsync(database, new SongFolderSummary(song.FolderNo, $"Folder {song.FolderNo}", true, 0),
                Song(song.SongId, song.Title, song.FolderNo, lyrics: song.Lyrics));
            repository.Details.Remove(song.SongId);
            sut.SaveBlockedReason.Should().BeEmpty();

            var expected = winForms[song.SongId];
            if ((sut.Lyrics, sut.Notations) != expected)
            {
                differing.Add($"song {song.SongId}: {FirstDifference(expected.Lyrics, sut.Lyrics)}; MSC {expected.Notations} / {sut.Notations}");
            }

            // What was saved, opened again and saved untouched, is stored the same.
            var reopened = SongEditorLyrics.Split(sut.Lyrics, sut.Notations);
            if (SongEditorLyrics.Merge(reopened.Text1, reopened.Notations1, reopened.Text2, reopened.Notations2) != (sut.Lyrics, sut.Notations))
            {
                unstable.Add(song.SongId);
            }

            await sut.SaveAsync();
            bool isRefused = sut.ValidationMessage.Length > 0;
            if (isRefused)
            {
                Add(refused, System.Text.RegularExpressions.Regex.Replace(sut.ValidationMessage, @"\[[^\]]+\]|\d+절", "…"), song.SongId);
            }
            else
            {
                sut.StatusMessage.Should().Be("저장되었습니다.");
                (repository.LastSong!.Lyrics, repository.LastSong.Notations).Should().Be((sut.Lyrics, sut.Notations));
                repository.LastSong.Capo.Should().Be(Math.Max(song.Capo, -1));
            }

            string storedLyrics = song.Lyrics.Replace("\r\n", "\n").TrimEnd('\n', '\r');
            if (storedLyrics != sut.Lyrics || song.Notations != sut.Notations)
            {
                string kind = KindOfChange(storedLyrics, song.Notations, sut.Lyrics, sut.Notations);
                Add(changedByBoth, isRefused ? $"refused, so not saved: {kind}" : kind, song.SongId);
            }

        }


        var report = new System.Text.StringBuilder()
            .AppendLine($"songs compared: {songs.Count}")
            .AppendLine($"differing between the WPF editor and FrmEditItem: {differing.Count}");
        differing.ForEach(line => report.AppendLine("  " + line));
        report.AppendLine($"not stored the same when opened and saved a second time: {unstable.Count} {string.Join(' ', unstable)}");
        report.AppendLine($"stored text changed by an untouched save (both apps): {changedByBoth.Values.Sum(ids => ids.Count)}");
        foreach (var (kind, ids) in changedByBoth)
        {
            report.AppendLine($"  {ids.Count,5}  {kind}: {string.Join(' ', ids)}");
        }

        report.AppendLine($"refused by the editor, as FrmEditItem refuses them: {refused.Values.Sum(ids => ids.Count)}");
        foreach (var (rule, ids) in refused)
        {
            report.AppendLine($"  {ids.Count,5}  {rule}: {string.Join(' ', ids)}");
        }

        string reportPath = Path.Combine(Path.GetTempPath(), "Axicultus_SongEditor_RealSongs.txt");
        File.WriteAllText(reportPath, report.ToString());
        differing.Should().BeEmpty($"the full report is in {reportPath}");
        unstable.Should().BeEmpty($"the full report is in {reportPath}");

        static void Add(SortedDictionary<string, List<int>> groups, string key, int songId)
        {
            if (!groups.TryGetValue(key, out var ids))
            {
                groups[key] = ids = [];
            }

            ids.Add(songId);
        }
    }

    private static List<SongDetail> ReadRealSongs(string database)
    {
        var songs = new List<SongDetail>();
        using var connection = new System.Data.SQLite.SQLiteConnection($"Data Source={database};Read Only=True;Pooling=False");
        connection.Open();
        using var command = new System.Data.SQLite.SQLiteCommand(
            "SELECT SONGID, TITLE_1, FOLDERNO, LYRICS, SEQUENCE, CAPO, MSC FROM SONG ORDER BY SONGID", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            string Text(int column) => reader.IsDBNull(column) ? "" : Convert.ToString(reader.GetValue(column)) ?? "";
            int Number(int column) => int.TryParse(Text(column), out int value) ? value : 0;
            songs.Add(Detail(Number(0), Text(1), Number(2), lyrics: Text(3), sequence: Text(4), capo: Number(5), notations: Text(6)));
        }

        return songs;
    }

    // LYRICS and MSC as FrmEditItem stores each song when it is opened and saved untouched: the WinForms app's
    // own gfLyrics.ExtractLyrics and Merge_Songs, loaded from its build output.
    private static Dictionary<int, (string Lyrics, string Notations)> SaveWithFrmEditItem(List<SongDetail> songs)
    {
        const System.Reflection.BindingFlags statics = System.Reflection.BindingFlags.Static
            | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        var gf = System.Reflection.Assembly.LoadFrom(WinFormsAssembly).GetType("Axicultus.Gf", throwOnError: true)!;
        var byRef = typeof(string).MakeByRefType();
        var extract = gf.GetMethod("ExtractLyrics", statics, [typeof(string), typeof(string), byRef, byRef, byRef, byRef])!;
        var merge = gf.GetMethod("Merge_Songs", statics)!;
        var itemType = merge.GetParameters()[0].ParameterType;
        object first = Activator.CreateInstance(itemType)!, second = Activator.CreateInstance(itemType)!;
        gf.GetMethod("InitializeVerseHeadings", statics)!.Invoke(null, null);

        void Fill(object item, object lyrics, object notations)
        {
            itemType.GetField("CompleteLyrics")!.SetValue(item, lyrics);
            itemType.GetField("Notations")!.SetValue(item, notations);
            itemType.GetField("OriginalNotations")!.SetValue(item, notations);
        }

        var saved = new Dictionary<int, (string, string)>();
        foreach (var song in songs)
        {
            object[] panes = [song.Lyrics, song.Notations, "", "", "", ""];
            (extract.Invoke(null, panes) as IDisposable)?.Dispose();
            Fill(first, panes[2], panes[3]);
            Fill(second, panes[4], panes[5]);
            object[] stored = [first, second, "", ""];
            merge.Invoke(null, stored);
            saved[song.SongId] = ((string)stored[2], (string)stored[3]);
        }

        return saved;
    }

    private static string FirstDifference(string expected, string actual)
    {
        string[] a = expected.Split('\n'), b = actual.Split('\n');
        int line = 0;
        while (line < a.Length && line < b.Length && a[line] == b[line])
        {
            line++;
        }

        return $"line {line}: FrmEditItem \"{(line < a.Length ? a[line] : "<end>")}\" ({a.Length} lines), editor \"{(line < b.Length ? b[line] : "<end>")}\" ({b.Length} lines)";
    }

    // What an untouched save does to the stored text: the first of these that tells the two texts apart.
    private static string KindOfChange(string stored, string storedChords, string saved, string savedChords)
    {
        static bool IsMarker(string line) => line.StartsWith('[') && line.EndsWith(']');
        string[] before = [.. stored.Split('\n').Where(line => line.Length > 0)];
        string[] after = [.. saved.Split('\n').Where(line => line.Length > 0)];
        string[] wordsBefore = [.. before.Where(line => !IsMarker(line))], wordsAfter = [.. after.Where(line => !IsMarker(line))];
        string kind = stored == saved ? ""
            : before.SequenceEqual(after) ? "empty lines dropped"
            : wordsBefore.SequenceEqual(wordsAfter) ? "headings or region markers added, dropped or moved"
            : wordsBefore.Order().SequenceEqual(wordsAfter.Order()) ? "lyric lines in another order"
            : "lyric lines lost or added";
        return storedChords == savedChords ? kind : (kind.Length > 0 ? kind + " + " : "") + "chord records changed";
    }

    private static async Task<SongEditorViewModel> OpenAsync(TempSongEditorSettings fixture, FakeAdminDatabaseRepository repository, SongDetail detail)
    {
        fixture.CreateAdminDatabaseFile("custom.db");
        repository.Details[detail.SongId] = detail;
        var sut = new SongEditorViewModel(fixture.Settings, repository, repository, null, null);
        await sut.LoadAsync(
            fixture.AdminDatabasePath,
            new SongFolderSummary(detail.FolderNo, "Opened from", true, 1),
            Song(detail.SongId, detail.Title, detail.FolderNo, lyrics: detail.Lyrics));
        return sut;
    }

    private static SongSummary Song(
        int id,
        string title,
        int folderNo,
        string alternateTitle = "",
        string category = "",
        string key = "",
        string lyrics = "")
        => new(id, title, alternateTitle, folderNo, SongNumber: id, category, key, lyrics);

    private static SongDetail Detail(
        int id,
        string title,
        int folderNo,
        string lyrics = "",
        string alternateTitle = "",
        int songNumber = 1,
        string sequence = "",
        string writer = "",
        string copyright = "",
        int capo = 0,
        string timing = "",
        string key = "",
        string notations = "",
        string category = "",
        string licenceAdmin1 = "",
        string licenceAdmin2 = "",
        string bookReference = "",
        string userReference = "",
        string settings = "",
        string formatData = "")
        => new(
            id,
            title,
            alternateTitle,
            folderNo,
            songNumber,
            lyrics,
            sequence,
            writer,
            copyright,
            capo,
            timing,
            key,
            notations,
            category,
            licenceAdmin1,
            licenceAdmin2,
            bookReference,
            userReference,
            settings,
            formatData);

    [Fact]
    public void Preview_SplitsChordNotation_PerLine_OnMarker()
    {
        // 리치 per-line 미리보기 1차: '»' 뒤 코드/노테이션을 본문과 분리해 줄 단위로 담는다.
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);

        sut.Lyrics = "Amazing grace » G  C\nHow sweet the sound";

        sut.PreviewLyricLines.Should().HaveCount(2);
        sut.PreviewLyricLines[0].Text.Should().Be("Amazing grace");
        sut.PreviewLyricLines[0].Notation.Should().Be("G  C", "'»' 뒤는 코드/노테이션");
        sut.PreviewLyricLines[1].Text.Should().Be("How sweet the sound");
        sut.PreviewLyricLines[1].Notation.Should().BeEmpty("마커 없는 줄은 전부 본문");
    }

    [Fact]
    public void Preview_NormalizesMojibakeChordMarker_LikeOutput()
    {
        // 미리보기도 출력과 동일하게 "Â»"(U+00C2 U+00BB) 모지바케를 '»'로 모아 코드를 분리한다(마커 해석 일치).
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);

        sut.Lyrics = "은혜로다 Â» G C";

        sut.PreviewLyricLines.Should().ContainSingle();
        sut.PreviewLyricLines[0].Text.Should().Be("은혜로다", "Â 잔여 없이 본문만");
        sut.PreviewLyricLines[0].Notation.Should().Be("G C");
    }

    [Fact]
    public void Preview_LeadingMarker_PutsAllInNotation_WithEmptyText()
    {
        // 경계: 줄이 '»'로 시작하면 본문은 비고 코드만 있다(코드 전용 줄).
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);

        sut.Lyrics = "» G  C  D";

        sut.PreviewLyricLines.Should().ContainSingle();
        sut.PreviewLyricLines[0].Text.Should().BeEmpty();
        sut.PreviewLyricLines[0].Notation.Should().Be("G  C  D");
    }

    [Fact]
    public void Preview_EmptyLyrics_ShowsSinglePlaceholderLine()
    {
        using var fixture = TempSongEditorSettings.Create();
        fixture.CreateAdminDatabaseFile("custom.db");
        var sut = new SongEditorViewModel(fixture.Settings, new FakeAdminDatabaseRepository());
        sut.Load(fixture.AdminDatabasePath, new SongFolderSummary(1, "Morning", true, 0), null);

        sut.Lyrics = "   ";

        sut.PreviewLyricLines.Should().ContainSingle().Which.Text.Should().Be("가사 없음");
    }

    private sealed class FakeAdminDatabaseRepository : IAdminDatabaseRepository, IAdminSongDetailRepository
    {
        public string LastDatabasePath { get; private set; } = "";

        public string LastBackupRoot { get; private set; } = "";

        public SongWriteModel? LastSong { get; private set; }

        public Dictionary<int, SongDetail> Details { get; } = [];

        public List<SongFolderSummary> Folders { get; } = [];

        public Exception? DetailFailure { get; set; }

        public Task<SongDetail?> GetSongDetailAsync(string databasePath, int songId)
            => DetailFailure is null
                ? Task.FromResult(Details.TryGetValue(songId, out var detail) ? detail : null)
                : throw DetailFailure;

        public Task<AdminDatabaseSchemaInventory> AnalyzeSchemaAsync(string databasePath)
            => throw new NotSupportedException();

        public Exception? FoldersFailure { get; set; }

        public Task<IReadOnlyList<SongFolderSummary>> GetSongFoldersAsync(string databasePath)
            => FoldersFailure is null ? Task.FromResult<IReadOnlyList<SongFolderSummary>>(Folders) : throw FoldersFailure;

        public Task<IReadOnlyList<SongSummary>> GetSongsAsync(string databasePath, int? folderNo = null)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<DeletedSongSummary>> GetDeletedSongsAsync(string databasePath)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SaveFolderAsync(string databasePath, string backupRoot, SongFolderWriteModel folder)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SoftDeleteFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<FolderDeleteRequest> deletes)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> RecoverFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<FolderRecoveryRequest> recoveries)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SaveSongAsync(string databasePath, string backupRoot, SongWriteModel song)
        {
            LastDatabasePath = databasePath;
            LastBackupRoot = backupRoot;
            LastSong = song;
            var songId = song.SongId ?? 44;
            return Task.FromResult(new AdminDatabaseWriteReport(
                Succeeded: true,
                AdminDatabaseWriteOperation.SaveSong,
                databasePath,
                Path.Combine(backupRoot, "backup.db"),
                [songId],
                [song.FolderNo],
                Issues: []));
        }

        public Task<AdminDatabaseWriteReport> MoveSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongMoveRequest> moves)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SoftDeleteSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongDeleteRequest> deletes)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> RecoverSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongRecoveryRequest> recoveries)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> ReorderFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<FolderOrderRequest> order)
            => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> ReorderSongsAsync(string databasePath, string backupRoot, int folderNo, IReadOnlyList<SongOrderRequest> order)
            => throw new NotSupportedException();
    }

    private sealed class FakeLiveSessionService : ILiveSessionService
    {
        public event EventHandler<LiveSessionChangedEventArgs>? SessionChanged;

        public LiveSessionSnapshot Current { get; set; } = LiveSessionSnapshot.Off;

        public void GoLive(LiveQueueItem item, string outputMonitorName)
        {
            Current = new LiveSessionSnapshot(LiveState.Active, item.Title, outputMonitorName, IsBlackout: false, item.Kind);
            SessionChanged?.Invoke(this, new LiveSessionChangedEventArgs(Current));
        }

        public void UpdateHiddenContent(LiveQueueItem item, string outputMonitorName)
        {
            if (Current.State != LiveState.Hidden)
            {
                return;
            }

            Current = Current with
            {
                CurrentItemTitle = item.Title,
                OutputMonitorName = outputMonitorName,
                CurrentItemKind = item.Kind,
                CurrentItemId = item.Id,
            };
            SessionChanged?.Invoke(this, new LiveSessionChangedEventArgs(Current));
        }

        public void HideOutput(bool blackout)
        {
            Current = Current with { State = LiveState.Hidden, IsBlackout = blackout, IsCleared = false };
            SessionChanged?.Invoke(this, new LiveSessionChangedEventArgs(Current));
        }

        public void ClearOutput()
        {
            Current = Current with { State = LiveState.Hidden, IsBlackout = false, IsCleared = true };
            SessionChanged?.Invoke(this, new LiveSessionChangedEventArgs(Current));
        }

        public void Restore()
        {
            if (Current.State != LiveState.Hidden)
            {
                return;
            }

            Current = Current with { State = LiveState.Active, IsBlackout = false, IsCleared = false };
            SessionChanged?.Invoke(this, new LiveSessionChangedEventArgs(Current));
        }

        public void Refresh()
            => SessionChanged?.Invoke(this, new LiveSessionChangedEventArgs(Current));

        public void SetAudienceAlert(bool parental, OutputAlert? alert) { }

        public void SetLyricsAlertMessage(string text)
        {
            Current = Current with { LyricsAlertMessage = text.Trim() };
            SessionChanged?.Invoke(this, new LiveSessionChangedEventArgs(Current));
        }

        public void SetReferenceAlert(bool visible, string text)
        {
            Current = Current with
            {
                IsReferenceAlertVisible = visible,
                ReferenceAlertText = visible ? text : string.Empty,
            };
            SessionChanged?.Invoke(this, new LiveSessionChangedEventArgs(Current));
        }

        public void Stop()
        {
            Current = LiveSessionSnapshot.Off;
            SessionChanged?.Invoke(this, new LiveSessionChangedEventArgs(Current));
        }
    }

    private sealed class FakeLiveSafetyPrompt : ILiveSafetyPrompt
    {
        public bool NextResult { get; init; } = true;

        public LiveSafetyRequest? LastRequest { get; private set; }

        public Task<bool> ConfirmAsync(LiveSafetyRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(NextResult);
        }
    }

    private sealed class TempSongEditorSettings : IDisposable
    {
        private TempSongEditorSettings(string root)
        {
            Root = root;
            Directory.CreateDirectory(root);
            BackupRoot = Path.Combine(root, "ConfiguredBackups");
            Settings = new SettingsService(new SettingsServiceOptions(
                Path.Combine(root, "settings.json"),
                Path.Combine(root, "SettingsBackups")));
        }

        public string Root { get; }

        public string BackupRoot { get; }

        public ISettingsService Settings { get; }

        public string AdminDatabasePath { get; private set; } = "";

        public static TempSongEditorSettings Create()
            => new(Path.Combine(Path.GetTempPath(), $"Axicultus_SongEditor_{Guid.NewGuid():N}"));

        public void CreateAdminDatabaseFile(string fileName)
        {
            AdminDatabasePath = Path.Combine(Root, fileName);
            File.WriteAllText(AdminDatabasePath, "");
        }

        // A song database with the columns FrmEditItem writes, and two folders in use.
        public void CreateSongDatabase(string fileName)
        {
            AdminDatabasePath = Path.Combine(Root, fileName);
            Execute(
                """
                CREATE TABLE FOLDER (FolderNo INTEGER PRIMARY KEY, Name TEXT, Use TEXT);
                INSERT INTO FOLDER (FolderNo, Name, Use) VALUES (1, 'Morning', '1'), (2, 'Evening', '1');
                CREATE TABLE SONG (
                    SONGID INTEGER PRIMARY KEY, TITLE_1 TEXT, TITLE_2 TEXT, SONG_NUMBER INTEGER, FOLDERNO INTEGER,
                    LYRICS TEXT, SEQUENCE TEXT, WRITER TEXT, COPYRIGHT TEXT, CAPO INTEGER, TIMING TEXT, "KEY" TEXT,
                    MSC TEXT, CATEGORY TEXT, LICENCE_ADMIN1 TEXT, LICENCE_ADMIN2 TEXT, BOOK_REFERENCE TEXT,
                    USER_REFERENCE TEXT, SETTINGS TEXT, FORMATDATA TEXT, CJK_WordCount TEXT, CJK_StrokeCount TEXT,
                    LastModified TEXT, OldFolder INTEGER DEFAULT 0);
                """,
                []);
        }

        public int InsertSong(Dictionary<string, object> columns)
        {
            var names = columns.Keys.ToList();
            Execute(
                $"INSERT INTO SONG ({string.Join(", ", names.Select(name => $"\"{name}\""))}) VALUES ({string.Join(", ", names.Select((_, index) => $"@p{index}"))});",
                names.Select((name, index) => new System.Data.SQLite.SQLiteParameter($"@p{index}", columns[name])).ToArray());
            return 1;
        }

        public Dictionary<string, object> ReadSong(int songId)
        {
            using var connection = new System.Data.SQLite.SQLiteConnection($"Data Source={AdminDatabasePath};Pooling=False");
            connection.Open();
            using var command = new System.Data.SQLite.SQLiteCommand(
                """
                SELECT TITLE_1, TITLE_2, SONG_NUMBER, FOLDERNO, LYRICS, SEQUENCE, WRITER, COPYRIGHT, CAPO, TIMING, "KEY", MSC, CATEGORY,
                       LICENCE_ADMIN1, LICENCE_ADMIN2, BOOK_REFERENCE, USER_REFERENCE, SETTINGS, FORMATDATA
                FROM SONG WHERE SONGID = @id;
                """,
                connection);
            command.Parameters.AddWithValue("@id", songId);
            using var reader = command.ExecuteReader();
            reader.Read().Should().BeTrue();
            return Enumerable.Range(0, reader.FieldCount)
                .ToDictionary(reader.GetName, index => reader.GetValue(index) is long number ? (int)number : reader.GetValue(index));
        }

        private void Execute(string sql, System.Data.SQLite.SQLiteParameter[] parameters)
        {
            using var connection = new System.Data.SQLite.SQLiteConnection($"Data Source={AdminDatabasePath};Pooling=False");
            connection.Open();
            using var command = new System.Data.SQLite.SQLiteCommand(sql, connection);
            command.Parameters.AddRange(parameters);
            command.ExecuteNonQuery();
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Root, recursive: true);
            }
            catch
            {
            }
        }
    }
}
