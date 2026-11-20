using System;

namespace Axicultus.Module;

public static class ChordNotation
{
    // The opening parenthesis of the row has already been consumed.
    public static int FindRecordEnd(ReadOnlySpan<char> record) => IndexOutsideParentheses(record, ')');

    public static int FindBassSeparator(ReadOnlySpan<char> chord) => IndexOutsideParentheses(chord, '/');

    private static int IndexOutsideParentheses(ReadOnlySpan<char> text, char delimiter)
    {
        int first = text.IndexOfAny('(', delimiter);
        if (first < 0 || text[first] == delimiter)
            return first;

        int depth = 0;
        for (int index = first; index < text.Length; index++)
        {
            char value = text[index];
            if (value == delimiter && depth == 0)
                return index;
            if (value == '(')
                depth++;
            else if (value == ')' && depth > 0)
                depth--;
            else if (value == ';')
                depth = 0; // An unfinished chord suffix cannot extend into the next field.
        }
        return -1;
    }
}
