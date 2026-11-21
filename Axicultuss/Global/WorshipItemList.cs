using System;
using System.Collections.Generic;
using System.Text;

namespace Axicultus.Global;

public sealed class WorshipItem
{
    public string IdString = "";   // column 0
    public string Type = "";       // column 1
    public string Title = "";      // column 2
    public string Flag = "";       // column 3: always " "; copied to DocumentItem.IndexGroup (" " = no index number)
    public string FormatString = ""; // column 4
}

/// <summary>Items of the loaded worship list. Index is the item number, starting at 1, as before.</summary>
public sealed class WorshipItemList
{
    private readonly List<WorshipItem> items = [new WorshipItem()]; // [0] unused
    public int Count => items.Count - 1;
    public WorshipItem this[int itemNumber]
    {
        get
        {
            while (items.Count <= itemNumber) items.Add(new WorshipItem());
            return items[itemNumber];
        }
    }
    public void Clear() { items.Clear(); items.Add(new WorshipItem()); }
}
