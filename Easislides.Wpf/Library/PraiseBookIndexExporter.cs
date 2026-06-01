using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Easislides.Wpf.Library;

/// <summary>
/// 찬양집 색인을 인쇄 가능한 HTML 문서로 변환한다(FrmMain GenerateIndexReport 대응).
/// 머리글자 그룹과 곡 제목·번호를 표로 출력하며, 제목의 HTML 특수문자는 이스케이프한다.
/// 순수 함수형(파일 I/O 없음) — 결과 문자열만 만들어 테스트하기 쉽다(호출부가 파일로 저장).
/// </summary>
public interface IPraiseBookIndexExporter
{
    string BuildHtml(string title, IReadOnlyList<PraiseBookIndexGroup> groups);
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
}
