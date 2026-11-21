using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Axicultus.Module;

/// <summary>Named on-disk fields shared by the two presenters. Their existing layout engines consume legacy headers.</summary>
public class ListFormatJsonConverter : JsonConverter<string>
{
    private static readonly FrozenDictionary<string, string> FieldNames = BuildFieldNames();
    private static readonly FrozenDictionary<string, string> FieldCodes =
        FieldNames.ToFrozenDictionary(pair => pair.Value, pair => pair.Key, StringComparer.Ordinal);

    private static FrozenDictionary<string, string> BuildFieldNames()
    {
        var names = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["1"] = "indexFileVersion",
            ["11"] = "panelBackgroundColorArgb", ["12"] = "panelBackgroundTransparent",
            ["13"] = "panelTextColorArgb", ["14"] = "panelUseRegion1TextColor",
            ["15"] = "panelDisplayFlags", ["16"] = "panelHeightPercent", ["17"] = "panelFontFamily",
            ["18"] = "panelFontStyleFlags", ["19"] = "panelIndicatorFontSize",
            ["21"] = "headingMode", ["22"] = "displayEffectFlags", ["23"] = "headingAlignment",
            ["25"] = "visibleLyricsRegions", ["26"] = "backgroundStartColorArgb",
            ["27"] = "backgroundEndColorArgb", ["28"] = "backgroundPattern",
            ["29"] = "region1TextColorArgb", ["30"] = "region2TextColorArgb",
            ["31"] = "region1Alignment", ["32"] = "region2Alignment",
            ["41"] = "region1FontStyleFlags", ["42"] = "region2FontStyleFlags",
            ["43"] = "region1FontFamily", ["44"] = "region2FontFamily",
            ["45"] = "region1TopPercent", ["46"] = "region2TopPercent",
            ["47"] = "region1FontSize", ["48"] = "region2FontSize",
            ["50"] = "mediaMode", ["51"] = "mediaPath", ["52"] = "mediaVolume", ["53"] = "mediaBalance",
            ["54"] = "mediaPlaybackFlags", ["55"] = "captureDeviceNumber", ["56"] = "mediaOutputMonitor",
            ["61"] = "backgroundImagePath", ["62"] = "backgroundImageMode", ["63"] = "verticalAlignment",
            ["64"] = "leftMarginPercent", ["65"] = "rightMarginPercent", ["66"] = "bottomMarginPercent",
            ["71"] = "transposeOffset", ["72"] = "itemTransition", ["73"] = "slideTransition",
            ["151"] = "praiseBookHeadingFlags", ["153"] = "praiseBookLyricsPattern",
            ["154"] = "praiseBookSection", ["155"] = "praiseBookColumns", ["156"] = "praiseBookPageSize",
            ["170"] = "praiseBookLineSpacing", ["171"] = "praiseBookSongSpacing",
            ["172"] = "praiseBookShowScreenBreaks", ["173"] = "praiseBookOneSongPerPage",
            ["174"] = "praiseBookCjkGroupStyle", ["180"] = "praiseBookNotationFlags"
        };
        string[] parts = ["SongNumber", "Title", "Copyright", "Verse", "ChorusBridge", "Notations", "BookReference", "UserReference"];
        for (int part = 0; part < parts.Length; part++)
        {
            int key = 101 + 5 * part;
            string prefix = "praiseBook" + parts[part];
            names[key.ToString(CultureInfo.InvariantCulture)] = prefix + "StyleFlags";
            names[(key + 1).ToString(CultureInfo.InvariantCulture)] = prefix + "FontSize";
            names[(key + 2).ToString(CultureInfo.InvariantCulture)] = prefix + "ColorArgb";
        }
        return names.ToFrozenDictionary(StringComparer.Ordinal);
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value.Length > 0) WriteFields(writer, value);
        writer.WriteEndObject();
    }

    protected virtual void WriteFields(Utf8JsonWriter writer, string header)
    {
        bool terminated = header.EndsWith('>');
        var entries = (terminated ? header[..^1] : header).Split('>');
        var seen = new HashSet<string>(StringComparer.Ordinal);
        Dictionary<int, string>? extra = null;
        for (int i = 0; i < entries.Length; i++)
        {
            string entry = entries[i];
            int equals = entry.IndexOf('=');
            string code = equals < 0 ? "" : entry[..equals];
            if (!FieldNames.TryGetValue(code, out var name) || !seen.Add(code))
            {
                // Old presenters differ on duplicate/unfinished fields. Preserve even their positions.
                (extra ??= []).Add(i, entry);
                continue;
            }
            string value = entry[(equals + 1)..];
            bool isText = code is "17" or "43" or "44" or "51" or "56" or "61" or "72" or "73";
            if (!isText && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number)
                && value == number.ToString(CultureInfo.InvariantCulture))
            {
                if (IsColor(name)) writer.WriteString(name, $"#{number:X8}");
                else writer.WriteNumber(name, number);
            }
            else if (IsColor(name) && TryReadColor(value, out _))
                (extra ??= []).Add(i, entry); // A hex string was not a valid decimal color in a legacy header.
            else
                writer.WriteString(name, value);
        }
        if (extra is not null)
        {
            writer.WriteStartObject("legacyEntries");
            foreach (var (index, entry) in extra) writer.WriteString(index.ToString(CultureInfo.InvariantCulture), entry);
            writer.WriteEndObject();
        }
        if (!terminated) writer.WriteBoolean("legacyUnterminated", true);
    }

    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String) return reader.GetString()!;
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("Expected a format object.");
        using var document = JsonDocument.ParseValue(ref reader);
        return ReadFields(document.RootElement);
    }

    protected virtual string ReadFields(JsonElement format)
    {
        var entries = new List<string>();
        SortedDictionary<int, string>? extra = null;
        bool terminated = true;
        foreach (var field in format.EnumerateObject())
        {
            if (field.NameEquals("legacyUnterminated") && field.Value.ValueKind is JsonValueKind.True or JsonValueKind.False)
                terminated = !field.Value.GetBoolean();
            else if (field.NameEquals("legacyEntries") && field.Value.ValueKind == JsonValueKind.Object)
            {
                foreach (var entry in field.Value.EnumerateObject())
                {
                    if (!int.TryParse(entry.Name, NumberStyles.None, CultureInfo.InvariantCulture, out int index)
                        || entry.Value.ValueKind != JsonValueKind.String || !(extra ??= []).TryAdd(index, entry.Value.GetString()!))
                        throw new JsonException("Invalid legacy format entry.");
                }
            }
            else if (FieldCodes.TryGetValue(field.Name, out var code) &&
                     field.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number)
            {
                string value = field.Value.ToString();
                if (value.Contains('>')) throw new JsonException("Unsupported format field delimiter.");
                if (IsColor(field.Name) && TryReadColor(value, out int argb)) value = argb.ToString(CultureInfo.InvariantCulture);
                entries.Add(code + "=" + value);
            }
            else throw new JsonException($"Unsupported format field '{field.Name}'.");
        }
        if (extra is not null)
        {
            foreach (var (index, entry) in extra)
            {
                if (index > entries.Count || entry.Contains('>')) throw new JsonException("Invalid legacy format entry.");
                entries.Insert(index, entry);
            }
        }
        return entries.Count == 0 ? "" : string.Join('>', entries) + (terminated ? ">" : "");
    }

    private static bool IsColor(string name) => name.EndsWith("ColorArgb", StringComparison.Ordinal);

    private static bool TryReadColor(string value, out int argb)
    {
        argb = 0;
        return value.Length == 9 && value[0] == '#' &&
            int.TryParse(value.AsSpan(1), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out argb);
    }
}

// The legacy Praise Book item slot holds a displayed song number, which may have leading zeros.
public sealed class ListEntryFormatJsonConverter : ListFormatJsonConverter
{
    protected override void WriteFields(Utf8JsonWriter writer, string header)
    {
        if (header.IndexOfAny(['=', '>']) < 0) writer.WriteString("displayNumber", header);
        else base.WriteFields(writer, header);
    }

    protected override string ReadFields(JsonElement format)
    {
        if (!format.TryGetProperty("displayNumber", out var number)) return base.ReadFields(format);
        if (number.ValueKind != JsonValueKind.String || format.GetPropertyCount() != 1) throw new JsonException("Invalid display number.");
        return number.GetString()!;
    }
}
