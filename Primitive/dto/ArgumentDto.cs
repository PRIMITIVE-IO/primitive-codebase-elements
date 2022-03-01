using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive.dto
{
    [PublicAPI]
    public class ArgumentDto
    {
        public readonly int Index;
        public readonly string Name;
        public readonly string Type;

        public ArgumentDto(int index, string name, string type)
        {
            Index = index;
            Name = name;
            Type = type;
        }
    }
}