using System;

namespace PrimitiveCodebaseElements.Primitive
{
    public static class Extensions
    {
        public static CodebaseElementName CreateElementName(
            this CodebaseElementType elementType,
            string fullyQualifiedName)
        {
            switch (elementType)
            {
                case CodebaseElementType.Field:
                    return new FieldName(fullyQualifiedName);
                case CodebaseElementType.Method:
                    return new MethodName(fullyQualifiedName);
                case CodebaseElementType.Class:
                    return new ClassName(fullyQualifiedName);
                case CodebaseElementType.File:
                    return new FileName(fullyQualifiedName);
                case CodebaseElementType.Package:
                    return new PackageName(fullyQualifiedName);
                default:
                    throw new NotImplementedException(
                        "Cannot create CodebaseElementName " + $"with codebase element type of `{elementType}`.");
            }
        }
    }

    public enum CodebaseElementType
    {
        Unknown = -1,
        Field = 0,
        Method = 1,
        Class = 2,
        Package = 3,
        File = 4
    }
}
