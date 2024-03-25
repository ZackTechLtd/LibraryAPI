using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Common.Util
{
    public static class ClassPropertyExtension
    {
        public static string GetNestedString(this string str, char start, char end)
        {
            int s = -1;
            int i = -1;
            while (++i < str.Length)
                if (str[i] == start)
                {
                    s = i;
                    break;
                }
            int e = -1;
            int depth = 0;
            while (++i < str.Length)
                if (str[i] == end)
                {
                    e = i;
                    if (depth == 0)
                        break;
                    else
                        --depth;
                }
                else if (str[i] == start)
                    ++depth;
            if (e > s)
                return str.Substring(s + 1, e - s - 1);
            return null;
        }

        public static List<int> SplitToIntList(this string list, char separator = ',')
        {
            return list.Split(separator).Select(Int32.Parse).ToList();
        }

        public static int[] SplitToIntArray(this string list, char separator = ',')
        {
            return list.Split(separator).Select(Int32.Parse).ToArray();
        }

        public static string HtmlToPlainText(this string html)
        {
            string buf;
            string block = "address|article|aside|blockquote|canvas|dd|div|dl|dt|" +
              "fieldset|figcaption|figure|footer|form|h\\d|header|hr|li|main|nav|" +
              "noscript|ol|output|p|pre|section|table|tfoot|ul|video";

            string patNestedBlock = $"(\\s*?</?({block})[^>]*?>)+\\s*";
            buf = Regex.Replace(html, patNestedBlock, "\n", RegexOptions.IgnoreCase);

            // Replace br tag to newline.
            buf = Regex.Replace(buf, @"<(br)[^>]*>", "\n", RegexOptions.IgnoreCase);

            // (Optional) remove styles and scripts.
            buf = Regex.Replace(buf, @"<(script|style)[^>]*?>.*?</\1>", "", RegexOptions.Singleline);

            // Remove all tags.
            buf = Regex.Replace(buf, @"<[^>]*(>|$)", "", RegexOptions.Multiline);

            // Replace HTML entities.
            buf = WebUtility.HtmlDecode(buf);
            return buf;
        }

        public static string AddSpacesToSentence(this string text, bool preserveAcronyms = false)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }
    }

}
