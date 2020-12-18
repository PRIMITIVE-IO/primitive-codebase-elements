using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive
{
    [PublicAPI]
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
    }

    [PublicAPI]
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
    }
}