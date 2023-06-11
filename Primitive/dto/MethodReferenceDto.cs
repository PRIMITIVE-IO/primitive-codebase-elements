using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive.dto
{
    [PublicAPI]
    public class MethodReferenceDto
    {
        public readonly CodeReferenceType Type;
        
        // use these values when reconstructing from sqlite
        public readonly int? FromMethodId;
        public readonly int? ToMethodId;
        
        // use these values when solving references in a parser
        public readonly string? FromMethodSignature;
        public readonly string? ToMethodSignature;
        
        //line/column coordinates in file
        public readonly CodeRange CodeRange;

        /// <summary>
        /// Use this constructor when reconstructing references from a sqlite database analysis
        /// </summary>
        public MethodReferenceDto(
            CodeReferenceType type,
            int fromMethodId,
            int toMethodId,
            CodeRange codeRange)
        {
            Type = type;
            FromMethodId = fromMethodId;
            ToMethodId = toMethodId;
            CodeRange = codeRange;
        }
        
        /// <summary>
        /// Use this constructor when solving references in a parser
        /// </summary>
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