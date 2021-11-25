using System;
using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive.dto
{
    public class ClassReferenceDto
    {
        public readonly CodeReferenceType Type;
        public readonly string FromFqn;

        public readonly string ToFqn;

        //char index in method source code snippet
        [Obsolete("CodeRange should be used instead")]
        public readonly int StartPosition;

        //char index in method source code snippet
        [Obsolete("CodeRange should be used instead")]
        public readonly int EndPosition;

        //char index in file
        [Obsolete("CodeRange should be used instead")]
        public readonly int StartIdx;

        //char index in file
        [Obsolete("CodeRange should be used instead")]
        public readonly int EndIdx;

        // line/column coordinates in file
        // nullable for backward compatibility. Should be Non-null after removing all Idx
        [CanBeNull] public readonly CodeRange CodeRange;

        public ClassReferenceDto(
            CodeReferenceType type,
            string fromFqn,
            string toFqn,
            int startPosition,
            int endPosition,
            int startIdx,
            int endIdx,
            [CanBeNull] CodeRange codeRange = default)
        {
            Type = type;
            FromFqn = fromFqn;
            ToFqn = toFqn;
            StartPosition = startPosition;
            EndPosition = endPosition;
            StartIdx = startIdx;
            EndIdx = endIdx;
            CodeRange = codeRange;
        }
    }
}