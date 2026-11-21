using System;
using System.Collections.Generic;
using System.Text;

namespace Axicultus.SQLite;

internal static class BibleSearchQuery
{
    internal static BoundQuery? Build(string text, bool partialWords, int bookNumber, int mode)
    {
        string passage = text.Trim().ToLowerInvariant();
        if (passage.Length == 0)
            return null;

        string[] terms = mode is 0 or 1
            ? passage.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            : [passage];
        var parameters = new List<(string Name, object Value)>();
        var conditions = new string[terms.Length];
        for (int i = 0; i < terms.Length; i++)
        {
            string name = "@term" + i;
            if (partialWords)
            {
                conditions[i] = "lower(bibletext) LIKE " + name;
                parameters.Add((name, "%" + terms[i] + "%"));
            }
            else
            {
                // Padding lets the same word boundary match the beginning and end of a verse.
                conditions[i] = "(' ' || lower(bibletext) || ' ') GLOB " + name;
                parameters.Add((name, WholeWordPattern(terms[i])));
            }
        }
        string bookFilter = "book>0";
        if (bookNumber >= 1)
        {
            bookFilter = "book=@book";
            parameters.Add(("@book", bookNumber));
        }
        string matches = string.Join(mode == 1 ? " OR " : " AND ", conditions);
        return new BoundQuery($"SELECT book, chapter, verse, bibletext FROM Bible WHERE {bookFilter} AND ({matches}) ORDER BY book, chapter, verse",
            [.. parameters]);
    }

    private static string WholeWordPattern(string term)
    {
        const string boundary = "[][\t\r\n -/:-@]";
        var pattern = new StringBuilder("*").Append(boundary);
        foreach (char c in term)
        {
            // Keep LIKE's user wildcards while treating GLOB's own metacharacters as text.
            switch (c)
            {
                case '%': pattern.Append('*'); break;
                case '_': pattern.Append('?'); break;
                case '*': pattern.Append("[*]"); break;
                case '?': pattern.Append("[?]"); break;
                case '[': pattern.Append("[[]"); break;
                default: pattern.Append(c); break;
            }
        }
        return pattern.Append(boundary).Append('*').ToString();
    }
}
