using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Easislides.Wpf.Library;

/// <summary>
/// 찬양집 색인을 인쇄 가능한 문서로 변환한다(FrmMain GenerateIndexReport / "Listing of Selected Folder" 대응).
/// 머리글자 그룹과 곡 제목·번호를 출력한다. 두 가지 형식을 지원한다:
///  - <see cref="BuildHtml"/>: 브라우저로 보기·인쇄하기 좋은 HTML.
///  - <see cref="BuildRtf"/>: Word 로 편집·인쇄하기 좋은 RTF(레거시가 내보내던 형식).
/// 순수 함수형(파일 I/O 없음) — 결과 문자열만 만들어 테스트하기 쉽다(호출부가 파일로 저장).
/// </summary>
public interface IPraiseBookIndexExporter
{
    string BuildHtml(string title, IReadOnlyList<PraiseBookIndexGroup> groups);

    /// <summary>현재 색인을 RTF(서식 있는 텍스트) 문서로 만든다 — 레거시 "Listing of Selected Folder" 의 RTF 출력 대응(Word 편집·인쇄용).</summary>
    string BuildRtf(string title, IReadOnlyList<PraiseBookIndexGroup> groups);
}

public sealed class PraiseBookIndexExporter : IPraiseBookIndexExporter
{
    public string BuildHtml(string title, IReadOnlyList<PraiseBookIndexGroup> groups)
    {
        ArgumentNullException.ThrowIfNull(groups);
        var safeTitle = Escape(string.IsNullOrWhiteSpace(title) ? "찬양집 색인" : title);

        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html lang=\"ko\"><head><meta charset=\"utf-8\">");
        sb.Append("<title>").Append(safeTitle).Append("</title>");
        sb.Append("<style>");
        sb.Append("body{font-family:'Malgun Gothic','맑은 고딕',sans-serif;margin:24px;color:#222;}");
        sb.Append("h1{font-size:20px;border-bottom:2px solid #444;padding-bottom:6px;}");
        sb.Append("h2{font-size:16px;margin:16px 0 4px;color:#1a4d8f;}");
        sb.Append("table{width:100%;border-collapse:collapse;}");
        sb.Append("td{padding:2px 6px;border-bottom:1px solid #eee;}");
        sb.Append("td.num{text-align:right;color:#888;width:64px;}");
        sb.Append("</style></head><body>");
        sb.Append("<h1>").Append(safeTitle).Append("</h1>");

        foreach (var group in groups)
        {
            sb.Append("<h2>").Append(Escape(group.Key)).Append("</h2><table>");
            foreach (var entry in group.Entries)
            {
                sb.Append("<tr><td>").Append(Escape(entry.Title)).Append("</td>");
                sb.Append("<td class=\"num\">")
                  .Append(entry.Number > 0 ? entry.Number.ToString() : string.Empty)
                  .Append("</td></tr>");
            }

            sb.Append("</table>");
        }

        sb.Append("</body></html>");
        return sb.ToString();
    }

    // HTML 특수문자(&<>"' )를 이스케이프해 제목에 들어간 기호가 마크업을 깨지 않게 한다.
    private static string Escape(string value) => WebUtility.HtmlEncode(value ?? string.Empty);

    public string BuildRtf(string title, IReadOnlyList<PraiseBookIndexGroup> groups)
    {
        ArgumentNullException.ThrowIfNull(groups);
        var safeTitle = EscapeRtf(string.IsNullOrWhiteSpace(title) ? "찬양집 색인" : title);

        var sb = new StringBuilder();
        // RTF 머리말 — 한글 글꼴(맑은 고딕, 한글 charset 129) + 유니코드 1글자 폴백(\uc1: \uN 뒤 ? 한 글자가 폴백).
        sb.Append(@"{\rtf1\ansi\ansicpg1252\deff0{\fonttbl{\f0\fnil\fcharset129 Malgun Gothic;}}");
        sb.Append(@"\viewkind4\uc1\pard\f0 ");
        // 제목 — 굵게 18pt(\fs 는 0.5pt 단위라 36=18pt). \sa 는 문단 뒤 간격.
        sb.Append(@"\sa180\b\fs36 ").Append(safeTitle).Append(@"\b0\fs24\par ");

        foreach (var group in groups)
        {
            // 머리글자 그룹 제목 — 굵게 14pt.
            sb.Append(@"\pard\sa60\b\fs28 ").Append(EscapeRtf(group.Key)).Append(@"\b0\fs22\par ");
            foreach (var entry in group.Entries)
            {
                // 곡 한 줄 — "제목 <탭> 번호". 번호 0 은 빈 칸으로(HTML 출력과 같은 규칙).
                sb.Append(@"\pard\tx7000\fs22 ").Append(EscapeRtf(entry.Title)).Append(@"\tab ");
                if (entry.Number > 0)
                {
                    sb.Append(entry.Number); // 번호는 ASCII 숫자 → 이스케이프 불필요.
                }

                sb.Append(@"\par ");
            }
        }

        sb.Append('}');
        return sb.ToString();
    }

    // RTF 특수문자(\ { })는 역슬래시로 이스케이프하고, ASCII 밖(한글 등) 글자는 \uN? 유니코드 escape 로 바꾼다.
    // N 은 부호 있는 16비트 코드값(예: '가'=U+AC00 → -21504), ? 는 RTF 리더가 유니코드를 모를 때 쓰는 폴백 글자.
    // (HTML 과 달리 RTF 에서는 & < > " ' 가 특수문자가 아니므로 그대로 둔다 — 오직 \ { } 만 escape 가 필요.)
    private static string EscapeRtf(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(value.Length + 8);
        foreach (var ch in value)
        {
            switch (ch)
            {
                case '\\': sb.Append(@"\\"); break;
                case '{': sb.Append(@"\{"); break;
                case '}': sb.Append(@"\}"); break;
                case '\r':
                case '\n': sb.Append(' '); break; // 한 줄 항목이라 줄바꿈은 공백으로.
                default:
                    if (ch < 0x20)
                    {
                        sb.Append(' '); // 기타 제어문자는 공백으로(문서가 깨지지 않게).
                    }
                    else if (ch < 0x80)
                    {
                        sb.Append(ch); // ASCII 는 그대로.
                    }
                    else
                    {
                        sb.Append(@"\u").Append((short)ch).Append('?'); // 한글 등 → \uN?(부호 있는 16비트).
                    }

                    break;
            }
        }

        return sb.ToString();
    }
}
