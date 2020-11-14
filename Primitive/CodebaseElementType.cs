using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace PrimitiveCodebaseElements.Primitive
{
    [PublicAPI]
    public static class Extensions
    {
        public static CodebaseElementName CreateElementName(
            this CodebaseElementType elementType,
            string fullyQualifiedName)
        {
            switch (elementType)
            {
                case CodebaseElementType.Field:
                    return JsonConvert.DeserializeObject<FieldName>(fullyQualifiedName);
                case CodebaseElementType.Method:
                    return JsonConvert.DeserializeObject<MethodName>(fullyQualifiedName);
                case CodebaseElementType.Class:
                    return JsonConvert.DeserializeObject<ClassName>(fullyQualifiedName);
                case CodebaseElementType.File:
                    return new FileName(fullyQualifiedName);
                case CodebaseElementType.Package:
                    return new PackageName(fullyQualifiedName);
                case CodebaseElementType.Unknown:
                    return null;
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