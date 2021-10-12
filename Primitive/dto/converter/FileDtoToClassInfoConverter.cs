using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive.dto.converter
{
    public class FileDtoToClassInfoConverter
    {
        public static List<ClassInfo> ToClassInfos(List<FileDto> fileDtos)
        {
            List<ClassReferenceDto> classReferences = fileDtos.SelectMany(it => it.Classes)
                .SelectMany(it => it.ReferencesFromThis)
                .ToList();

            ILookup<string, ClassReferenceDto> outgoingClassReferencesByFqn = classReferences
                .ToLookup(it => it.FromFqn);

            ILookup<string, ClassReferenceDto> incomingClassReferencesByFqn = classReferences
                .ToLookup(it => it.ToFqn);

            Dictionary<string, ClassName> fqnToClassName = fileDtos.SelectMany(fileDto =>
                fileDto.Classes.Select(classDto => Tuple.Create(classDto.FullyQualifiedName, new ClassName(
                        containmentFile: new FileName(filePath: fileDto.Path),
                        containmentPackage: new PackageName(packageNameString: classDto.PackageName),
                        className: classDto.Name
                    ))
                )
            ).ToDictionary(it => it.Item1, it => it.Item2);

            Dictionary<string, MethodName> methodSignatureToMethodName = fileDtos.SelectMany(fileDto =>
                fileDto.Classes.SelectMany(classDto =>
                    classDto.Methods.Select(methodDto => Tuple.Create(methodDto.Signature, new MethodName(
                        parent: fqnToClassName[classDto.FullyQualifiedName],
                        methodName: methodDto.Name,
                        returnType: methodDto.ReturnType,
                        argumentTypes: methodDto.Arguments.Select(argumentDto =>
                            new Argument(argumentDto.Name, TypeName.For(argumentDto.Type)))
                    )))
                )
            ).ToDictionary(it => it.Item1, it => it.Item2);

            List<Tuple<string, string>> methodReferences = fileDtos.SelectMany(fileDto => fileDto.Classes)
                .SelectMany(classDto => classDto.Methods)
                .SelectMany(methodDto => methodDto.MethodReferences)
                .Select(methodReference => Tuple.Create(methodReference.FromMethodSignature, methodReference.ToMethodSignature))
                .ToList();

            ILookup<string, string> outgoingMethodReferences = methodReferences
                .ToLookup(it => it.Item1, it => it.Item2);

            ILookup<string, string> incomingMethodReferences = methodReferences
                .ToLookup(it => it.Item2, it => it.Item1);

            Dictionary<string, ClassInfo> classInfosByFqn = fileDtos
                .SelectMany(fileDto =>
                    fileDto.Classes.Select(classDto =>
                        Tuple.Create(classDto.FullyQualifiedName, ConstructClassInfo(fileDto,
                            classDto,
                            fqnToClassName,
                            methodSignatureToMethodName,
                            incomingMethodReferences,
                            outgoingMethodReferences,
                            outgoingClassReferencesByFqn,
                            incomingClassReferencesByFqn
                        ))
                    )
                )
                .ToDictionary(it => it.Item1, it => it.Item2);

            foreach (KeyValuePair<string, ClassInfo> fqnToClassInfo in classInfosByFqn)
            {
                string outerClassFqn = OuterClassFqn(fqnToClassInfo.Key);
                if (outerClassFqn != null)
                {
                    classInfosByFqn[outerClassFqn].Children.Add(fqnToClassInfo.Value);
                }
            }

            return classInfosByFqn
                //skip inner classes
                .Where(fqnToClassInfo => !fqnToClassInfo.Key.Contains("$"))
                .Select(it => it.Value)
                .ToList();
        }

        private static ClassInfo ConstructClassInfo(
            FileDto fileDto,
            ClassDto classDto,
            Dictionary<string, ClassName> fqnToClassName,
            Dictionary<string, MethodName> methodSignatureToMethodName,
            ILookup<string, string> incomingMethodReferences,
            ILookup<string, string> outgoingMethodReferences,
            ILookup<string, ClassReferenceDto> outgoingClassReferencesByFqn,
            ILookup<string, ClassReferenceDto> incomingClassReferencesByFqn)
        {
            ClassName className = fqnToClassName[classDto.FullyQualifiedName];

            ClassInfo classInfo = new ClassInfo(
                className: className,
                methods: classDto.Methods.Select(methodDto => ConstructMethodInfo(
                        methodSignatureToMethodName: methodSignatureToMethodName,
                        methodDto: methodDto,
                        className: className,
                        fileDto: fileDto,
                        incomingMethodReferences: incomingMethodReferences[methodDto.Signature].ToList(),
                        outgoingMethodReferences: outgoingMethodReferences[methodDto.Signature].ToList()
                    )
                ).ToList(),
                fields: classDto.Fields.Select(fieldDto => new FieldInfo(
                    fieldName: new FieldName(
                        containmentClass: className,
                        fieldName: fieldDto.Name,
                        fieldType: fieldDto.Type
                    ),
                    parentClass: className,
                    accessFlags: fieldDto.AccFlag,
                    sourceCode: new SourceCodeSnippet(fieldDto.SourceCode, fileDto.Language)
                )).ToList(),
                accessFlags: classDto.Modifier,
                innerClasses: new List<ClassInfo>(),
                headerSource: new SourceCodeSnippet(classDto.Header, fileDto.Language),
                isTestClass: fileDto.IsTest
            );

            IEnumerable<CodeReferenceEndpoint> classReferencesFromThis = outgoingClassReferencesByFqn[classDto.FullyQualifiedName]
                .Select(it => new CodeReferenceEndpoint(it.Type, fqnToClassName[it.ToFqn]));

            classInfo.ReferencesFromThis.AddRange(classReferencesFromThis);

            IEnumerable<CodeReferenceEndpoint> classReferencesToThis = incomingClassReferencesByFqn[classDto.FullyQualifiedName]
                .Select(it => new CodeReferenceEndpoint(it.Type, fqnToClassName[it.FromFqn]));

            classInfo.ReferencesToThis.AddRange(classReferencesToThis);

            return classInfo;
        }

        [CanBeNull]
        private static string OuterClassFqn(string classDtoFullyQualifiedName)
        {
            int lastIndexOfDollarSign = classDtoFullyQualifiedName.LastIndexOf('$');
            if (lastIndexOfDollarSign == -1) return null;
            return classDtoFullyQualifiedName.Substring(0, lastIndexOfDollarSign);
        }

        private static MethodInfo ConstructMethodInfo(
            Dictionary<string, MethodName> methodSignatureToMethodName,
            MethodDto methodDto,
            ClassName className,
            FileDto fileDto,
            List<string> incomingMethodReferences,
            List<string> outgoingMethodReferences)
        {
            MethodInfo methodInfo = new MethodInfo(
                methodName: methodSignatureToMethodName[methodDto.Signature],
                accessFlags: methodDto.AccFlag,
                parentClass: className,
                arguments: methodDto.Arguments.Select(argumentDto => new Argument(
                    name: argumentDto.Name,
                    type: TypeName.For(argumentDto.Type)
                )),
                returnType: TypeName.For(methodDto.ReturnType),
                sourceCode: new SourceCodeSnippet(methodDto.SourceCode, fileDto.Language)
            );

            methodInfo.ReferencesToThis.AddRange(incomingMethodReferences.Select(it =>
                new CodeReferenceEndpoint(CodeReferenceType.MethodCall, methodSignatureToMethodName[it])));
            methodInfo.ReferencesFromThis.AddRange(outgoingMethodReferences.Select(it =>
                new CodeReferenceEndpoint(CodeReferenceType.MethodCall, methodSignatureToMethodName[it])));

            return methodInfo;
        }
    }
}