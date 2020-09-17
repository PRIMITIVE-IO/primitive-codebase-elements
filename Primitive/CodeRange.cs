namespace PrimitiveCodebaseElements.Primitive
{
    public class CodeRange
    {
        public readonly int Start;
        public readonly int End;

        public CodeRange(
            int startPosition,
            int endPosition)
        {
            Start = startPosition;
            End = endPosition;
        }

        public override string ToString() => $"(Start={Start}, End={End})";
    }

    public class CodeRangeWithReference
    {
        public readonly int Start;
        public readonly int End;
        public readonly CodebaseElementName Reference;

        public CodeRangeWithReference(
            CodeRange codeRange,
            CodebaseElementName reference)
        {
            Start = codeRange.Start;
            End = codeRange.End;
            Reference = reference;
        }

        public override string ToString() =>
            $"{Reference.FullyQualified}: (Start={Start}, End={End})";
    }
}
