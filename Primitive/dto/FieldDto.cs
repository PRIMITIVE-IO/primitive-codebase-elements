#nullable enable
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

        // line/column coordinates in file
        public readonly CodeRange CodeRange;
        

        public FieldDto(
            string name, 
            string type, 
            AccessFlags accFlag,
            CodeRange codeRange )
        {
            Name = name;
            Type = type;
            AccFlag = accFlag;
            CodeRange = codeRange;
        }
    }
}