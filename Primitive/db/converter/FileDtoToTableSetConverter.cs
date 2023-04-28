using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.dto;

namespace PrimitiveCodebaseElements.Primitive.db.converter
{
    [PublicAPI]
    public static class FileDtoToTableSetConverter
    {
        public static TableSet ConvertToTableSet(List<FileDto> fileDtos, IReadOnlyDictionary<string, int> fileIdsByPath)
        {
            IEnumerable<string> types = fileDtos
                .SelectMany(fileDto => fileDto.Classes)
                .SelectMany(classDto =>
                {
                    return classDto.Fields.Select(fieldDto => fieldDto.Type)
                        .Concat(classDto.Methods.Select(methodDto => methodDto.ReturnType))
                        .Concat(classDto.Methods.SelectMany(methodDto => methodDto.Arguments)
                            .Select(argumentDto => argumentDto.Type));
                })
                .Concat(fileDtos.SelectMany(fileDto => fileDto.Fields).Select(fieldDto => fieldDto.Type))
                .Concat(fileDtos.SelectMany(fileDto => fileDto.Functions).SelectMany(methodDto =>
                {
                    return methodDto.Arguments.Select(argumentDto => argumentDto.Type)
                        .Concat(new[] { methodDto.ReturnType });
                }))
                .Distinct();

            Dictionary<string, int> typeToId = types
                .Select((type, id) => Tuple.Create(type, id))
                .ToDictionary(it => it.Item1, it => it.Item2);

            int classId = 1;
            int methodId = 1;
            int argumentId = 1;
            int fieldId = 1;
            List<DbFile> dbFiles = new List<DbFile>();
            List<DbClass> dbClasses = new List<DbClass>();
            List<DbMethod> dbMethods = new List<DbMethod>();
            List<DbArgument> dbArguments = new List<DbArgument>();
            List<DbField> dbFields = new List<DbField>();
            List<DbType> dbTypes = typeToId.Select(it => new DbType(it.Value, it.Key)).ToList();
            List<DbSourceIndex> dbSourceIndices = new List<DbSourceIndex>();

            Dictionary<string, int> classFqnToId = new Dictionary<string, int>();
            Dictionary<string, int> methodSignatureToId = new Dictionary<string, int>();
            Dictionary<string, int> truncatedMethodSignatureToId = new Dictionary<string, int>();

            foreach (FileDto fileDto in fileDtos)
            {
                int fileId = fileIdsByPath[fileDto.Path.Replace('\\', '/')];
                dbFiles.Add(new DbFile(
                    id: fileId,
                    directoryId: -1,
                    name: Path.GetFileName(fileDto.Path),
                    path: fileDto.Path,
                    sourceText: fileDto.Text,
                    language: (int)fileDto.Language
                ));

                ProcessMethods(
                    methodDtos: fileDto.Functions,
                    methodSignatureToId: methodSignatureToId,
                    methodIdCounter: ref methodId,
                    truncatedMethodSignatureToId: truncatedMethodSignatureToId,
                    dbMethodsAcc: dbMethods,
                    parentFileId: fileId,
                    parentClassId: null,
                    typeToId: typeToId,
                    fileDto: fileDto,
                    dbArgumentsAcc: dbArguments,
                    dbSourceIndicesAcc: dbSourceIndices,
                    fileId: fileId,
                    argumentIdCounter: ref argumentId
                );

                ProcessFields(
                    fields: fileDto.Fields,
                    dbFieldsAcc: dbFields,
                    fieldIdCounter: ref fieldId,
                    parentFileId: fileId,
                    parentClassId: null,
                    typeToId: typeToId,
                    fileDto: fileDto,
                    dbSourceIndicesAcc: dbSourceIndices,
                    fileId: fileId
                );

                foreach (ClassDto classDto in fileDto.Classes)
                {
                    classFqnToId[classDto.FullyQualifiedName] = classId;
                    int? parentClassId = null;
                    if (!string.IsNullOrEmpty(classDto.ParentClassFqn))
                    {
                        parentClassId = classFqnToId[classDto.ParentClassFqn];
                    }

                    dbClasses.Add(new DbClass(
                        id: classId,
                        parentClassId: parentClassId,
                        parentFileId: fileId,
                        fqn: classDto.FullyQualifiedName,
                        accessFlags: (int)classDto.Modifier,
                        language: (int)fileDto.Language,
                        isTestClass: fileDto.IsTest ? 1 : 0
                    ));

                    ProcessMethods(
                        methodDtos: classDto.Methods,
                        methodSignatureToId: methodSignatureToId,
                        methodIdCounter: ref methodId,
                        truncatedMethodSignatureToId: truncatedMethodSignatureToId,
                        dbMethodsAcc: dbMethods,
                        parentFileId: fileId,
                        parentClassId: classId,
                        typeToId: typeToId,
                        fileDto: fileDto,
                        dbArgumentsAcc: dbArguments,
                        dbSourceIndicesAcc: dbSourceIndices,
                        fileId: fileId,
                        argumentIdCounter: ref argumentId
                    );

                    ProcessFields(
                        fields: classDto.Fields,
                        dbFieldsAcc: dbFields,
                        fieldIdCounter: ref fieldId,
                        parentFileId: fileId,
                        parentClassId: classId,
                        typeToId: typeToId,
                        fileDto: fileDto,
                        dbSourceIndicesAcc: dbSourceIndices,
                        fileId: fileId
                    );

                    dbSourceIndices.Add(new DbSourceIndex(
                        elementId: classId,
                        fileId: fileId,
                        type: "CLASS",
                        startLine: classDto.CodeRange.Start.Line,
                        startColumn: classDto.CodeRange.Start.Column,
                        endLine: classDto.CodeRange.End.Line,
                        endColumn: classDto.CodeRange.End.Column
                    ));

                    classId++;
                }
            }

            List<DbClassReference> classReferences = new List<DbClassReference>();
            List<DbMethodReference> methodReferences = new List<DbMethodReference>();

            int classReferenceId = 1;
            int methodReferenceId = 1;
            foreach (FileDto fileDto in fileDtos)
            {
                foreach (ClassDto classDto in fileDto.Classes)
                {
                    foreach (ClassReferenceDto referencesFromThis in classDto.ReferencesFromThis)
                    {
                        classReferences.Add(new DbClassReference(
                            id: classReferenceId,
                            type: (int)referencesFromThis.Type,
                            fromId: classFqnToId[referencesFromThis.FromFqn],
                            toId: classFqnToId[referencesFromThis.ToFqn],
                            startLine: referencesFromThis.CodeRange.Start.Line,
                            startColumn: referencesFromThis.CodeRange.Start.Column,
                            endLine: referencesFromThis.CodeRange.End.Line,
                            endColumn: referencesFromThis.CodeRange.End.Column
                        ));

                        classReferenceId++;
                    }

                    foreach (MethodDto methodDto in classDto.Methods)
                    {
                        foreach (MethodReferenceDto methodReferenceDto in methodDto.MethodReferences)
                        {
                            if (!methodSignatureToId.TryGetValue(methodReferenceDto.FromMethodSignature,
                                    out int fromId))
                            {
                                PrimitiveLogger.Logger.Instance()
                                    .Warn($"Cannot find method signature: {methodReferenceDto.FromMethodSignature}");
                                if (methodReferenceDto.FromMethodSignature.Contains('('))
                                {
                                    // soft match
                                    string truncatedFromSig =
                                        methodReferenceDto.FromMethodSignature[
                                            ..methodReferenceDto.FromMethodSignature.IndexOf('(')];
                                    if (!truncatedMethodSignatureToId.TryGetValue(truncatedFromSig, out fromId))
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            if (!methodSignatureToId.TryGetValue(methodReferenceDto.ToMethodSignature, out int toId))
                            {
                                PrimitiveLogger.Logger.Instance()
                                    .Warn(
                                        $"Cannot find referenced method signature: {methodReferenceDto.ToMethodSignature}");

                                if (methodReferenceDto.ToMethodSignature.Contains('('))
                                {
                                    // soft match
                                    string truncatedFromSig =
                                        methodReferenceDto.ToMethodSignature[
                                            ..methodReferenceDto.ToMethodSignature.IndexOf('(')];
                                    if (!truncatedMethodSignatureToId.TryGetValue(truncatedFromSig, out toId))
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            methodReferences.Add(new DbMethodReference(
                                id: methodReferenceId,
                                type: (int)methodReferenceDto.Type,
                                fromId: fromId,
                                toId: toId,
                                startLine: methodReferenceDto.CodeRange.Start.Line,
                                startColumn: methodReferenceDto.CodeRange.Start.Column,
                                endLine: methodReferenceDto.CodeRange.End.Line,
                                endColumn: methodReferenceDto.CodeRange.End.Column
                            ));

                            methodReferenceId++;
                        }
                    }
                }
            }

            return new TableSet(
                directories: new List<DbDirectory>(),
                arguments: dbArguments,
                classes: dbClasses,
                classReferences: classReferences,
                fields: dbFields,
                files: dbFiles,
                methods: dbMethods,
                methodReferences: methodReferences,
                types: dbTypes,
                sourceIndices: dbSourceIndices
            );
        }

        static void ProcessFields(
            List<FieldDto> fields,
            List<DbField> dbFieldsAcc,
            ref int fieldIdCounter,
            int parentFileId,
            int? parentClassId,
            Dictionary<string, int> typeToId,
            FileDto fileDto,
            List<DbSourceIndex> dbSourceIndicesAcc,
            int fileId
        )
        {
            foreach (FieldDto fieldDto in fields)
            {
                dbFieldsAcc.Add(new DbField(
                    id: fieldIdCounter,
                    parentClassId: parentClassId,
                    parentFileId: parentFileId,
                    name: fieldDto.Name,
                    typeId: typeToId[fieldDto.Type],
                    accessFlags: (int)fieldDto.AccFlag,
                    language: (int)fileDto.Language
                ));
                dbSourceIndicesAcc.Add(new DbSourceIndex(
                    elementId: fieldIdCounter,
                    fileId: fileId,
                    type: "FIELD",
                    startLine: fieldDto.CodeRange.Start.Line,
                    startColumn: fieldDto.CodeRange.Start.Column,
                    endLine: fieldDto.CodeRange.End.Line,
                    endColumn: fieldDto.CodeRange.End.Column
                ));

                fieldIdCounter++;
            }
        }

        static void ProcessMethods(
            List<MethodDto> methodDtos,
            Dictionary<string, int> methodSignatureToId,
            ref int methodIdCounter,
            Dictionary<string, int> truncatedMethodSignatureToId,
            List<DbMethod> dbMethodsAcc,
            int parentFileId,
            int? parentClassId,
            Dictionary<string, int> typeToId,
            FileDto fileDto,
            List<DbArgument> dbArgumentsAcc,
            List<DbSourceIndex> dbSourceIndicesAcc,
            int fileId,
            ref int argumentIdCounter
        )
        {
            foreach (MethodDto methodDto in methodDtos)
            {
                if (!methodSignatureToId.TryAdd(methodDto.Signature, methodIdCounter))
                {
                    PrimitiveLogger.Logger.Instance().Warn($"Duplicated method {methodDto.Signature}");
                }

                if (methodDto.Signature.Contains('('))
                {
                    string truncatedMethodSignature = methodDto.Signature[..methodDto.Signature.IndexOf('(')];
                    if (!truncatedMethodSignatureToId.ContainsKey(truncatedMethodSignature))
                    {
                        truncatedMethodSignatureToId.Add(truncatedMethodSignature, methodIdCounter);
                    }
                }

                dbMethodsAcc.Add(new DbMethod(
                    id: methodIdCounter,
                    parentClassId: parentClassId,
                    parentFileId: parentFileId,
                    name: methodDto.Name,
                    returnTypeId: typeToId[methodDto.ReturnType],
                    accessFlags: (int)methodDto.AccFlag,
                    language: (int)fileDto.Language,
                    cyclomaticScore: methodDto.CyclomaticScore
                ));
                int argIndex = 0;
                foreach (ArgumentDto argumentDto in methodDto.Arguments)
                {
                    dbArgumentsAcc.Add(new DbArgument(
                        id: argumentIdCounter,
                        methodId: methodIdCounter,
                        argIndex: argIndex,
                        name: argumentDto.Name,
                        typeId: typeToId[argumentDto.Type]
                    ));

                    argumentIdCounter++;
                    argIndex++;
                }

                dbSourceIndicesAcc.Add(new DbSourceIndex(
                    elementId: methodIdCounter,
                    fileId: fileId,
                    type: "METHOD",
                    startLine: methodDto.CodeRange.Start.Line,
                    startColumn: methodDto.CodeRange.Start.Column,
                    endLine: methodDto.CodeRange.End.Line,
                    endColumn: methodDto.CodeRange.End.Column
                ));

                methodIdCounter++;
            }
        }
    }
}