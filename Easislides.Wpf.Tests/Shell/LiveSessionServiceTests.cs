using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class LiveSessionServiceTests
{
    [Fact]
    public void NewService_DefaultsToOffSnapshot()
    {
        var sut = new LiveSessionService();

        sut.Current.State.Should().Be(LiveState.Off);
        sut.Current.CurrentItemTitle.Should().BeEmpty();
        sut.Current.OutputMonitorName.Should().BeEmpty();
        sut.Current.IsBlackout.Should().BeFalse();
    }

    [Fact]
    public void GoLive_CarriesItemIdInSnapshot_ForTransitionItemVsSlideDetection()
    {
        // 스냅샷에 항목 Id 를 실어 출력 VM 이 "항목 전환 vs 절·슬라이드 이동"을 구분할 수 있게 한다(증분92).
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-42", "은혜로다"), "Display 1");

        sut.Current.CurrentItemId.Should().Be("song-42", "GoLive 가 항목 Id 를 스냅샷에 실어야 함");
    }

    [Fact]
    public void Restore_FromHidden_ReturnsToActiveAndClearsBlackoutKeepingContent()
    {
        // 숨김/블랙에서 복귀 — 상태만 Active 로 되돌리고 직전 항목(콘텐츠)은 보존(§7.3-B 화면 제어).
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-1", "은혜로다"), "Display 1");
        sut.HideOutput(blackout: true);
        sut.Current.State.Should().Be(LiveState.Hidden);

        sut.Restore();

        sut.Current.State.Should().Be(LiveState.Active);
        sut.Current.IsBlackout.Should().BeFalse();
        sut.Current.CurrentItemTitle.Should().Be("은혜로다", "복귀 시 직전 항목 보존");
    }

    [Fact]
    public void Restore_WhenNotHidden_IsNoOp()
    {
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-1", "은혜로다"), "Display 1");

        sut.Restore();

        sut.Current.State.Should().Be(LiveState.Active, "Active 상태에선 Restore 가 아무 변화 없음");
    }

    [Fact]
    public void GoLive_UpdatesSnapshotAndRaisesChanged()
    {
        var sut = new LiveSessionService();
        var changes = new List<LiveSessionSnapshot>();
        sut.SessionChanged += (_, e) => changes.Add(e.Snapshot);

        sut.GoLive(new LiveQueueItem("song-3", "주일찬양 #3 은혜로다"), "모니터 2");

        sut.Current.State.Should().Be(LiveState.Active);
        sut.Current.CurrentItemTitle.Should().Be("주일찬양 #3 은혜로다");
        sut.Current.OutputMonitorName.Should().Be("모니터 2");
        sut.Current.IsBlackout.Should().BeFalse();
        changes.Should().ContainSingle().Which.Should().Be(sut.Current);
    }

    [Fact]
    public void HideOutput_MarksSessionHiddenWithoutForgettingCurrentItem()
    {
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-3", "주일찬양 #3 은혜로다"), "모니터 2");

        sut.HideOutput(blackout: false);

        sut.Current.State.Should().Be(LiveState.Hidden);
        sut.Current.CurrentItemTitle.Should().Be("주일찬양 #3 은혜로다");
        sut.Current.IsBlackout.Should().BeFalse();
    }

    [Fact]
    public void UpdateHiddenContent_ReplacesPayloadWithoutRestoringOutput()
    {
        // FrmMain btnToOutput/F8: Black/Clear 로 숨긴 동안에도 다음 복귀 payload 는 새 OutputItem 으로 바뀌어야 한다.
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-live", "이전 찬양", LiveItemKinds.Song) { Lyrics = "[1]\n이전 절" }, "모니터 1");
        sut.HideOutput(blackout: true);
        var changes = new List<LiveSessionSnapshot>();
        sut.SessionChanged += (_, e) => changes.Add(e.Snapshot);

        sut.UpdateHiddenContent(
            new LiveQueueItem("song-next", "다음 찬양", LiveItemKinds.Song) { Lyrics = "[1]\n다음 절" },
            "모니터 2");

        sut.Current.State.Should().Be(LiveState.Hidden, "payload refresh must not flash live output");
        sut.Current.IsBlackout.Should().BeTrue("Black 상태는 유지");
        sut.Current.IsCleared.Should().BeFalse();
        sut.Current.CurrentItemTitle.Should().Be("다음 찬양");
        sut.Current.CurrentItemBodyText.Should().Be("다음 절");
        sut.Current.CurrentItemId.Should().Be("song-next");
        sut.Current.OutputMonitorName.Should().Be("모니터 2");
        changes.Should().ContainSingle().Which.State.Should().Be(LiveState.Hidden);

        sut.Restore();
        sut.Current.State.Should().Be(LiveState.Active);
        sut.Current.CurrentItemTitle.Should().Be("다음 찬양", "복귀 시 갱신된 payload 가 살아나야 함");
    }

    [Fact]
    public void UpdateHiddenContent_PreservesClearedState()
    {
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-live", "이전 찬양", LiveItemKinds.Song) { Lyrics = "[1]\n이전 절" }, "모니터 1");
        sut.ClearOutput();

        sut.UpdateHiddenContent(
            new LiveQueueItem("song-next", "다음 찬양", LiveItemKinds.Song) { Lyrics = "[1]\n다음 절" },
            "모니터 1");

        sut.Current.State.Should().Be(LiveState.Hidden);
        sut.Current.IsCleared.Should().BeTrue("Clear 상태는 유지");
        sut.Current.IsBlackout.Should().BeFalse();
        sut.Current.CurrentItemTitle.Should().Be("다음 찬양");
    }

    [Fact]
    public void BlackoutOutput_MarksHiddenAndBlackout()
    {
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-3", "주일찬양 #3 은혜로다"), "모니터 2");

        sut.HideOutput(blackout: true);

        sut.Current.State.Should().Be(LiveState.Hidden);
        sut.Current.IsBlackout.Should().BeTrue();
    }

    [Fact]
    public void GoLive_PropagatesPreviewSourceAndPixelDimensionsFromBitmap()
    {
        var preview = CreateBitmap(1920, 1080);
        var item = new LiveQueueItem("song-3", "주일찬양 #3 은혜로다")
        {
            PreviewSource = preview,
            PreviewFillMode = ImageFillMode.Fill,
            SlideNumber = 4,
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemPreviewSource.Should().BeSameAs(preview);
        sut.Current.CurrentItemPreviewFillMode.Should().Be(ImageFillMode.Fill);
        sut.Current.CurrentItemPreviewPixelWidth.Should().Be(1920);
        sut.Current.CurrentItemPreviewPixelHeight.Should().Be(1080);
    }

    [Fact]
    public void GoLive_WithoutPreviewSource_LeavesPreviewFieldsAtDefaults()
    {
        var item = new LiveQueueItem("song-3", "주일찬양 #3 은혜로다");
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemPreviewSource.Should().BeNull();
        sut.Current.CurrentItemPreviewPixelWidth.Should().Be(0);
        sut.Current.CurrentItemPreviewPixelHeight.Should().Be(0);
    }

    [Fact]
    public void GoLive_WithSongLyrics_CarriesFirstVerseByDefault()
    {
        // 절 단위 페이지네이션(PR B): LyricsPageIndex=0(기본)이면 첫 절만 출력에 보인다.
        // 작성 마커([1]/[~코드])는 출력용으로 제거된다.
        var item = new LiveQueueItem("song-3", "은혜로다")
        {
            Lyrics = "[~G D]\n[1]\n1절 가사\n둘째 줄\n[2]\n2절 가사",
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be("1절 가사\n둘째 줄");
        sut.Current.CurrentItemBodyText.Should().NotContain("[");
    }

    [Fact]
    public void GoLive_WithShowNotationsOff_HidesChords()
    {
        // "코드 표시" off(기본)면 '»' 뒤 코드는 회중 화면에 안 보인다(가사만).
        var item = new LiveQueueItem("song-3", "은혜로다")
        {
            Lyrics = "[1]\nAmazing grace » G  C\nHow sweet » D7",
            ShowNotations = false,
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be("Amazing grace\nHow sweet");
        sut.Current.CurrentItemBodyText.Should().NotContain("G  C").And.NotContain("»");
    }

    [Fact]
    public void GoLive_ChorusPage_SetsCurrentPageIsChorus()
    {
        // [C] 절을 송출 중이면 CurrentPageIsChorus=true(강조 후렴만 효과 판정).
        var item = new LiveQueueItem("song-c", "은혜로다", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n첫 절\n[C]\n후렴 가사",
            LyricsPageIndex = 1, // 후렴 페이지.
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentPageIsChorus.Should().BeTrue();
    }

    [Fact]
    public void GoLive_SetsCurrentSectionLabel_ForVerseHeading()
    {
        // 절 헤딩(FrmMain Def_Head All) — 현재 송출 절의 섹션 라벨이 스냅샷에 실린다.
        // 후렴 판정과 같은 라벨 경로를 재사용하므로, [C] 페이지의 라벨은 후렴 라벨("C")이어야 한다.
        const string lyrics = "[1]\n첫 절\n[C]\n후렴 가사";
        var sut = new LiveSessionService();

        var chorusItem = new LiveQueueItem("song-c", "은혜로다", LiveItemKinds.Song) { Lyrics = lyrics, LyricsPageIndex = 1 };
        sut.GoLive(chorusItem, "모니터 2");
        sut.Current.CurrentSectionLabel.Should().Be("C", "후렴 페이지의 섹션 라벨");

        var verseItem = new LiveQueueItem("song-v", "은혜로다", LiveItemKinds.Song) { Lyrics = lyrics, LyricsPageIndex = 0 };
        sut.GoLive(verseItem, "모니터 2");
        sut.Current.CurrentSectionLabel.Should().Be("1", "1절 페이지의 섹션 라벨");
    }

    [Fact]
    public void GoLive_NonChorusPage_CurrentPageIsChorusFalse()
    {
        var item = new LiveQueueItem("song-v", "은혜로다", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n첫 절\n[C]\n후렴 가사",
            LyricsPageIndex = 0, // 1절(후렴 아님).
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentPageIsChorus.Should().BeFalse();
    }

    [Fact]
    public void GoLive_DualLanguageUnlabeledIntro_DoesNotMisdetectChorus()
    {
        // 이중 언어 + 라벨 없는 머리말 절이 섞이면 라벨↔페이지가 어긋난다. 그 경우 엉뚱한 절을 후렴으로
        // 오인하지 않고 후렴 판정을 비활성(false)한다 — 안전 저하(잘못된 강조 방지).
        var item = new LiveQueueItem("song-dual", "은혜로다", LiveItemKinds.Song)
        {
            Lyrics = "머리말 R1\n[region 2]\n머리말 R2\n\n[C]\n후렴 R1\n[region 2]\n후렴 R2",
            LyricsPageIndex = 1, // 실제 후렴 페이지지만, 라벨 어긋남으로 후렴 판정 비활성.
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentPageIsChorus.Should().BeFalse("라벨↔페이지 어긋남 → 후렴 판정 비활성(안전)");
    }

    [Fact]
    public void GoLive_WithBibleItem_CarriesVerseBodyNotJustTitle()
    {
        // 예전엔 성경 항목이 제목만 송출했다 — 이제 구절 본문이 현재 절로 보인다.
        var item = new LiveQueueItem("0;kjv.db;;1;1;1;1;2;", "Genesis 1:1-2 (KJV)", LiveItemKinds.Bible)
        {
            Lyrics = "1:1 In the beginning\n\n1:2 And the earth",
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be("1:1 In the beginning"); // 첫 절.
    }

    [Fact]
    public void GoLive_WithBibleVerse_PreservesGuillemetQuotes()
    {
        // 일부 번역(독일어·프랑스어 등)은 인용부호로 »…« 를 쓴다 — 코드 마커로 오인돼 잘리면 안 된다(성경 본문 보존).
        var item = new LiveQueueItem("0;luther.db;;1;1;3;1;3;", "1 Mose 1:3", LiveItemKinds.Bible)
        {
            Lyrics = "1:3 Und Gott sprach: »Es werde Licht!« Und es ward Licht.",
            // 코드 표시가 켜져 있어도 성경 본문은 코드 전처리를 타지 않아 그대로 보존돼야 한다.
            ShowNotations = true,
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be("1:3 Und Gott sprach: »Es werde Licht!« Und es ward Licht.");
    }

    [Fact]
    public void GoLive_WithBibleVerse_NormalizesMojibakeGuillemet()
    {
        // 이중 인코딩된 "Â»" 모지바케도 정규화 후 보호하므로 본문엔 깨끗한 »만 남고 'Â'가 새지 않는다.
        var item = new LiveQueueItem("0;luther.db;;1;1;3;1;3;", "1 Mose 1:3", LiveItemKinds.Bible)
        {
            Lyrics = "1:3 Gott sprach: Â»Licht!Â«",
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be("1:3 Gott sprach: »Licht!Â«");
        sut.Current.CurrentItemBodyText.Should().NotContain("Â»");
    }

    [Fact]
    public void GoLive_WithDualLanguageBible_CarriesBothRegions()
    {
        // 이중 언어 성경: 주 언어(Region1)와 보조 언어(Region2)를 같은 절로 함께 송출.
        var item = new LiveQueueItem("0;kjv.db;krv.db;43;3;16;3;16;", "John 3:16 (KJV/개역)", LiveItemKinds.Bible)
        {
            Lyrics = "3:16 For God so loved the world.\n[region 2]\n하나님이 세상을 이처럼 사랑하사.",
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be("3:16 For God so loved the world.");
        sut.Current.CurrentItemBodyText2.Should().Be("하나님이 세상을 이처럼 사랑하사.");
    }

    [Fact]
    public void GoLive_WithShowNotationsOn_PutsChordsAboveLyrics()
    {
        // "코드 표시" on 이면 각 가사 줄 위에 코드 줄이 함께 송출된다(레거시 ShowNotations).
        var item = new LiveQueueItem("song-3", "은혜로다")
        {
            Lyrics = "[1]\nAmazing grace » G  C\nHow sweet » D7",
            ShowNotations = true,
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be("G  C\nAmazing grace\nD7\nHow sweet");
    }

    [Fact]
    public void GoLive_WithShowNotationsAndTranspose_ShiftsLiveChords()
    {
        // 코드 표시 on + 조옮김 +2 → 송출 코드만 두 반음 올라간다(C→D, G→A). 가사 불변.
        var item = new LiveQueueItem("song-3", "은혜로다", LiveItemKinds.Song)
        {
            Lyrics = "[1]\nAmazing grace » C  G",
            ShowNotations = true,
            TransposeSemitones = 2,
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be("D  A\nAmazing grace");
    }

    [Fact]
    public void GoLive_WithTransposeButNotationsOff_ChordsStayHidden()
    {
        // 코드 표시 off 면 조옮김이 있어도 코드는 안 보인다(무회귀).
        var item = new LiveQueueItem("song-3", "은혜로다", LiveItemKinds.Song)
        {
            Lyrics = "[1]\nAmazing grace » C  G",
            ShowNotations = false,
            TransposeSemitones = 2,
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be("Amazing grace");
    }

    [Fact]
    public void GoLive_WithShowNotationsOn_KeepsSameVersePagination()
    {
        // 코드 줄을 끼워도 절 경계는 그대로라 같은 페이지 인덱스가 같은 절을 가리킨다(코드만 추가).
        var item = new LiveQueueItem("song-3", "은혜로다")
        {
            Lyrics = "[1]\n1절 » C\n[2]\n2절 » G",
            LyricsPageIndex = 1,
            ShowNotations = true,
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be("G\n2절", "2절 페이지에 코드 줄이 위에 붙는다");
    }

    [Fact]
    public void GoLive_WithSongLyricsAtPage1_CarriesSecondVerse()
    {
        // LyricsPageIndex=1이면 두 번째 절만 출력에 보인다(MainViewModel이 이동 시 얹어 전달).
        var item = new LiveQueueItem("song-3", "은혜로다")
        {
            Lyrics = "[~G D]\n[1]\n1절 가사\n둘째 줄\n[2]\n2절 가사",
            LyricsPageIndex = 1,
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be("2절 가사");
    }

    [Fact]
    public void GoLive_CarriesPositionLabelToSnapshot()
    {
        // 위치 인디케이터(§7.3-A): 항목의 PositionLabel("2/4" 등)이 스냅샷으로 전달된다.
        var item = new LiveQueueItem("song-3", "은혜로다") { Lyrics = "[1]\n1절\n[2]\n2절", PositionLabel = "1/2" };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemPositionLabel.Should().Be("1/2");
    }

    [Fact]
    public void GoLive_WithoutLyrics_LeavesBodyTextEmpty()
    {
        // 가사 없는 항목(PPT/미디어/공지 등)은 본문이 비어 출력에 텍스트가 나타나지 않는다.
        var item = new LiveQueueItem("ppt-1", "주보 PPT", LiveItemKinds.PowerPoint);
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().BeEmpty();
    }

    [Fact]
    public void GoLive_WithSongFormatData_CarriesRegion1OverrideColorsToSnapshot()
    {
        // 곡별 FormatData(레거시 v32)에 글자색(29)·배경색(26)이 있으면 region1 색을 스냅샷에 실어
        // 출력 렌더러가 운영 기본 색 대신 그 곡의 색으로 송출하게 한다(미리보기와 동일 규약).
        // 29=-65536(불투명 빨강), 26=-16776961(불투명 파랑).
        var item = new LiveQueueItem("song-3", "은혜로다")
        {
            Lyrics = "[1]\n1절 가사",
            FormatData = "29=-65536>26=-16776961>",
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.OverrideTextColorArgb.Should().Be(-65536, "29 = region1 글자색");
        sut.Current.OverrideBackgroundColorArgb.Should().Be(-16776961, "26 = region1 배경색");
    }

    [Fact]
    public void GoLive_WithoutFormatData_LeavesOverrideColorsNull()
    {
        // FormatData 가 없으면 오버라이드는 null → 렌더러는 운영 기본 색을 그대로 쓴다(무회귀).
        var item = new LiveQueueItem("song-3", "은혜로다") { Lyrics = "[1]\n1절 가사" };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.OverrideTextColorArgb.Should().BeNull();
        sut.Current.OverrideBackgroundColorArgb.Should().BeNull();
    }

    [Theory]
    [InlineData("31=1>", LyricsTextAlignment.Left)]   // 레거시 1 = Near(왼쪽)
    [InlineData("31=2>", LyricsTextAlignment.Center)] // 2 = 가운데
    [InlineData("31=3>", LyricsTextAlignment.Right)]  // 3 = Far(오른쪽)
    public void GoLive_WithSongFormatDataAlignment_CarriesOverrideAlignment(string formatData, LyricsTextAlignment expected)
    {
        // 곡별 FormatData 정렬(31, region1)을 스냅샷에 실어 출력이 그 곡의 정렬로 가사를 송출하게 한다.
        var item = new LiveQueueItem("song-3", "은혜로다") { Lyrics = "[1]\n1절 가사", FormatData = formatData };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.OverrideTextAlignment.Should().Be(expected);
    }

    [Fact]
    public void GoLive_WithoutAlignmentField_LeavesOverrideAlignmentNull()
    {
        // 정렬 항목(31)이 없으면 오버라이드는 null → 운영 기본 정렬 유지(무회귀).
        var item = new LiveQueueItem("song-3", "은혜로다") { Lyrics = "[1]\n1절 가사", FormatData = "29=-65536>" };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.OverrideTextAlignment.Should().BeNull();
    }

    [Fact]
    public void GoLive_WithOutOfRangeAlignment_LeavesOverrideAlignmentNull()
    {
        // 범위 밖 정렬값(31=9)은 디코더가 막아 null → 종단에서도 오버라이드 없음(운영 기본 정렬 유지).
        var item = new LiveQueueItem("song-3", "은혜로다") { Lyrics = "[1]\n1절 가사", FormatData = "31=9>" };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.OverrideTextAlignment.Should().BeNull();
    }

    [Fact]
    public void GoLive_WithSongFormatDataVerticalAlignmentAndMargins_CarriesOutputPositionOverrides()
    {
        var item = new LiveQueueItem("song-3", "Song")
        {
            Lyrics = "[1]\nbody",
            FormatData = "63=2>64=4>65=5>66=6>",
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "Display 1");

        sut.Current.OverrideVerticalAlignment.Should().Be(LyricsVerticalAlignment.Bottom);
        sut.Current.OverrideBodyLeftMargin.Should().Be(4);
        sut.Current.OverrideBodyRightMargin.Should().Be(5);
        sut.Current.OverrideBodyBottomMargin.Should().Be(6);
    }

    [Fact]
    public void GoLive_WithSongFormatDataFont_CarriesFontNameAndPixelSize()
    {
        // 곡별 FormatData 폰트명(43)·크기(47, 레거시 pt)를 스냅샷에 실어 출력이 그 곡의 글꼴로 송출하게 한다.
        // 레거시 폰트 크기는 pt(WinForms 기본) → WPF px(DIP): 48pt × 96/72 = 64px.
        var item = new LiveQueueItem("song-3", "은혜로다")
        {
            Lyrics = "[1]\n1절 가사",
            FormatData = "43=Batang>47=48>",
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.OverrideFontName.Should().Be("Batang", "43 = region1 폰트명");
        sut.Current.OverrideFontSizePx.Should().Be(64, "47 = region1 폰트 크기(48pt → 64px)");
    }

    [Fact]
    public void GoLive_WithoutFontFields_LeavesFontOverridesNull()
    {
        // 폰트 항목(43/47)이 없으면 오버라이드는 null → 운영/테마 기본 글꼴·크기 유지(무회귀).
        var item = new LiveQueueItem("song-3", "은혜로다") { Lyrics = "[1]\n1절 가사", FormatData = "29=-65536>" };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.OverrideFontName.Should().BeNull();
        sut.Current.OverrideFontSizePx.Should().BeNull();
    }

    [Fact]
    public void GoLive_WithBlankFontName_LeavesFontNameNull()
    {
        // 폰트명이 공백뿐이면(43= ) 디코드 후에도 의미 없으므로 null → 기본 글꼴 유지.
        var item = new LiveQueueItem("song-3", "은혜로다") { Lyrics = "[1]\n1절 가사", FormatData = "43=   >47=12>" };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.OverrideFontName.Should().BeNull("공백 폰트명은 무시");
        sut.Current.OverrideFontSizePx.Should().Be(16, "12pt → 16px(크기는 유효)");
    }

    [Fact]
    public void GoLive_WithSongFormatDataBackgroundImage_CarriesImagePath()
    {
        // 곡별 FormatData 배경 이미지(61) 경로를 스냅샷에 실어 출력이 색 배경 대신 그 곡의 이미지를 표시하게 한다.
        var item = new LiveQueueItem("song-3", "은혜로다")
        {
            Lyrics = "[1]\n1절 가사",
            FormatData = @"61=C:\Backgrounds\sky.jpg>62=0>",
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.OverrideBackgroundImagePath.Should().Be(@"C:\Backgrounds\sky.jpg", "61 = region1 배경 이미지 경로");
        sut.Current.OverrideBackgroundImageMode.Should().Be(LyricsBackgroundMode.Tile, "62=0 = legacy Tile Image");
    }

    [Fact]
    public void GoLive_WithDualLanguageLyrics_CarriesRegion1AndRegion2Separately()
    {
        // 이중 언어([region 2]) 곡이 라이브로 나가면 Region1 은 BodyText, Region2 는 BodyText2 로 따로 실린다
        // (출력이 영역별 스타일·색으로 동시 송출). region2 색(30)도 함께 적재.
        var item = new LiveQueueItem("song-3", "은혜로다")
        {
            Lyrics = "[1]\nAmazing grace\n[region 2]\n놀라운 은혜",
            FormatData = "30=-16776961>", // region2 글자색(파랑)
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be("Amazing grace", "Region1 은 본문1");
        sut.Current.CurrentItemBodyText2.Should().Be("놀라운 은혜", "Region2 는 본문2");
        sut.Current.OverrideTextColorArgb2.Should().Be(-16776961, "30 = region2 글자색");
    }

    [Fact]
    public void GoLive_DualLanguage_CarriesRegion2Alignment()
    {
        // region2 정렬(32=3→오른쪽)이 스냅샷에 실린다 — 이중 언어 Region2 본문 정렬 독립.
        var item = new LiveQueueItem("song-3", "은혜로다")
        {
            Lyrics = "[1]\nAmazing grace\n[region 2]\n놀라운 은혜",
            FormatData = "31=1>32=3>", // region1=왼쪽, region2=오른쪽
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.OverrideTextAlignment.Should().Be(LyricsTextAlignment.Left, "31 = region1 정렬");
        sut.Current.OverrideTextAlignment2.Should().Be(LyricsTextAlignment.Right, "32 = region2 정렬");
    }

    [Fact]
    public void GoLive_CarriesSongNumberToSnapshot()
    {
        // 곡 번호가 스냅샷에 실려 출력 "곡 번호 표시"에 쓰인다.
        var item = new LiveQueueItem("song-3", "은혜로다") { Lyrics = "[1]\n1절", SongNumber = 123 };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemNumber.Should().Be(123);
    }

    [Fact]
    public void GoLive_NoticeItem_RendersTextVerbatim_NotThroughLyricsFormatter()
    {
        // 공지(InfoScreen)는 자유 텍스트라 가사 마커([광고] 같은 마커-only 줄·» 등)를 해석하지 않고
        // 입력 그대로 본문으로 송출한다(가사 포맷터가 마커 줄을 삭제·손상하지 않도록 우회).
        var noticeText = "[광고] 점심 식사\n주차장 만차";
        var item = new LiveQueueItem(LiveItemKinds.NoticeLiveId, "공지", LiveItemKinds.Notice) { Lyrics = noticeText };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be(noticeText, "공지는 가사 마커 해석 없이 그대로 표시");
        sut.Current.CurrentItemBodyText2.Should().BeEmpty();
    }

    [Fact]
    public void GoLive_CarriesNextTitleToSnapshot()
    {
        // 다음 항목 제목이 스냅샷에 실려 출력 "다음 항목 표시"(Display Panel PrevNext)에 쓰인다.
        var item = new LiveQueueItem("song-3", "은혜로다") { Lyrics = "[1]\n1절", NextTitle = "주 은혜임을" };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemNextTitle.Should().Be("주 은혜임을");
    }

    [Fact]
    public void GoLive_DualLanguage_CarriesRegion2Font()
    {
        // region2 글꼴명(44)·크기(48, pt→px)를 스냅샷에 싣는다 — 이중 언어 Region2 본문 글꼴 독립.
        var item = new LiveQueueItem("song-3", "은혜로다")
        {
            Lyrics = "[1]\nAmazing\n[region 2]\n은혜",
            FormatData = "44=Batang>48=12>",
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.OverrideFontName2.Should().Be("Batang", "44 = region2 폰트명");
        sut.Current.OverrideFontSizePx2.Should().Be(16, "48 = region2 크기(12pt → 16px)");
    }

    [Fact]
    public void GoLive_SingleLanguage_LeavesBodyText2Empty()
    {
        var item = new LiveQueueItem("song-3", "은혜로다") { Lyrics = "[1]\nAmazing grace" };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText2.Should().BeEmpty("단일 영역은 Region2 없음(무회귀)");
    }

    [Fact]
    public void GoLive_DualLanguage_VerseWithoutRegion2_ShowsOnlyRegion1()
    {
        // 혼합 곡(1절은 R1/R2, 2절은 R1만) — region2 없는 절은 Region1 만 송출(빈 줄 없이). 정렬 유지.
        var item = new LiveQueueItem("song-3", "은혜로다")
        {
            Lyrics = "[1]\nAmazing grace\n[region 2]\n놀라운 은혜\n[2]\nGrace alone",
            LyricsPageIndex = 1, // 2절(R2 없음)
        };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be("Grace alone", "region2 없는 절은 Region1 만");
    }

    [Fact]
    public void GoLive_SingleLanguageLyrics_BodyUnchanged()
    {
        // 단일 영역 곡은 기존 경로 그대로(무회귀) — region2 결합 없음.
        var item = new LiveQueueItem("song-3", "은혜로다") { Lyrics = "[1]\nAmazing grace\n둘째 줄" };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.CurrentItemBodyText.Should().Be("Amazing grace\n둘째 줄");
    }

    [Fact]
    public void GoLive_WithoutBackgroundImage_LeavesOverrideNull()
    {
        // 배경 이미지 항목(61)이 없으면 오버라이드는 null → 색 배경 유지(무회귀).
        var item = new LiveQueueItem("song-3", "은혜로다") { Lyrics = "[1]\n1절 가사", FormatData = "29=-65536>" };
        var sut = new LiveSessionService();

        sut.GoLive(item, "모니터 2");

        sut.Current.OverrideBackgroundImagePath.Should().BeNull();
    }

    [Fact]
    public void Restore_FromHidden_PreservesBodyText()
    {
        // 숨김→복귀 시 가사 본문도 보존되어야 직전 곡이 그대로 다시 보인다.
        var item = new LiveQueueItem("song-3", "은혜로다") { Lyrics = "1절 가사" };
        var sut = new LiveSessionService();
        sut.GoLive(item, "모니터 2");
        sut.HideOutput(blackout: false);

        sut.Restore();

        sut.Current.CurrentItemBodyText.Should().Be("1절 가사");
    }

    private static BitmapSource CreateBitmap(int width, int height)
    {
        var stride = width * 4;
        var pixels = new byte[stride * height];
        var bitmap = BitmapSource.Create(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, palette: null, pixels, stride);
        bitmap.Freeze();
        return bitmap;
    }

    [Fact]
    public void Stop_ReturnsToOffAndClearsBlackout()
    {
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-3", "주일찬양 #3 은혜로다"), "모니터 2");
        sut.HideOutput(blackout: true);

        sut.Stop();

        sut.Current.State.Should().Be(LiveState.Off);
        sut.Current.CurrentItemTitle.Should().BeEmpty();
        sut.Current.OutputMonitorName.Should().BeEmpty();
        sut.Current.IsBlackout.Should().BeFalse();
    }

    // ─── ClearOutput (배경 유지·콘텐츠 비우기 — 레거시 LiveClear) ──────────────

    [Fact]
    public void ClearOutput_FromActive_EntersClearedKeepingContentAndNoBlackout()
    {
        // 비우기: 콘텐츠는 숨기되 배경은 유지(Black=완전검정과 구별). 콘텐츠는 보존(복귀 대비).
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-1", "은혜로다") { Lyrics = "1절 가사" }, "Display 1");

        sut.ClearOutput();

        sut.Current.State.Should().Be(LiveState.Hidden);
        sut.Current.IsCleared.Should().BeTrue();
        sut.Current.IsBlackout.Should().BeFalse("비우기는 검정이 아니라 배경 유지");
        sut.Current.CurrentItemBodyText.Should().Be("1절 가사", "복귀 대비 콘텐츠 보존");
    }

    [Fact]
    public void ClearOutput_WhenOff_IsNoOp()
    {
        var sut = new LiveSessionService();

        sut.ClearOutput();

        sut.Current.State.Should().Be(LiveState.Off);
        sut.Current.IsCleared.Should().BeFalse();
    }

    [Fact]
    public void HideOutput_ResetsClearedFlag()
    {
        // Black/Hide 와 Clear 는 상호 배타 — Clear 후 Black 으로 전환하면 IsCleared 가 꺼진다.
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-1", "은혜로다"), "Display 1");
        sut.ClearOutput();

        sut.HideOutput(blackout: true);

        sut.Current.IsCleared.Should().BeFalse();
        sut.Current.IsBlackout.Should().BeTrue();
    }

    [Fact]
    public void Restore_FromCleared_ReturnsToActiveAndResetsClearedFlag()
    {
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-1", "은혜로다") { Lyrics = "1절 가사" }, "Display 1");
        sut.ClearOutput();

        sut.Restore();

        sut.Current.State.Should().Be(LiveState.Active);
        sut.Current.IsCleared.Should().BeFalse();
        sut.Current.CurrentItemBodyText.Should().Be("1절 가사");
    }

    [Fact]
    public void Stop_ClearsClearedFlag()
    {
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-1", "은혜로다"), "Display 1");
        sut.ClearOutput();

        sut.Stop();

        sut.Current.IsCleared.Should().BeFalse();
    }

    // ─── Refresh (출력 강제 재렌더 — 레거시 RefreshOutput) ─────────────────────

    [Fact]
    public void Refresh_RaisesSessionChangedWithCurrentSnapshot()
    {
        // 새로고침: 스냅샷이 바뀌지 않아도 SessionChanged 를 다시 발생시켜 출력을 강제 재렌더한다.
        var sut = new LiveSessionService();
        sut.GoLive(new LiveQueueItem("song-1", "은혜로다"), "Display 1");
        var changes = new List<LiveSessionSnapshot>();
        sut.SessionChanged += (_, e) => changes.Add(e.Snapshot);

        sut.Refresh();

        changes.Should().ContainSingle("Refresh 는 현재 스냅샷으로 한 번 재통지");
        changes[0].Should().Be(sut.Current);
    }

    [Fact]
    public void Refresh_WhenOff_StillRaisesForConsistency()
    {
        var sut = new LiveSessionService();
        var changes = new List<LiveSessionSnapshot>();
        sut.SessionChanged += (_, e) => changes.Add(e.Snapshot);

        sut.Refresh();

        changes.Should().ContainSingle();
        changes[0].State.Should().Be(LiveState.Off);
    }
}
