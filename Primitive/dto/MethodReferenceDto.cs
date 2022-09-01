using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive.dto
{
    [PublicAPI]
    public class MethodReferenceDto
    {
        public readonly CodeReferenceType Type;
        public readonly string FromMethodSignature;

        public readonly string ToMethodSignature;
        //line/column coordinates in file
        public readonly CodeRange CodeRange;

        public MethodReferenceDto(
            CodeReferenceType type,
            string fromMethodSignature,
            string toMethodSignature,
            CodeRange codeRange)
        {
            Type = type;
            FromMethodSignature = fromMethodSignature;
            ToMethodSignature = toMethodSignature;
            CodeRange = codeRange;
        }

        public override string ToString()
        {
            return
                $"MethodReferenceDto(type={Type}, fromMethodSignature={FromMethodSignature}, toMethodSignature={ToMethodSignature}, codeRange={CodeRange})";
        }
    }
}