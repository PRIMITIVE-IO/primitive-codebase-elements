using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive
{
    [PublicAPI]
    public class CodeRange
    {
        public readonly int Start;
        public readonly int End;
        public readonly int StartLine;
        public readonly int EndLine;
        public readonly int StartColumn;
        public readonly int EndColumn;

        public CodeRange(
            int startPosition,
            int endPosition,
            int startLine,
            int startColumn,
            int endLine,
            int endColumn)
        {
            Start = startPosition;
            End = endPosition;
            StartLine = startLine;
            EndLine = endLine;
            StartColumn = startColumn;
            EndColumn = endColumn;
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