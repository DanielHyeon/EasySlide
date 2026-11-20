using System;

namespace Axicultus.Global;

/// <summary>
/// Options of the item search (Find dialog) used by Gf.BuildItemSearchQuery.
/// The defaults are those of the quick find box: search every text column, no filters.
/// </summary>
public sealed class ItemSearchOptions
{
    public bool InTitle = true;
    public bool InContents = true;
    public bool InSongNumber = true;
    public bool InBookRef = true;
    public bool InUserRef = true;
    public bool InLicAdmin = true;
    public bool InWriter = true;
    public bool InCopyright = true;

    public bool NotationsOnly;
    public string Key = "";
    public string Timing = "";
    public bool UseDates;
    public DateTime DateFrom;
    public DateTime DateTo;
}
