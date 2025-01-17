using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive.dto
{
    [PublicAPI]
    public class MethodDto
    {
        public readonly string Signature;
        public readonly int MethodId;
        public readonly string Name;
        public readonly AccessFlags AccFlag;
        public readonly List<ArgumentDto> Arguments;
        public readonly string ReturnType;
        public readonly List<MethodReferenceDto> MethodReferences;
        // line/column coordinates in file
        public readonly CodeRange CodeRange;
        public readonly int? CyclomaticScore;

        public MethodDto(
            string signature,
            int methodId,
            string name,
            AccessFlags accFlag,
            List<ArgumentDto> arguments,
            string returnType,
            CodeRange codeRange,
            List<MethodReferenceDto> methodReferences,
            int? cyclomaticScore = default)
        {
            Signature = signature;
            MethodId = methodId;
            if (methodId == -1)
            {
                MethodId = ElementIndexer.GetMethodIndex();
            }

            Name = name;
            AccFlag = accFlag;
            Arguments = arguments;
            ReturnType = returnType;
            MethodReferences = methodReferences;
            CodeRange = codeRange;
            CyclomaticScore = cyclomaticScore;
        }
        
        public static string MethodSignature(AccessFlags accessFlags, string parentFqnOrPath, string methodName, List<ArgumentDto> arguments, string returnType)
        {
            string args = string.Join(",", arguments.Select(it => it.Type + " " + it.Name));
            return $"{accessFlags}|{parentFqnOrPath}.{methodName}({args}):{returnType}";
        }

        public MethodDto With(List<MethodReferenceDto> methodReferences)
        {
            return new MethodDto(
                name: Name,
                methodId: MethodId,
                accFlag: AccFlag,
                arguments: Arguments,
                returnType: ReturnType,
                methodReferences: methodReferences,
                signature: Signature,
                codeRange: CodeRange,
                cyclomaticScore: CyclomaticScore
            );
        }
    }
}