using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Axicultus.Module;

namespace Axicultus.Wpf.Shell;

/// <summary>XML 항목의 형식화된 참조와 원본 표시 정보.</summary>
public sealed record EswWorshipListItem(LegacyListReference Reference, string Title, string Folder, string FormatData)
{
    public EswWorshipListItem(string TypeCode, string Id, string Title, string Folder, string FormatData)
        : this(LegacyListReference.FromFields(TypeCode, Id, Title), Title, Folder, FormatData) { }

    public string TypeCode => Reference.Code;
    public string Id => Reference.Id;
}

/// <summary>WinForms 목록 XML의 헤더와 항목을 문서 순서대로 읽는다.</summary>
public static class EswWorshipListParser
{
    /// <summary>.esw XML 문자열에서 항목 원시 데이터를 순서대로 추출한다. ItemID 가 비면 그 항목은 건너뛴다.</summary>
    public static IReadOnlyList<EswWorshipListItem> Parse(string? xml)
        => Parse(xml, out _, out _);

    internal static IReadOnlyList<EswWorshipListItem> Parse(string? xml, out string? sessionFormat)
        => Parse(xml, out sessionFormat, out _);

    internal static IReadOnlyList<EswWorshipListItem> Parse(string? xml, out string? sessionFormat, out string? sessionNotes)
        => ReadItems(xml, static (reference, title, folder, format) =>
            new EswWorshipListItem(reference, title, folder, format), out sessionFormat, out sessionNotes);

    internal static IReadOnlyList<T> ReadItems<T>(string? xml,
        System.Func<LegacyListReference, string, string, string, T?> project,
        out string? sessionFormat, out string? sessionNotes) where T : class
    {
        sessionFormat = null;
        sessionNotes = null;
        var items = new List<T>();
        if (string.IsNullOrWhiteSpace(xml)) return items;

        if (xml.AsSpan().TrimStart().StartsWith("{"))
        {
            ServiceListDocument document;
            try { document = ServiceListDocument.ReadJson(xml); }
            catch (System.Text.Json.JsonException) { return items; }
            catch (System.IO.InvalidDataException) { return items; }
            sessionFormat = document.FormatData;
            sessionNotes = document.Notes;
            foreach (var entry in document.Items)
            {
                if (string.IsNullOrWhiteSpace(entry.ItemId)) continue;
                var item = project(LegacyListReference.Parse(entry.ItemId, entry.Title), entry.Title, entry.Folder, entry.FormatData);
                if (item is not null) items.Add(item);
            }
            return items;
        }

        using var input = new System.IO.StringReader(xml);
        using var reader = System.Xml.XmlReader.Create(input);
        try
        {
            while (!reader.EOF)
            {
                if (reader.NodeType == System.Xml.XmlNodeType.Element && reader.Name == "Item")
                {
                    var element = (XElement)XNode.ReadFrom(reader);
                    var itemId = (string?)element.Element("ItemID");
                    if (string.IsNullOrWhiteSpace(itemId)) continue;
                    var title = (string?)element.Element("Title1") ?? "";
                    var reference = LegacyListReference.Parse(itemId, title);
                    var item = project(reference, title, (string?)element.Element("Folder") ?? "",
                        (string?)element.Element("FormatData") ?? "");
                    if (item is not null) items.Add(item);
                }
                else if (reader.NodeType == System.Xml.XmlNodeType.Element && reader.Name == "ListHeader")
                {
                    var header = (XElement)XNode.ReadFrom(reader);
                    sessionFormat = (string?)header.Element("FormatData");
                    sessionNotes = (string?)header.Element("Notes");
                }
                else reader.Read();
            }
        }
        catch (System.Xml.XmlException)
        {
            sessionFormat = sessionNotes = null;
            return [];
        }

        return items;
    }
}
