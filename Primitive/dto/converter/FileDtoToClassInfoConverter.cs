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

            Dictionary<string, ClassName> fqnToClassName = ToDictionarySkipDuplicates(fileDtos.SelectMany(fileDto =>
                fileDto.Classes.Select(classDto => Tuple.Create(classDto.FullyQualifiedName, new ClassName(
                        containmentFile: new FileName(filePath: fileDto.Path),
                        containmentPackage: new PackageName(packageNameString: classDto.PackageName),
                        className: classDto.Name
                    ))
                )
            ));

            Dictionary<string, MethodName> methodSignatureToMethodName = ToDictionarySkipDuplicates(fileDtos
                .SelectMany(fileDto => fileDto.Classes)
                .SelectMany(classDto =>
                    classDto.Methods.Select(methodDto => Tuple.Create(methodDto.Signature, new MethodName(
                        parent: fqnToClassName[classDto.FullyQualifiedName],
                        methodName: methodDto.Name,
                        returnType: methodDto.ReturnType,
                        argumentTypes: methodDto.Arguments.Select(argumentDto =>
                            new Argument(argumentDto.Name, TypeName.For(argumentDto.Type)))
                    )))
                )
            );

            List<Tuple<string, string>> methodReferences = fileDtos.SelectMany(fileDto => fileDto.Classes)
                .SelectMany(classDto => classDto.Methods)
                .SelectMany(methodDto => methodDto.MethodReferences)
                .Select(methodReference =>
                    Tuple.Create(methodReference.FromMethodSignature, methodReference.ToMethodSignature))
                .ToList();

            ILookup<string, string> outgoingMethodReferences = methodReferences
                .ToLookup(it => it.Item1, it => it.Item2);

            ILookup<string, string> incomingMethodReferences = methodReferences
                .ToLookup(it => it.Item2, it => it.Item1);

            Dictionary<string, ClassInfo> classInfosByFqn = ToDictionarySkipDuplicates(fileDtos
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
            );

            var classFqnToParentClassFqn = fileDtos
                .SelectMany(it => it.Classes)
                .ToDictionary(it => it.FullyQualifiedName, it => it.ParentClassFqn);

            foreach (KeyValuePair<string, ClassInfo> fqnToClassInfo in classInfosByFqn)
            {

                classFqnToParentClassFqn.TryGetValue(fqnToClassInfo.Key, out string explicitOuterClassFqn);
                
                string outerClassFqn = explicitOuterClassFqn ?? ExtractOuterClassFqnFromFqn(fqnToClassInfo.Key);
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

        private static Dictionary<string, T> ToDictionarySkipDuplicates<T>(
            IEnumerable<Tuple<string, T>> tuples)
        {
            Dictionary<string, List<T>> grouped = tuples
                .GroupBy(it => it.Item1, it => it.Item2)
                .ToDictionary(it => it.Key, it => it.ToList());

            List<KeyValuePair<string, List<T>>> duplicates = grouped.Where(it => it.Value.Count > 1).ToList();

            if (duplicates.Count > 0)
            {
                PrimitiveLogger.Logger.Instance().Error("Skip duplicates: " + string.Join(", ", duplicates));
            }

            return grouped.Where(it => it.Value.Count == 1)
                .ToDictionary(it => it.Key, it => it.Value.First());
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
            List<MethodDto> missingMethods =
                classDto.Methods.Where(x => !methodSignatureToMethodName.ContainsKey(x.Signature)).ToList();

            missingMethods.ForEach(method =>
                PrimitiveLogger.Logger.Instance()
                    .Error($"Missing method signature for {classDto.Path}: {method.Signature}"));

            ClassInfo classInfo = new ClassInfo(
                className: className,
                methods: classDto.Methods.Where(methodDto => !missingMethods.Contains(methodDto)).Select(methodDto =>
                    ConstructMethodInfo(
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

            IEnumerable<CodeReferenceEndpoint> classReferencesFromThis =
                outgoingClassReferencesByFqn[classDto.FullyQualifiedName]
                    .Select(it => new CodeReferenceEndpoint(it.Type, fqnToClassName[it.ToFqn]));

            classInfo.ReferencesFromThis.AddRange(classReferencesFromThis);

            IEnumerable<CodeReferenceEndpoint> classReferencesToThis =
                incomingClassReferencesByFqn[classDto.FullyQualifiedName]
                    .Select(it => new CodeReferenceEndpoint(it.Type, fqnToClassName[it.FromFqn]));

            classInfo.ReferencesToThis.AddRange(classReferencesToThis);

            return classInfo;
        }

        [CanBeNull]
        private static string ExtractOuterClassFqnFromFqn(string classDtoFullyQualifiedName)
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