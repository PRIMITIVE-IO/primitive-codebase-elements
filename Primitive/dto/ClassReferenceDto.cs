namespace PrimitiveCodebaseElements.Primitive.dto
{
    public class ClassReferenceDto
    {
        public readonly CodeReferenceType Type;
        public readonly string FromFqn;
        public readonly string ToFqn;
        public readonly int StartPosition;
        public readonly int EndPosition;
        public readonly int StartIdx;
        public readonly int EndIdx;

        public ClassReferenceDto(
            CodeReferenceType type,
            string fromFqn,
            string toFqn,
            int startPosition,
            int endPosition,
            int startIdx,
            int endIdx)
        {
            Type = type;
            FromFqn = fromFqn;
            ToFqn = toFqn;
            StartPosition = startPosition;
            EndPosition = endPosition;
            StartIdx = startIdx;
            EndIdx = endIdx;
        }
    }
}