using System;

namespace Axicultus;

internal sealed class LyricsEditHistory
{
    internal readonly record struct Snapshot(string Rtf, int Caret);
    private const int Capacity = 101; // Current state and up to 100 undo steps.
    private Snapshot[] entries;
    private int oldest;
    private int count;
    private int position = -1; // Offset from the oldest snapshot.

    private int Slot(int offset) => (oldest + offset) % Capacity;

    internal void Reset(Snapshot initial)
    {
        if (entries != null)
            Array.Clear(entries);
        oldest = count = 0;
        position = -1;
        Record(initial);
    }

    internal void Record(Snapshot snapshot)
    {
        entries ??= new Snapshot[Capacity];
        // Release RTF strings on the discarded redo branch as soon as a new edit is made.
        while (count > position + 1)
            entries[Slot(--count)] = default;
        if (count == Capacity)
        {
            oldest = Slot(1);
            count--;
        }
        position = count++;
        entries[Slot(position)] = snapshot;
    }

    internal bool TryMove(bool redo, out Snapshot snapshot)
    {
        int next = position + (redo ? 1 : -1);
        if (next < 0 || next >= count)
        {
            snapshot = default;
            return false;
        }
        position = next;
        snapshot = entries[Slot(position)];
        return true;
    }
}
