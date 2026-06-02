using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class ExternalFileClassifierTests
{
    [Theory]
    [InlineData(@"C:\decks\sermon.ppt", ExternalFileKind.PowerPoint)]
    [InlineData(@"C:\decks\sermon.pptx", ExternalFileKind.PowerPoint)]
    [InlineData(@"C:\decks\SERMON.PPTX", ExternalFileKind.PowerPoint)] // 대소문자 무시
    [InlineData(@"D:\clip.mp4", ExternalFileKind.Media)]
    [InlineData(@"D:\clip.MOV", ExternalFileKind.Media)]
    [InlineData(@"D:\song.mp3", ExternalFileKind.Media)]
    [InlineData(@"D:\song.wav", ExternalFileKind.Media)]
    [InlineData(@"C:\image.png", ExternalFileKind.Image)]   // 이미지 = 배경 드롭용(큐 아님)
    [InlineData(@"C:\photo.JPG", ExternalFileKind.Image)]   // 대소문자 무시
    [InlineData(@"C:\pic.jpeg", ExternalFileKind.Image)]
    [InlineData(@"C:\pic.bmp", ExternalFileKind.Image)]
    [InlineData(@"C:\anim.gif", ExternalFileKind.Image)]
    [InlineData(@"C:\notes.txt", ExternalFileKind.Unsupported)]  // 텍스트
    [InlineData(@"C:\doc.docx", ExternalFileKind.Unsupported)]   // Word 는 드래그 미지원(메뉴로)
    [InlineData(@"C:\noext", ExternalFileKind.Unsupported)]
    public void Classify_MapsByExtension(string path, ExternalFileKind expected)
        => ExternalFileClassifier.Classify(path).Should().Be(expected);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Classify_BlankPath_IsUnsupported(string? path)
        => ExternalFileClassifier.Classify(path).Should().Be(ExternalFileKind.Unsupported);
}
