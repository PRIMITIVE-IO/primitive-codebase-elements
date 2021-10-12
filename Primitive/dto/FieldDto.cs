namespace PrimitiveCodebaseElements.Primitive.dto
{
    public class FieldDto
    {
        public readonly string Name;
        public readonly string Type;
        public readonly AccessFlags AccFlag;
        public readonly string SourceCode;
        public readonly int StartIdx;
        public readonly int EndIdx;

        public FieldDto(string name, string type, AccessFlags accFlag, string sourceCode, int startIdx, int endIdx)
        {
            Name = name;
            Type = type;
            AccFlag = accFlag;
            SourceCode = sourceCode;
            StartIdx = startIdx;
            EndIdx = endIdx;
        }
    }
}