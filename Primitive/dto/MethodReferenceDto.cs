using System;
using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive.dto
{
    [PublicAPI]
    public class MethodReferenceDto
    {
        public readonly CodeReferenceType Type;
        public readonly string FromMethodSignature;

        public readonly string ToMethodSignature;

        //char offset in method de-indented source code snippet
        [Obsolete("CodeRange should be used instead")]
        public readonly int StartPosition;

        //char offset in method de-indented source code snippet
        [Obsolete("CodeRange should be used instead")]
        public readonly int EndPosition;

        //char offset in file
        [Obsolete("CodeRange should be used instead")]
        public readonly int StartIdx;

        //char offset in file
        [Obsolete("CodeRange should be used instead")]
        public readonly int EndIdx;

        //line/column coordinates in file. Should be non-null after complete migration
        [CanBeNull] public readonly CodeRange CodeRange;

        public MethodReferenceDto(
            CodeReferenceType type,
            string fromMethodSignature,
            string toMethodSignature,
            int startPosition,
            int endPosition,
            int startIdx,
            int endIdx,
            CodeRange codeRange = default)
        {
            Type = type;
            FromMethodSignature = fromMethodSignature;
            ToMethodSignature = toMethodSignature;
            StartPosition = startPosition;
            EndPosition = endPosition;
            StartIdx = startIdx;
            EndIdx = endIdx;
            CodeRange = codeRange;
        }

        public override string ToString()
        {
            return
                $"MethodReferenceDto(type={Type}, fromMethodSignature={FromMethodSignature}, toMethodSignature={ToMethodSignature}, codeRange={CodeRange})";
        }
    }
}