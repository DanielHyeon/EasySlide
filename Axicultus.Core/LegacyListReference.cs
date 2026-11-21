using System;
using System.Globalization;
using System.IO;

namespace Axicultus.Module;

public enum LegacyListItemKind { Unknown, DatabaseSong, BiblePassage, PowerPoint, Media, TextFile, InfoScreen, WordDocument }

/// <summary>Meaning of an ESP/ESW ItemID and Title1 pair. Code and Id retain the original reference.</summary>
public sealed record LegacyListReference
{
    private LegacyListReference(string code, string id, string title)
    {
        Code = code;
        Id = id;
        Kind = code.Length == 1 ? char.ToUpperInvariant(code[0]) switch
        {
            'D' => LegacyListItemKind.DatabaseSong,
            'B' => LegacyListItemKind.BiblePassage,
            'P' => LegacyListItemKind.PowerPoint,
            'M' => LegacyListItemKind.Media,
            'T' => LegacyListItemKind.TextFile,
            'I' => LegacyListItemKind.InfoScreen,
            'W' => LegacyListItemKind.WordDocument,
            _ => LegacyListItemKind.Unknown
        } : LegacyListItemKind.Unknown;

        if (Kind == LegacyListItemKind.DatabaseSong && int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var songId) && songId > 0)
            SongId = songId;
        if (Kind is LegacyListItemKind.PowerPoint or LegacyListItemKind.Media or LegacyListItemKind.TextFile
            or LegacyListItemKind.InfoScreen or LegacyListItemKind.WordDocument)
        {
            // The WinForms writer puts "1" in ItemID and the path in Title1. Other writers embed the path in ItemID.
            var caption = title.Trim();
            ContentPath = id == "1" || (!LooksLikePath(id) && LooksLikePath(caption)) ? caption : id.Trim();
        }
    }

    public LegacyListItemKind Kind { get; }
    public string Code { get; }
    public string Id { get; }
    public int? SongId { get; }
    public string? ContentPath { get; }
    public string? BibleReference => Kind == LegacyListItemKind.BiblePassage ? Id : null;

    public static LegacyListReference Parse(string itemId, string title)
    {
        var value = itemId.AsSpan().Trim();
        return value.IsEmpty ? FromFields("", "", title) : FromFields(value[0].ToString(), value[1..].ToString(), title);
    }

    public static LegacyListReference FromFields(string code, string id, string title) => new(code, id, title);

    private static bool LooksLikePath(string text) => Path.IsPathRooted(text) || Path.HasExtension(text);
}
