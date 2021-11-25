using System;
using System.Collections.Generic;
using System.Linq;

namespace PrimitiveCodebaseElements.Primitive.dto
{
    public class CodeRange
    {
        public readonly CodeLocation Start;
        public readonly CodeLocation End;

        public CodeRange(CodeLocation start, CodeLocation end)
        {
            Start = start;
            End = end;
        }

        public string Of(string text)
        {
            int idxCounter = 0;
            int lastIdx = End.Line - Start.Line;
            IEnumerable<string> lines = text.Split('\n')
                .Skip(Start.Line - 1)
                .Take(End.Line - Start.Line + 1)
                .Select(line =>
                {
                    string acc = line;
                    if (idxCounter == 0) //clip the first line
                    {
                        acc = acc.Substring(Start.Column - 1);
                    }
                    else if (idxCounter == lastIdx) // clip the last line
                    {
                        acc = acc.Substring(0, End.Column);
                    }

                    idxCounter++;

                    return acc;
                });

            return string.Join("\n", lines);
        }

        protected bool Equals(CodeRange other)
        {
            return Equals(Start, other.Start) && Equals(End, other.End);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CodeRange)obj);
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
    }
}