using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Axicultus.Wpf.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Axicultus.Wpf.Library;

public sealed partial class OptionsFolderDraft(SongFolderSummary source) : ObservableObject
{
    private SongFolderSummary _original = source;
    private readonly Dictionary<string, string> _changes = new(StringComparer.OrdinalIgnoreCase);
    [ObservableProperty, NotifyPropertyChangedFor(nameof(GroupHeader))] private string _name = source.Name;
    [ObservableProperty] private bool _isEnabled = source.IsEnabled;
    public string GroupHeader => $"Settings for the {Name} Folder";
    public int Id => _original.FolderNo;

    public object this[string key]
    {
        get
        {
            var (column, format) = Split(key);
            var text = _changes.GetValueOrDefault(column) ?? _original.FormatFields?.GetValueOrDefault(column) ?? "";
            if (key == "HeadingOption") return int.TryParse(text, out int mode) ? mode : 0;
            if (format.Length == 0) return text;
            int.TryParse(text, out var flags);
            return format switch
            {
                "Bold" => (flags & 1) != 0,
                "Underline" => (flags & 4) != 0,
                "RTL" => (flags & 64) != 0,
                "Italic" => (flags & 2) != 0 ? 1 : (flags & 16) != 0 ? 2 : 0,
                _ => throw new ArgumentException(key)
            };
        }
        set
        {
            var (column, format) = Split(key);
            var field = FolderFormatField.All.First(f => f.Column == column);
            string current = _changes.GetValueOrDefault(column) ?? _original.FormatFields?.GetValueOrDefault(column) ?? "";
            string text;
            if (format.Length == 0)
                text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
            else
            {
                int.TryParse(current, out var flags);
                var (mask, bits) = format switch
                {
                    "Bold" => (1, value is true ? 1 : 0),
                    "Underline" => (4, value is true ? 4 : 0),
                    "RTL" => (64, value is true ? 64 : 0),
                    "Italic" => (18, Convert.ToInt32(value) switch { 1 => 18, 2 => 16, _ => 0 }),
                    _ => throw new ArgumentException(key)
                };
                text = ((flags & ~mask) | bits).ToString(CultureInfo.InvariantCulture);
            }
            if (!field.IsValid(text)) throw new ArgumentException(field.Label);
            if (text == current) return;
            if (text == _original.FormatFields?.GetValueOrDefault(column)) _changes.Remove(column);
            else _changes[column] = text;
            OnPropertyChanged(System.Windows.Data.Binding.IndexerName);
        }
    }

    private static (string Column, string Format) Split(string key)
    {
        int separator = key.IndexOf('_');
        return separator < 0 ? (key, "") : (key[..separator], key[(separator + 1)..]);
    }

    internal SongFolderWriteModel? GetChanges() =>
        _changes.Count == 0 && Name == _original.Name && IsEnabled == _original.IsEnabled ? null :
            new(Id, Name, IsEnabled, FormatChanges: new Dictionary<string, string>(_changes));

    internal void AcceptChanges()
    {
        if (GetChanges() is null) return;
        var fields = _original.FormatFields?.ToDictionary() ?? new Dictionary<string, string>();
        foreach (var pair in _changes) fields[pair.Key] = pair.Value;
        _original = _original with { Name = Name, IsEnabled = IsEnabled, FormatFields = fields };
        _changes.Clear();
    }
}
