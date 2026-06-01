using System.Collections.Generic;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class WorshipListValidatorTests
{
    // 어떤 경로도 존재하지 않는다고 보는 판정(파일 누락 시나리오).
    private static readonly System.Func<string, bool> NoneExist = _ => false;

    // 모든 경로가 존재한다고 보는 판정.
    private static readonly System.Func<string, bool> AllExist = _ => true;

    [Fact]
    public void Validate_EmptyOrNull_NoProblems()
    {
        var sut = new WorshipListValidator(AllExist);

        sut.Validate(null).Should().BeEmpty();
        sut.Validate(new List<LiveQueueItem>()).Should().BeEmpty();
    }

    [Fact]
    public void Validate_NonFileItems_AreSkipped()
    {
        // 곡·성경·공지처럼 ContentPath 가 없는 항목은 파일 검사 대상이 아니다(없는 파일로 오판하지 않음).
        var items = new[]
        {
            new LiveQueueItem("song:1", "은혜로다", LiveItemKinds.Song),
            new LiveQueueItem("bible:1", "창세기 1:1", LiveItemKinds.Bible),
            new LiveQueueItem("notice:1", "공지", LiveItemKinds.Notice),
        };
        var sut = new WorshipListValidator(NoneExist);

        sut.Validate(items).Should().BeEmpty();
    }

    [Fact]
    public void Validate_PowerPointWithExistingFile_NoProblem()
    {
        var items = new[]
        {
            new LiveQueueItem("ppt:a", "찬양 PPT", LiveItemKinds.PowerPoint) { ContentPath = @"C:\decks\a.pptx" },
        };
        var sut = new WorshipListValidator(AllExist);

        sut.Validate(items).Should().BeEmpty();
    }

    [Fact]
    public void Validate_PowerPointWithMissingFile_ReportsFileNotFound()
    {
        var item = new LiveQueueItem("ppt:a", "찬양 PPT", LiveItemKinds.PowerPoint) { ContentPath = @"C:\decks\gone.pptx" };
        var sut = new WorshipListValidator(NoneExist);

        var problems = sut.Validate(new[] { item });

        problems.Should().ContainSingle();
        problems[0].Item.Should().Be(item);
        problems[0].Kind.Should().Be(WorshipItemProblemKind.FileNotFound);
        problems[0].Message.Should().Contain("찬양 PPT").And.Contain("찾을 수 없습니다");
    }

    [Fact]
    public void Validate_PowerPointWithEmptyPath_ReportsMissingFilePath()
    {
        // PowerPoint 항목인데 연결 파일이 없으면 명백한 결함(재생할 게 없음).
        var item = new LiveQueueItem("ppt:b", "빈 PPT", LiveItemKinds.PowerPoint);
        var sut = new WorshipListValidator(AllExist);

        var problems = sut.Validate(new[] { item });

        problems.Should().ContainSingle();
        problems[0].Kind.Should().Be(WorshipItemProblemKind.MissingFilePath);
    }

    [Fact]
    public void Validate_MediaFileMissing_ReportsFileNotFound()
    {
        var item = new LiveQueueItem("media:v", "찬양 영상", LiveItemKinds.Media) { ContentPath = @"C:\media\v.mp4" };
        var sut = new WorshipListValidator(NoneExist);

        var problems = sut.Validate(new[] { item });

        problems.Should().ContainSingle().Which.Kind.Should().Be(WorshipItemProblemKind.FileNotFound);
    }

    [Fact]
    public void Validate_DeviceMedia_IsSkippedEvenWithNonexistentPath()
    {
        // 라이브 카메라/캡처 장치는 파일이 아니라 장치라 "파일 존재" 검사를 하지 않는다(장치 ID 를 파일로 오판 금지).
        var items = new[]
        {
            new LiveQueueItem("cam", "라이브 카메라", "LiveCamera") { ContentPath = "device-0" },
            new LiveQueueItem("cap", "캡처", "CaptureDevice"),
        };
        var sut = new WorshipListValidator(NoneExist);

        sut.Validate(items).Should().BeEmpty();
    }

    [Fact]
    public void Validate_MultipleProblems_PreservesOrder()
    {
        var ppt = new LiveQueueItem("ppt:a", "PPT", LiveItemKinds.PowerPoint) { ContentPath = @"C:\a.pptx" };
        var ok = new LiveQueueItem("song:1", "정상 곡", LiveItemKinds.Song);
        var media = new LiveQueueItem("media:v", "영상", LiveItemKinds.Media) { ContentPath = @"C:\v.mp4" };
        var sut = new WorshipListValidator(NoneExist);

        var problems = sut.Validate(new[] { ppt, ok, media });

        problems.Should().HaveCount(2);
        problems[0].Item.Should().Be(ppt, "입력 순서 유지");
        problems[1].Item.Should().Be(media);
    }
}
