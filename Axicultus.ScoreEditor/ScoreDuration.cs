// SPDX-License-Identifier: GPL-3.0-only
// C# adaptation of the written-duration calculation in MuseScore Studio 4.6.5.
// Upstream copyright (C) 2021 MuseScore Limited.
// https://github.com/musescore/MuseScore/blob/653f38d8ea80ebd2b084c2b5e5521a32b00983c7/src/engraving/dom/durationtype.cpp
namespace Axicultus.ScoreEditor;

/// <summary>Written durations in whole-note units. Follows MuseScore Studio 4.6.5 TDuration::fraction.</summary>
public readonly record struct ScoreDuration(string Type, int Dots = 0)
{
    public static System.Collections.ObjectModel.ReadOnlyCollection<string> Types { get; } =
        Array.AsReadOnly(new[] { "long", "breve", "whole", "half", "quarter", "eighth", "16th", "32nd", "64th", "128th", "256th", "512th", "1024th" });

    public (int Numerator, int Denominator) Fraction()
    {
        int index = Types.IndexOf(Type);
        if (index < 0 || Dots is < 0 or > 4) throw new InvalidDataException($"Unsupported duration: {Type}, {Dots}");
        int baseNumerator = 1 << Math.Max(0, 2 - index);
        int baseDenominator = 1 << Math.Max(0, index - 2);
        int numerator = baseNumerator * ((2 << Dots) - 1);
        int denominator = baseDenominator << Dots;
        while ((numerator & 1) == 0 && (denominator & 1) == 0) { numerator /= 2; denominator /= 2; }
        return (numerator, denominator);
    }

    internal int Units
    {
        get { var (n, d) = Fraction(); return n * 16384 / d; }
    }
}
