using System;
using System.Collections.Generic;
using System.Text;

namespace Easislides.Global
{
    public sealed class VerseLocation   // a class: entries are changed in place after they are added
    {
        public int Version, Book, Chapter, Verse, Start, Length;
    }

    /// <summary>Verses shown in the Bible text box, numbered from 1 as before.</summary>
    public sealed class BibleVerseLocationList
    {
        private readonly List<VerseLocation> items = new List<VerseLocation> { new VerseLocation() }; // [0] unused
        public int Count => items.Count - 1;
        // Grows on demand, as the array it replaced had room for every number: the code writes an entry by its
        // number and keeps the count in [0].Version.
        public VerseLocation this[int number]
        {
            get
            {
                while (items.Count <= number) items.Add(new VerseLocation());
                return items[number];
            }
        }
        public VerseLocation Add() { var v = new VerseLocation(); items.Add(v); return v; }
        public void Clear() { items.Clear(); items.Add(new VerseLocation()); }
    }
}
