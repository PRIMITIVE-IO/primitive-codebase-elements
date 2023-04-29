using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive
{
    [PublicAPI]
    public enum CodeReferenceType
    {
        Undefined = -1,
        MethodCall = 0,
        ClassInheritance = 1,
        InterfaceImplementation = 2,
        MethodOverride = 3
    }
}