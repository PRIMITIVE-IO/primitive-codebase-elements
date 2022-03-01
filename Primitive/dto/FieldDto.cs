using System;
using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive.dto
{
    [PublicAPI]
    public class FieldDto
    {
        public readonly string Name;
        public readonly string Type;
        public readonly AccessFlags AccFlag;
        public readonly string SourceCode;
        //char offset in file
        [Obsolete("CodeRange should be used instead")]
        public readonly int StartIdx;
        //char offset in file
        [Obsolete("CodeRange should be used instead")]
        public readonly int EndIdx;
        // line/column coordinates in file
        // nullable for backward compatibility. Should be Non-null after removing all Idx
        [CanBeNull] public readonly CodeRange CodeRange;
        

        public FieldDto(
            string name, 
            string type, 
            AccessFlags accFlag,
            string sourceCode, 
            int startIdx, 
            int endIdx,
            [CanBeNull] CodeRange codeRange = default)
        {
            Name = name;
            Type = type;
            AccFlag = accFlag;
            SourceCode = sourceCode;
            StartIdx = startIdx;
            EndIdx = endIdx;
            CodeRange = codeRange;
        }
    }
}