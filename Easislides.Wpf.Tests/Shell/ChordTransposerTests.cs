using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>
/// 코드(화음) 조옮김 — 레거시 EasiSlides Transpose Up/Down Semi-Tone 대응의 핵심 로직.
/// 루트음을 반음 단위로 올리고/내리고, 접미사(m, 7, maj7, sus4…)와 슬래시 베이스(/G)는 보존한다.
/// </summary>
public class ChordTransposerTests
{
    [Theory]
    [InlineData("G", 2, "A")]
    [InlineData("C", 1, "C#")]
    [InlineData("B", 1, "C")]      // 옥타브 경계
    [InlineData("G", 0, "G")]      // 변화 없음
    [InlineData("G", 12, "G")]     // 한 옥타브 = 제자리
    [InlineData("C", -1, "B")]     // 내림(음수)
    [InlineData("Bb", 2, "C")]     // 플랫 입력
    [InlineData("F#", 1, "G")]     // 샤프 입력
    public void Transpose_Root_ShiftsBySemitones(string chord, int semitones, string expected)
        => ChordTransposer.Transpose(chord, semitones).Should().Be(expected);

    [Theory]
    [InlineData("Am", 2, "Bm")]
    [InlineData("F#m7", 1, "Gm7")]
    [InlineData("Cmaj7", 2, "Dmaj7")]
    [InlineData("Dsus4", -2, "Csus4")]
    public void Transpose_PreservesSuffix(string chord, int semitones, string expected)
        => ChordTransposer.Transpose(chord, semitones).Should().Be(expected);

    [Theory]
    [InlineData("C/G", 2, "D/A")]   // 슬래시 베이스도 함께 조옮김
    [InlineData("Am/E", 1, "A#m/F")]
    public void Transpose_SlashBass_TransposesBoth(string chord, int semitones, string expected)
        => ChordTransposer.Transpose(chord, semitones).Should().Be(expected);

    [Theory]
    [InlineData("", 2, "")]
    [InlineData("   ", 2, "   ")]
    [InlineData("N.C.", 2, "N.C.")] // 코드가 아닌 토큰은 그대로
    public void Transpose_NonChordOrEmpty_Unchanged(string chord, int semitones, string expected)
        => ChordTransposer.Transpose(chord, semitones).Should().Be(expected);

    [Fact]
    public void TransposeLine_ShiftsEveryChordToken_PreservesSpacing()
    {
        // 코드 줄의 모든 코드 토큰을 조옮김하되 공백(정렬)은 보존.
        ChordTransposer.TransposeLine("G   C   D7", 2).Should().Be("A   D   E7");
    }

    [Fact]
    public void TransposeLine_WordsNotStartingAtoG_AreLeftAlone()
    {
        // H~Z 로 시작하는 단어는 코드 루트가 아니므로 그대로 둔다(코드 칸에 실수로 들어간 일반 단어 방어).
        ChordTransposer.TransposeLine("Hello world Sing", 2).Should().Be("Hello world Sing");
    }

    [Fact]
    public void TransposeLine_WordsStartingAtoG_AreTreatedAsChords_KnownLimitation()
    {
        // 알려진 한계(문서화): A~G 로 시작하는 단어는 코드 루트로 인식돼 조옮김된다(예: "Go"→루트 G→+2 "Ao").
        // 코드 칸('»' 뒤)은 운영자가 코드만 적는 영역이라 실사용엔 문제 없지만, 동작을 핀으로 고정해 둔다.
        // "Go"→루트 G(+2)="A"+"o", "Fade"→루트 F(+2)="G"+"ade".
        ChordTransposer.TransposeLine("Go Fade", 2).Should().Be("Ao Gade");
    }
}
