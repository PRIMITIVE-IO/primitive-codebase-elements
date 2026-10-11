

namespace PrimitiveCodebaseElements.Primitive.dto
{
    
    public class ArgumentDto
    {
        public readonly int Index;
        public readonly string Name;
        public readonly string Type;
        public readonly int ArgumentId;

        public ArgumentDto(int index, string name, string type, int argumentId)
        {
            Index = index;
            Name = name;
            Type = type;
            ArgumentId = argumentId;
            if (argumentId == -1)
            {
                ArgumentId = ElementIndexer.GetArgumentIndex();
            }
        }
    }
}