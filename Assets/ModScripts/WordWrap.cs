using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class WordWrapClass
{
    public static IEnumerable<string> WordWrap(this string text, int maxWidth, int hangingIndent = 0)
    {
        if (text == null || text.Length == 0)
            return Enumerable.Empty<string>();

        return wordWrap(
            text.Split(new string[] { "\r\n", "\r", "\n" }, 0),
            maxWidth,
            hangingIndent,
            (txt, substrIndex) => txt.Substring(substrIndex).Split(new string[] { " " }, options: StringSplitOptions.RemoveEmptyEntries),
            cc => cc.Length,
            txt =>
            {
                // Count the number of spaces at the start of the paragraph
                int indentLen = 0;
                while (indentLen < txt.Length && txt[indentLen] == ' ')
                    indentLen++;
                return indentLen;
            },
            num => new string(' ', num),
            () => new List<string>(),
            list => list.Sum(c => c.Length),
            (list, cc) => { list.Add(cc); },
            list => list.Join(""),
            (str, start, length) => length == null ? str.Substring(start) : str.Substring(start, length.Value),
            (str1, str2) => str1 + str2);
    }

    public static IEnumerable<string> Wrap(this string text, int maxWidth, int hangingIndent = 0)
    {
        if (text == null || text.Length == 0)
            return Enumerable.Empty<string>();

        return wordWrap(
            text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None),
            maxWidth,
            hangingIndent,
            (txt, substrIndex) => txt.Substring(substrIndex).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries),
            str => str.Length,
            txt =>
            {
                // Count the number of spaces at the start of the paragraph
                int indentLen = 0;
                while (indentLen < txt.Length && txt[indentLen] == ' ')
                    indentLen++;
                return indentLen;
            },
            num => new string(' ', num),
            () => new StringBuilder(),
            sb => sb.Length,
            (sb, str) => { sb.Append(str); },
            sb => sb.ToString(),
            (str, start, length) => length == null ? str.Substring(start) : str.Substring(start, length.Value),
            (str1, str2) => str1 + str2);
    }

    internal static IEnumerable<T> wordWrap<T, TBuilder>(IEnumerable<T> paragraphs, int maxWidth, int hangingIndent, Func<T, int, IEnumerable<T>> splitSubstringIntoWords,
        Func<T, int> getLength, Func<T, int> getIndent, Func<int, T> spaces, Func<TBuilder> getBuilder, Func<TBuilder, int> getTotalLength, Action<TBuilder, T> add,
        Func<TBuilder, T> getString, Func<T, int, int?, T> substring, Func<T, T, T> concat)
    {
        foreach (var paragraph in paragraphs)
        {
            var indentLen = getIndent(paragraph);
            var indent = spaces(indentLen + hangingIndent);
            var space = spaces(indentLen);
            var numSpaces = indentLen;
            var curLine = getBuilder();

            // Split into words
            foreach (var wordForeach in splitSubstringIntoWords(paragraph, indentLen))
            {
                var word = wordForeach;
                var curLineLength = getTotalLength(curLine);

                if (curLineLength + numSpaces + getLength(word) > maxWidth)
                {
                    // Need to wrap
                    if (getLength(word) > maxWidth)
                    {
                        // This is a very long word
                        // Leave part of the word on the current line if at least 2 chars fit
                        if (curLineLength + numSpaces + 2 <= maxWidth || getTotalLength(curLine) == 0)
                        {
                            int length = maxWidth - getTotalLength(curLine) - numSpaces;
                            add(curLine, space);
                            add(curLine, substring(word, 0, length));
                            word = substring(word, length, null);
                        }
                        // Commit the current line
                        yield return getString(curLine);

                        // Now append full lines' worth of text until we're left with less than a full line
                        while (indentLen + getLength(word) > maxWidth)
                        {
                            yield return concat(indent, substring(word, 0, maxWidth - indentLen));
                            word = substring(word, maxWidth - indentLen, null);
                        }

                        // Start a new line with whatever is left
                        curLine = getBuilder();
                        add(curLine, indent);
                        add(curLine, word);
                    }
                    else
                    {
                        // This word is not very long and it doesn't fit so just wrap it to the next line
                        yield return getString(curLine);

                        // Start a new line
                        curLine = getBuilder();
                        add(curLine, indent);
                        add(curLine, word);
                    }
                }
                else
                {
                    // No need to wrap yet
                    add(curLine, space);
                    add(curLine, word);
                }

                if (numSpaces != 1)
                {
                    space = spaces(1);
                    numSpaces = 1;
                }
            }

            yield return getString(curLine);
        }
    }
}
