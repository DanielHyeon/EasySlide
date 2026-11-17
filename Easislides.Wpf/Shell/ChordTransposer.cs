using System;
using System.Collections.Generic;
using System.Text;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 코드(화음) 조옮김 — 레거시 EasiSlides "Transpose Up/Down Semi-Tone" 대응의 순수 로직.
/// 루트음(A~G + 선택적 #/b)을 반음 단위로 옮기고, 접미사(m, 7, maj7, sus4 …)와 슬래시 베이스(/G)는 보존한다.
/// 코드로 인식되지 않는 토큰(가사·기호)은 그대로 둔다(견고성).
/// </summary>
public static class ChordTransposer
{
    // 반음 올림 표기 기준 12음(샤프 우선). 인덱스 = 피치 클래스(C=0).
    private static readonly string[] SharpScale =
        { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

    // 음이름(플랫 포함) → 피치 클래스.
    private static readonly Dictionary<string, int> NoteToPitch = new(StringComparer.Ordinal)
    {
        ["C"] = 0, ["C#"] = 1, ["Db"] = 1, ["D"] = 2, ["D#"] = 3, ["Eb"] = 3,
        ["E"] = 4, ["F"] = 5, ["F#"] = 6, ["Gb"] = 6, ["G"] = 7, ["G#"] = 8,
        ["Ab"] = 8, ["A"] = 9, ["A#"] = 10, ["Bb"] = 10, ["B"] = 11,
    };

    /// <summary>코드 하나를 반음 단위로 조옮김한다. 코드가 아니면(빈/공백/비코드) 그대로 반환.</summary>
    public static string Transpose(string? chord, int semitones)
    {
        if (string.IsNullOrWhiteSpace(chord))
        {
            return chord ?? string.Empty;
        }

        // 슬래시 베이스(C/G) — 양쪽을 각각 조옮김.
        var slash = chord.IndexOf('/');
        if (slash > 0)
        {
            var left = Transpose(chord[..slash], semitones);
            var right = Transpose(chord[(slash + 1)..], semitones);
            return $"{left}/{right}";
        }

        // 루트음 파싱: 첫 글자(A~G) + 선택적 # 또는 b.
        if (!TryParseRoot(chord, out var rootLength, out var pitch))
        {
            return chord; // 코드 루트로 못 읽으면 그대로(가사·N.C. 등).
        }

        var newPitch = ((pitch + semitones) % 12 + 12) % 12;
        var suffix = chord[rootLength..];
        return SharpScale[newPitch] + suffix;
    }

    /// <summary>코드 줄의 모든 코드 토큰을 조옮김하되 공백(정렬)은 보존한다.</summary>
    public static string TransposeLine(string? line, int semitones)
    {
        if (string.IsNullOrEmpty(line))
        {
            return line ?? string.Empty;
        }

        var result = new StringBuilder(line.Length);
        var i = 0;
        while (i < line.Length)
        {
            if (char.IsWhiteSpace(line[i]))
            {
                result.Append(line[i]);
                i++;
                continue;
            }

            // 공백이 아닌 토큰을 끊어 조옮김.
            var start = i;
            while (i < line.Length && !char.IsWhiteSpace(line[i]))
            {
                i++;
            }

            result.Append(Transpose(line[start..i], semitones));
        }

        return result.ToString();
    }

    // 코드 머리의 루트음을 읽는다. 성공 시 길이(rootLength)와 피치 클래스를 돌려준다.
    private static bool TryParseRoot(string chord, out int rootLength, out int pitch)
    {
        rootLength = 0;
        pitch = 0;

        var letter = char.ToUpperInvariant(chord[0]);
        if (letter is < 'A' or > 'G')
        {
            return false;
        }

        // 루트 표기: 글자 + (선택)# 또는 b.
        var root = letter.ToString();
        if (chord.Length > 1 && (chord[1] == '#' || chord[1] == 'b'))
        {
            root += chord[1];
        }

        if (!NoteToPitch.TryGetValue(root, out pitch))
        {
            return false;
        }

        rootLength = root.Length;
        return true;
    }
}
