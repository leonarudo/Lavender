//Author: mrpeardotnet
//Link: https://gist.github.com/mrpeardotnet/cba4338ffe01cb6e41d2765d8886aded

using System.Linq;
using System.Collections.Generic;
using System.Text;

#nullable disable
/// <summary>
/// An extension to split strings within quotes, etc
/// <br /><br />
/// NOTICE: This is <b>intentionally</b> set to internal to stop conflicts 
/// in case another mod ships the same extension. This is a popular piece
/// of code and any one type can only have the same extension once before 
/// the call becomes ambiguous and the runtime doesn't like that!
/// </summary>
internal static class SplitStringExtension
{
    /// <summary>
    /// Splits the string by specified separator, but only when the separator is outside the quotes.
    /// </summary>
    /// <param name="source">The source string to separate.</param>
    /// <param name="separator">The character used to split strings.</param>
    /// <param name="trimSplits">If set to <c>true</c>, split strings are trimmed (whitespaces are removed).</param>
    /// <param name="ignoreEmptyResults">If set to <c>true</c>, empty split results are ignored (not included in the result).</param>
    /// <param name="preserveEscapeCharInQuotes">If set to <c>true</c>, then the escape character (\) used to escape e.g. quotes is included in the results.</param>
    public static string[] SplitOutsideQuotes(this string source, char separator, bool trimSplits = true, bool ignoreEmptyResults = true, bool preserveEscapeCharInQuotes = true)
    {
        return source.SplitOutsideQuotes(new[] { separator }, trimSplits, ignoreEmptyResults, preserveEscapeCharInQuotes);
    }

    /// <summary>
    /// Splits the string by specified separator, but only when the separator is outside the quotes.
    /// </summary>
    /// <param name="source">The source string to separate.</param>
    /// <param name="separators">The characters used to split strings.</param>
    /// <param name="trimSplits">If set to <c>true</c>, split strings are trimmed (whitespaces are removed).</param>
    /// <param name="ignoreEmptyResults">If set to <c>true</c>, empty split results are ignored (not included in the result).</param>
    /// <param name="preserveEscapeCharInQuotes">If set to <c>true</c>, then the escape character (\) used to escape e.g. quotes is included in the results.</param>
    public static string[] SplitOutsideQuotes(this string source, char[] separators, bool trimSplits = true, bool ignoreEmptyResults = true, bool preserveEscapeCharInQuotes = true)
    {
        if (source == null) return null;

        var result = new List<string>();
        var escapeFlag = false;
        var quotesOpen = false;
        var currentItem = new StringBuilder();

        foreach (var currentChar in source)
        {
            if (escapeFlag)
            {
                currentItem.Append(currentChar);
                escapeFlag = false;
                continue;
            }

            if (separators.Contains(currentChar) && !quotesOpen)
            {
                var currentItemString = trimSplits ? currentItem.ToString().Trim() : currentItem.ToString();
                currentItem.Clear();
                if (string.IsNullOrEmpty(currentItemString) && ignoreEmptyResults) continue;
                result.Add(currentItemString);
                continue;
            }

            switch (currentChar)
            {
                default:
                    currentItem.Append(currentChar);
                    break;
                case '\\':
                    if (quotesOpen && preserveEscapeCharInQuotes) currentItem.Append(currentChar);
                    escapeFlag = true;
                    break;
                case '"':
                    currentItem.Append(currentChar);
                    quotesOpen = !quotesOpen;
                    break;
            }
        }

        if (escapeFlag) currentItem.Append("\\");

        var lastCurrentItemString = trimSplits ? currentItem.ToString().Trim() : currentItem.ToString();
        if (!(string.IsNullOrEmpty(lastCurrentItemString) && ignoreEmptyResults)) result.Add(lastCurrentItemString);

        return result.ToArray();
    }
}
#nullable restore
