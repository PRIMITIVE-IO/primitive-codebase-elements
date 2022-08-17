using System;
using System.Text;
using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive.dto
{
    [PublicAPI]
    public class CodeRange
    {
        public readonly CodeLocation Start;
        public readonly CodeLocation End;

        public CodeRange(CodeLocation start, CodeLocation end)
        {
            Start = start;
            End = end;
        }

        public string Of(string[] lines)
        {
            string startLine = lines[Start.Line - 1];

            if (Start.Line == End.Line)
            {
                return startLine[..Math.Min(startLine.Length, End.Column)][(Start.Column - 1)..];
            }

            StringBuilder sb = new StringBuilder();
            string firstLine = startLine[(Start.Column - 1)..];
            sb.AppendLine(firstLine);
            // `i` starts from the second line (because Line is 1-based)
            for (int i = Start.Line; i < End.Line - 1; i++)
            {
                sb.AppendLine(lines[i]);
            }

            string lastLine = lines[End.Line - 1];
            sb.Append(lastLine[..Math.Min(lastLine.Length, End.Column)]);
            return sb.ToString();
        }

        public string Of(string text)
        {
            int lineCounter = 1;
            int lastLineBreakIdx = -1;
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                char currentChar = text[i];

                bool isFirstLine = Start.Line == lineCounter;
                bool isOneliner = Start.Line == End.Line;
                bool fartherThanStartColumn = Start.Column <= i - lastLineBreakIdx;
                bool closerThanEndColumn = i - lastLineBreakIdx <= End.Column;
                bool isLastLine = End.Line == lineCounter;

                if (
                    isOneliner && isFirstLine && fartherThanStartColumn && closerThanEndColumn ||
                    !isOneliner && isFirstLine && fartherThanStartColumn ||
                    Start.Line < lineCounter && lineCounter < End.Line ||
                    !isOneliner && isLastLine && closerThanEndColumn
                )
                {
                    res.Append(currentChar);
                }

                if (End.Line < lineCounter ||
                    End.Line == lineCounter && End.Column < i - lastLineBreakIdx)
                {
                    break;
                }

                if (currentChar == '\n')
                {
                    lineCounter++;
                    lastLineBreakIdx = i;
                }
            }

            return res.ToString();
        }

        public bool IsValid()
        {
            return Start.CompareTo(End) <= 0;
        }

        protected bool Equals(CodeRange other)
        {
            return Equals(Start, other.Start) && Equals(End, other.End);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((CodeRange)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Start != null ? Start.GetHashCode() : 0) * 397) ^ (End != null ? End.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return $"{nameof(Start)}: {Start}, {nameof(End)}: {End}";
        }

        public static CodeRange Of(int startLine, int startColumn, int endLine, int endColumn)
        {
            return new CodeRange(new CodeLocation(startLine, startColumn), new CodeLocation(endLine, endColumn));
        }

        public bool NotContains(CodeLocation location)
        {
            return !Contains(location);
        }

        public bool Contains(CodeLocation location)
        {
            return location.CompareTo(Start) >= 0 && location.CompareTo(End) <= 0;
        }

        public bool Contains(CodeRange range)
        {
            return Start.CompareTo(range.Start) <= 0 && End.CompareTo(range.End) >= 0;
        }

        public bool NotContains(CodeRange range)
        {
            return !Contains(range);
        }
    }
}