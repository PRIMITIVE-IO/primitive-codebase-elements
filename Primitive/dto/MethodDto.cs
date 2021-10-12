using System.Collections.Generic;
using System.Linq;

namespace PrimitiveCodebaseElements.Primitive.dto
{
    public class MethodDto
    {
        public readonly string Signature;
        public readonly string Name;
        public readonly AccessFlags AccFlag;
        public readonly List<ArgumentDto> Arguments;
        public readonly string ReturnType;
        public readonly string SourceCode;
        public readonly int StartIdx;
        public readonly int EndIdx;
        public readonly List<MethodReferenceDto> MethodReferences;

        public MethodDto(
            string signature,
            string name,
            AccessFlags accFlag,
            List<ArgumentDto> arguments,
            string returnType,
            string sourceCode,
            int startIdx,
            int endIdx,
            List<MethodReferenceDto> methodReferences = default)
        {
            Signature = signature;
            Name = name;
            AccFlag = accFlag;
            SourceCode = sourceCode;
            Arguments = arguments;
            ReturnType = returnType;
            EndIdx = endIdx;
            StartIdx = startIdx;
            MethodReferences = methodReferences ?? new List<MethodReferenceDto>();
        }
        
        public static string MethodSignature(string classFqn, string methodName, List<ArgumentDto> arguments)
        {
            string args = string.Join(",", arguments.Select(it => it.Type));
            return $"{classFqn}.{methodName}({args})";
        }

        public MethodDto With(List<MethodReferenceDto> methodReferences)
        {
            return new MethodDto(
                name: Name,
                accFlag: AccFlag,
                arguments: Arguments,
                returnType: ReturnType,
                sourceCode: SourceCode,
                startIdx: StartIdx,
                endIdx: EndIdx,
                methodReferences: methodReferences,
                signature: Signature
            );
        }
    }
}