using System.Collections.Generic;
using System.Xml.Linq;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 레거시 .esw(EasiSlides v3.2 예배 순서) 파일의 한 항목 원시 필드.
/// <para><see cref="TypeCode"/> 는 항목 종류 코드(레거시 ItemID 첫 글자 — D=DB 곡, P=PowerPoint, B=성경 등),
/// <see cref="Id"/> 는 나머지(곡 번호/파일명/성경 참조 등). 코드 의미 해석·내용 채우기는 가져오기 2단계가 맡는다.</para>
/// </summary>
public sealed record EswWorshipListItem(string TypeCode, string Id, string Title, string Folder, string FormatData);

/// <summary>
/// 레거시 .esw 예배 순서 파일(XML)을 읽어 항목 원시 데이터를 추출한다(가져오기 1단계 — 순수 파서).
/// <para>
/// .esw 는 바이너리가 아니라 XML 이다(레거시 FrmMain.LoadIndexFile 가 XmlTextReader 로 읽는다):
/// <c>&lt;EasiSlides&gt;&lt;ListItem&gt;&lt;ListHeader&gt;…&lt;/ListHeader&gt;
/// &lt;Item&gt;&lt;ItemID&gt;D123&lt;/ItemID&gt;&lt;Title1&gt;…&lt;/Title1&gt;&lt;Folder&gt;…&lt;/Folder&gt;&lt;FormatData&gt;…&lt;/FormatData&gt;&lt;/Item&gt;…</c>.
/// ItemID 첫 글자=종류 코드, 나머지=식별자(레거시 <c>Left(itemID,1)</c>/<c>Right(itemID,len-1)</c>).
/// </para>
/// <para>
/// 이 단계는 종류 코드를 WPF 항목으로 매핑하거나 가사·본문을 채우지 않는다 — 원시 필드만 돌려주고,
/// 매핑·내용 해석(곡 가사 DB 조회, 성경 본문 확장 등)은 가져오기 2단계가 맡는다. 손상된 XML 은 빈 목록(graceful).
/// </para>
/// </summary>
public static class EswWorshipListParser
{
    /// <summary>.esw XML 문자열에서 항목 원시 데이터를 순서대로 추출한다. ItemID 가 비면 그 항목은 건너뛴다.</summary>
    public static IReadOnlyList<EswWorshipListItem> Parse(string? xml)
    {
        var items = new List<EswWorshipListItem>();
        if (string.IsNullOrWhiteSpace(xml))
        {
            return items;
        }

        XDocument document;
        try
        {
            document = XDocument.Parse(xml);
        }
        catch (System.Xml.XmlException)
        {
            return items; // 손상·비-XML 파일은 빈 목록 — 호출 측이 "읽을 수 없음"을 안내한다.
        }

        // <Item> 만 대상으로 한다 — <ListHeader> 의 FormatData/Notes(설정·세션 메모)는 항목이 아니므로 자연히 제외된다.
        foreach (var element in document.Descendants("Item"))
        {
            var itemId = ((string?)element.Element("ItemID") ?? string.Empty).Trim();
            if (itemId.Length == 0)
            {
                continue; // 레거시도 ItemID 가 비면 항목을 쓰지 않는다(빈 항목 무시).
            }

            // 첫 글자=종류 코드, 나머지=식별자(레거시 Left/Right 규칙). 1글자뿐이면 식별자는 빈 문자열.
            var typeCode = itemId.Substring(0, 1);
            var id = itemId.Length > 1 ? itemId.Substring(1) : string.Empty;
            var title = (string?)element.Element("Title1") ?? string.Empty;
            var folder = (string?)element.Element("Folder") ?? string.Empty;
            var formatData = (string?)element.Element("FormatData") ?? string.Empty;

            items.Add(new EswWorshipListItem(typeCode, id, title, folder, formatData));
        }

        return items;
    }
}
