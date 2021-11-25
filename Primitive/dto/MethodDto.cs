using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

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
        //char index in file
        [Obsolete("CodeRange should be used instead")]
        public readonly int StartIdx;
        //char index in file
        [Obsolete("CodeRange should be used instead")]
        public readonly int EndIdx;
        public readonly List<MethodReferenceDto> MethodReferences;
        // line/column coordinates in file
        // nullable for backward compatibility. Should be Non-null after removing all Idx
        [CanBeNull] public readonly CodeRange CodeRange;

        public MethodDto(
            string signature,
            string name,
            AccessFlags accFlag,
            List<ArgumentDto> arguments,
            string returnType,
            string sourceCode,
            int startIdx,
            int endIdx,
            CodeRange codeRange = default,
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
            CodeRange = codeRange;
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
                signature: Signature,
                codeRange: CodeRange
            );
        }
    }
}