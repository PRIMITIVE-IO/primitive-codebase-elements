

namespace PrimitiveCodebaseElements.Primitive.dto
{
    
    public class ClassReferenceDto
    {
        public readonly CodeReferenceType Type;
        
        public readonly int? FromId;
        public readonly int? ToId;
        
        public readonly string? FromFqn;
        public readonly string? ToFqn;
        
        // line/column coordinates in file
        public readonly CodeRange CodeRange;

        /// <summary>
        /// Use this constructor when reconstructing references from a sqlite database analysis
        /// </summary>
        public ClassReferenceDto(
            CodeReferenceType type,
            int fromId,
            int toId,
            CodeRange codeRange)
        {
            Type = type;
            FromId = fromId;
            ToId = toId;
            CodeRange = codeRange;
        }
        
        /// <summary>
        /// Use this constructor when solving references in a parser
        /// </summary>
        public ClassReferenceDto(
            CodeReferenceType type,
            string fromFqn,
            string toFqn,
            CodeRange codeRange)
        {
            Type = type;
            FromFqn = fromFqn;
            ToFqn = toFqn;
            CodeRange = codeRange;
        }
        
        public override string ToString()
        {
            return $"ClassReferenceDto(type={Type}, fromFqn={FromId}, toFqn={ToId}, codeRange={CodeRange})";
        }
    }
}