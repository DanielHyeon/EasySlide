using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Easislides.Wpf.Data;

namespace Easislides.Wpf.Library;

public sealed class FolderFormatEdit : ObservableObject
{
    private readonly string _original;
    private readonly Action _changed;
    private string _value;
    public FolderFormatEdit(FolderFormatField field, string original, Action changed)
    {
        Field = field;
        _value = _original = original;
        _changed = changed;
        Flags = field.Flags?.Select((label, bit) => new Flag(this, label, 1 << bit)).ToArray() ?? [];
    }
    public FolderFormatField Field { get; }
    public IReadOnlyList<Flag> Flags { get; }
    public bool IsChanged => Value != _original;
    public string Value
    {
        get => _value;
        set { if (SetProperty(ref _value, value)) _changed(); }
    }

    public sealed class Flag(FolderFormatEdit edit, string label, int bit)
    {
        public string Label => label;
        private int Mask => int.TryParse(edit.Value, out var value) ? value : 0;
        public bool Enabled
        {
            get => (Mask & bit) != 0;
            set => edit.Value = (value ? Mask | bit : Mask & ~bit).ToString(CultureInfo.InvariantCulture);
        }
    }
}
