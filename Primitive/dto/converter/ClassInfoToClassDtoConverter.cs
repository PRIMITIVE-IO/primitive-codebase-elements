using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive.dto.converter
{
    [PublicAPI]
    public class ClassInfoToClassDtoConverter
    {
        public static FileDto ToParsingResultDto(List<ClassInfo> classInfos, string sourceText, string path)
        {
            return new FileDto(
                path: path,
                text: sourceText,
                isTest: classInfos.Any(it => it.IsTestClass),
                classes: classInfos.SelectMany(ToClassDtos).ToList(),
                language: SourceCodeSnippet.LanguageFromExtension(Path.GetExtension(path))
            );
        }

        static List<ClassDto> ToClassDtos(ClassInfo classInfo)
        {
            return classInfo.InnerClasses
                .SelectMany(ToClassDtos).ToList()
                .Concat(new List<ClassDto>
                    {
                        new ClassDto(
                            path: classInfo.className.ContainmentFile().FilePath,
                            packageName: classInfo.className.ContainmentPackage.PackageNameString,
                            name: classInfo.className.originalClassName,
                            fullyQualifiedName: classInfo.className.ToJavaFullyQualified(),
                            methods: classInfo.Methods.Select(ToMethodDto).ToList(),
                            fields: classInfo.Fields.Select(ToFieldDto).ToList(),
                            modifier: classInfo.accessFlags,
                            startIdx: -1,
                            endIdx: -1,
                            header: classInfo.SourceCode.Text
                        )
                    }
                ).ToList();
        }

        static FieldDto ToFieldDto(FieldInfo fieldInfo)
        {
            return new FieldDto(
                name: fieldInfo.FieldName.ShortName,
                type: fieldInfo.FieldName.FieldType,
                accFlag: fieldInfo.AccessFlags,
                sourceCode: fieldInfo.SourceCode.Text,
                startIdx: -1,
                endIdx: -1
            );
        }

        static MethodDto ToMethodDto(MethodInfo methodInfo)
        {
            return new MethodDto(
                signature: "",
                name: methodInfo.MethodName.ShortName,
                sourceCode: methodInfo.SourceCode.Text,
                accFlag: methodInfo.accessFlags,
                startIdx: -1,
                endIdx: -1,
                arguments: ToArgumentDto(methodInfo.Arguments),
                returnType: methodInfo.ReturnType.Signature
            );
        }

        static List<ArgumentDto> ToArgumentDto(IEnumerable<Argument> arguments)
        {
            return arguments.Select((t, i) => new ArgumentDto(
                index: i, 
                name: t.Name,
                type: t.Type.Signature)).ToList();
        }
    }
}