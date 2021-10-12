namespace PrimitiveCodebaseElements.Primitive.dto
{
    public class MethodReferenceDto
    {
        public readonly CodeReferenceType Type;
        public readonly string FromMethodSignature;
        public readonly string ToMethodSignature;
        public readonly int StartPosition;
        public readonly int EndPosition;
        public readonly int StartIdx;
        public readonly int EndIdx;

        public MethodReferenceDto(CodeReferenceType type, string fromMethodSignature, string toMethodSignature, int startPosition, int endPosition, int startIdx, int endIdx)
        {
            Type = type;
            FromMethodSignature = fromMethodSignature;
            ToMethodSignature = toMethodSignature;
            StartPosition = startPosition;
            EndPosition = endPosition;
            StartIdx = startIdx;
            EndIdx = endIdx;
        }
    }
    
}