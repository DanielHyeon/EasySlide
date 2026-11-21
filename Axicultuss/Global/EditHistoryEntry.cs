using System;
using System.Collections.Generic;
using System.Text;

namespace Axicultus.Global;

public sealed class EditHistoryEntry
{
    public string Id = "";    // column 0
    public string Title = ""; // column 1
}

/// <summary>A recent-edits list, numbered from 1; always holds Capacity entries, as the arrays did.</summary>
public sealed class EditHistoryList
{
    public const int Capacity = 20; // Gf.AbsoluteMaxHitoryItems
    private readonly EditHistoryEntry[] items = new EditHistoryEntry[Capacity + 1];
    public EditHistoryList() { for (int i = 0; i < items.Length; i++) items[i] = new EditHistoryEntry(); }
    public EditHistoryEntry this[int number] => items[number];
}
