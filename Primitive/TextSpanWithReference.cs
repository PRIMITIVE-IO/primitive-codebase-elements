using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using System;

namespace PrimitiveCodebaseElements.Primitive
{
    [PublicAPI]
    public class TextSpanWithReference
    {
        public int Start { get; }
        public int End { get; }
        public readonly CodebaseElementName Reference;

        public TextSpanWithReference(int start, int end, CodebaseElementName reference)
        {
            Start = start;
            End = end;
            Reference = reference;
        }

        public TextSpanWithReference AdjustedBy(int offset) =>
            new TextSpanWithReference(Start + offset, End + offset, Reference);
    }

    [PublicAPI]
    public class Insertion
    {
        readonly int Position;
        readonly string Text;

        public Insertion(int position, string text)
        {
            Position = position;
            Text = text;
        }

        public static string TextWithInsertions(
            string text,
            IEnumerable<Insertion> insertions)
        {
            if (string.IsNullOrEmpty(text)) return "";

            StringBuilder builder = new StringBuilder();

            int lastInsertionPosition = 0;

            foreach (Insertion insertion in insertions)
            {
                // All insertion positions are based on the original text positions, so we should always get substrings
                // of the original text.
                builder.Append(
                    text.AsSpan(
                        lastInsertionPosition,
                        insertion.Position - lastInsertionPosition));

                builder.Append(insertion.Text);

                lastInsertionPosition = insertion.Position;
            }

            builder.Append(text[lastInsertionPosition..]);

            return builder.ToString();
        }

        public override string ToString() =>
            $"(Position={Position}, Text={Text})";
    }
}