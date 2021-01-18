using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive
{
    [PublicAPI]
    public enum CodebaseElementType
    {
        Unknown = -1,
        Field = 0,
        Method = 1,
        Class = 2,
        Package = 3,
        File = 4,
        PrimitiveType = 5,
        ArrayType = 6
    }
}