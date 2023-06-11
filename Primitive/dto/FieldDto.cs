using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive.dto
{
    [PublicAPI]
    public class FieldDto
    {
        public readonly string Name;
        public readonly string Type;
        public readonly AccessFlags AccFlag;
        public readonly int FieldId;

        // line/column coordinates in file
        public readonly CodeRange CodeRange;
        

        public FieldDto(
            string name, 
            string type, 
            AccessFlags accFlag,
            int fieldId,
            CodeRange codeRange)
        {
            Name = name;
            Type = type;
            AccFlag = accFlag;
            FieldId = fieldId;
            if (fieldId == -1)
            {
                FieldId = ElementIndexer.GetFieldIndex();
            }
            CodeRange = codeRange;
        }
    }
}