#nullable enable
using System;
using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive.dto
{
    [PublicAPI]
    public class ClassReferenceDto
    {
        public readonly CodeReferenceType Type;
        public readonly string FromFqn;

        public readonly string ToFqn;
        // line/column coordinates in file
        public readonly CodeRange CodeRange;

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
            return $"ClassReferenceDto(type={Type}, fromFqn={FromFqn}, toFqn={ToFqn}, codeRange={CodeRange})";
        }
    }
}