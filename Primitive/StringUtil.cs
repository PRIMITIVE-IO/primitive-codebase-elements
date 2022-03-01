using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.dto;

namespace PrimitiveCodebaseElements.Primitive
{
    [PublicAPI]
    public static class StringUtil
    {
        public static string SubstringAfterLast(this string s, string sub)
        {
            int i = s.LastIndexOf(sub, StringComparison.Ordinal);

            if (i == -1) return s;

            int start = i + sub.Length;
            int len = s.Length - start;

            return s.Substring(start, len);
        }

        public static string TrimIndent(this string s)
        {
            return TrimIndent(s, out _);
        }

        public static string TrimIndent(this string s, out int unindentedCharsCount)
        {
            string[] lines = s.Split('\n');

            IEnumerable<int> firstNonWhitespaceIndices = lines
                .Skip(1)
                .Where(it => it.Trim().Length > 0)
                .Select(IndexOfFirstNonWhitespace);

            int firstNonWhitespaceIndex;

            if (firstNonWhitespaceIndices.Any()) firstNonWhitespaceIndex = firstNonWhitespaceIndices.Min();
            else firstNonWhitespaceIndex = -1;

            unindentedCharsCount = firstNonWhitespaceIndex;

            if (firstNonWhitespaceIndex == -1) return s;

            IEnumerable<string> unindentedLines = lines.Select(it => UnindentLine(it, firstNonWhitespaceIndex));
            return string.Join("\n", unindentedLines);
        }

        static string UnindentLine(string line, int firstNonWhitespaceIndex)
        {
            if (firstNonWhitespaceIndex < line.Length)
            {
                if (line[..firstNonWhitespaceIndex].Trim().Length != 0)
                {
                    //indentation contains some chars (if this is first line)
                    return line;
                }

                return line.Substring(firstNonWhitespaceIndex, line.Length - firstNonWhitespaceIndex);
            }

            return line.Trim().Length == 0 ? "" : line;
        }

        static int IndexOfFirstNonWhitespace(string s)
        {
            char[] chars = s.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] != ' ' && chars[i] != '\t') return i;
            }

            return -1;
        }

        public static int ClosedCurlyPosition(string source, int firstCurlyPosition)
        {
            int nestingCounter = 0;
            for (int i = firstCurlyPosition; i < source.Length; i++)
            {
                switch (source[i])
                {
                    case '{':
                        nestingCounter++;
                        break;
                    case '}':
                        nestingCounter--;
                        break;
                }

                if (nestingCounter == 0) return i;
            }

            throw new Exception($"Cannot find close curly brace starting from {firstCurlyPosition} for: {source}");
        }

        public static string Substring(this string s, Tuple<int, int> fromTo)
        {
            return s.Substring(fromTo.Item1, fromTo.Item2 - fromTo.Item1 + 1);
        }

        public static string SubstringBefore(this string s, string part)
        {
            return s[..s.IndexOf(part, StringComparison.Ordinal)];
        }

        public static string SubstringBeforeLast(this string s, string part)
        {
            int lastIdx = s.LastIndexOf(part, StringComparison.Ordinal);
            int len = lastIdx == -1 ? s.Length : lastIdx;
            return s[..len];
        }

        public static string SubstringAfter(this string s, string part)
        {
            int i = s.IndexOf(part, StringComparison.Ordinal);
            if (i == -1)
            {
                return s;
            }

            return s.Substring(Tuple.Create(i + part.Length, s.Length - 1));
        }

        [CanBeNull]
        public static CodeLocation LocationIn(this string s, PrimitiveCodebaseElements.Primitive.dto.CodeRange range,
            char c)
        {
            CodeLocation location = range.Of(s).LocationOf(c);
            if (location == null) return null;

            int column;
            if (location.Line == 1)
            {
                column = location.Column + range.Start.Column - 1;
            }
            else
            {
                column = location.Column;
            }

            return new CodeLocation(location.Line + range.Start.Line - 1, column);
        }

        public static int[] LineColIndex(this string s)
        {
            return s.Split('\n').Select(it => it.Length).ToArray();
        }

        public static CodeLocation OneCharLeft(this CodeLocation location, string s)
        {
            if (location.Line == 1 && location.Column == 1) throw new Exception($"Cannot shift left");
            if (location.Column > 1) return new CodeLocation(location.Line, location.Column - 1);

            return new CodeLocation(location.Line - 1, LineColIndex(s)[location.Line - 2]);
        }

        [CanBeNull]
        public static CodeLocation LocationOf(this string s, char c)
        {
            int idx = s.IndexOf(c);
            if (idx == -1) return null;

            int lineCounter = 1;
            int lastLineBreakIdx = -1;

            for (int i = 0; i < idx; i++)
            {
                char currentChar = s[i];
                if (currentChar == '\n')
                {
                    lineCounter++;
                }

                if (currentChar == '\r' || currentChar == '\n')
                {
                    lastLineBreakIdx = i;
                }
            }

            int col = idx - lastLineBreakIdx;

            return new CodeLocation(lineCounter, col);
        }
    }
}