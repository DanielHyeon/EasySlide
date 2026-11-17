using System;
using System.Collections.Generic;
using System.Text;

namespace Easislides.Global
{
    public sealed class DocumentItem
    {
        public string IdString = "";      // column 0
        public string Type = "";          // column 1
        public string Title = "";         // column 2
        public string IndexGroup = "";    // column 3
        public string NumberAndInfo = ""; // column 4: "<song number>><info>"; passed by ref to ExtractOneInfo
    }

    public sealed class DocumentItemList
    {
        private readonly List<DocumentItem> items = new List<DocumentItem> { new DocumentItem() }; // [0] unused
        public int Count => items.Count - 1;
        public DocumentItem this[int itemNumber]
        {
            get
            {
                while (items.Count <= itemNumber) items.Add(new DocumentItem());
                return items[itemNumber];
            }
        }
        public void Clear() { items.Clear(); items.Add(new DocumentItem()); }
    }
}
