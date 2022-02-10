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
                .SelectMany(parsingResultDto => parsingResultDto.Classes)
                .SelectMany(classDto =>
                {
                    return classDto.Fields.Select(fieldDto => fieldDto.Type)
                        .Concat(classDto.Methods.Select(methodDto => methodDto.ReturnType))
                        .Concat(classDto.Methods.SelectMany(methodDto => methodDto.Arguments)
                            .Select(argumentDto => argumentDto.Type));
                }).Distinct();

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

                foreach (ClassDto classDto in fileDto.Classes)
                {
                    classFqnToId[classDto.FullyQualifiedName] = classId;

                    dbClasses.Add(new DbClass(
                        id: classId,
                        parentType: (int)CodebaseElementType.File,
                        parentId: fileId,
                        fqn: classDto.FullyQualifiedName,
                        accessFlags: (int)classDto.Modifier,
                        headerSource: classDto.Header,
                        language: (int)fileDto.Language,
                        isTestClass: fileDto.IsTest ? 1 : 0
                    ));

                    foreach (MethodDto methodDto in classDto.Methods)
                    {
                        methodSignatureToId[methodDto.Signature] = methodId;

                        dbMethods.Add(new DbMethod(
                            id: methodId,
                            parentType: (int)CodebaseElementType.Class,
                            parentId: classId,
                            name: methodDto.Name,
                            returnTypeId: typeToId[methodDto.ReturnType],
                            accessFlags: (int)methodDto.AccFlag,
                            sourceCode: methodDto.SourceCode,
                            language: (int)fileDto.Language,
                            cyclomaticScore: 0
                        ));
                        int argIndex = 0;
                        foreach (ArgumentDto argumentDto in methodDto.Arguments)
                        {
                            dbArguments.Add(new DbArgument(
                                id: argumentId,
                                methodId: methodId,
                                argIndex: argIndex,
                                name: argumentDto.Name,
                                typeId: typeToId[argumentDto.Type]
                            ));

                            argumentId++;
                            argIndex++;
                        }

                        if (methodDto.StartIdx != -1)
                        {
                            dbSourceIndices.Add(new DbSourceIndex(
                                elementId: methodId,
                                fileId: fileId,
                                type: "METHOD",
                                startIdx: methodDto.StartIdx,
                                endIdx: methodDto.EndIdx,
                                startLine: methodDto.CodeRange?.Start.Line,
                                startColumn: methodDto.CodeRange?.Start.Column,
                                endLine: methodDto.CodeRange?.End.Line,
                                endColumn: methodDto.CodeRange?.End.Column
                            ));
                        }

                        methodId++;
                    }

                    foreach (FieldDto fieldDto in classDto.Fields)
                    {
                        dbFields.Add(new DbField(
                            id: fieldId,
                            parentType: (int)CodebaseElementType.Class,
                            parentId: classId,
                            name: fieldDto.Name,
                            typeId: typeToId[fieldDto.Type],
                            accessFlags: (int)fieldDto.AccFlag,
                            sourceCode: fieldDto.SourceCode,
                            language: (int)fileDto.Language
                        ));
                        if (fieldDto.StartIdx != -1)
                        {
                            dbSourceIndices.Add(new DbSourceIndex(
                                elementId: fieldId,
                                fileId: fileId,
                                type: "FIELD",
                                startIdx: fieldDto.StartIdx,
                                endIdx: fieldDto.EndIdx,
                                startLine: fieldDto.CodeRange?.Start.Line,
                                startColumn: fieldDto.CodeRange?.Start.Column,
                                endLine: fieldDto.CodeRange?.End.Line,
                                endColumn: fieldDto.CodeRange?.End.Column
                            ));
                        }

                        fieldId++;
                    }

                    if (classDto.StartIdx != -1)
                    {
                        dbSourceIndices.Add(new DbSourceIndex(
                            elementId: classId,
                            fileId: fileId,
                            type: "CLASS",
                            startIdx: classDto.StartIdx,
                            endIdx: classDto.EndIdx,
                            startLine: classDto.CodeRange?.Start.Line,
                            startColumn: classDto.CodeRange?.Start.Column,
                            endLine: classDto.CodeRange?.End.Line,
                            endColumn: classDto.CodeRange?.End.Column
                        ));
                    }

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
                            startPosition: referencesFromThis.EndPosition,
                            endPosition: referencesFromThis.EndPosition,
                            startIdx: referencesFromThis.StartIdx,
                            endIdx: referencesFromThis.EndIdx,
                            startLine: referencesFromThis.CodeRange?.Start.Line,
                            startColumn: referencesFromThis.CodeRange?.Start.Column,
                            endLine: referencesFromThis.CodeRange?.End.Line,
                            endColumn: referencesFromThis.CodeRange?.End.Column
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
                                continue;
                            }

                            if (!methodSignatureToId.TryGetValue(methodReferenceDto.ToMethodSignature, out int toId))
                            {
                                PrimitiveLogger.Logger.Instance()
                                    .Warn(
                                        $"Cannot find referenced method signature: {methodReferenceDto.ToMethodSignature}");
                                continue;
                            }

                            methodReferences.Add(new DbMethodReference(
                                id: methodReferenceId,
                                type: (int)methodReferenceDto.Type,
                                fromId: fromId,
                                toId: toId,
                                startPosition: methodReferenceDto.StartPosition,
                                endPosition: methodReferenceDto.EndPosition,
                                startIdx: methodReferenceDto.StartIdx,
                                endIdx: methodReferenceDto.EndIdx,
                                startLine: methodReferenceDto.CodeRange?.Start.Line,
                                startColumn: methodReferenceDto.CodeRange?.Start.Column,
                                endLine: methodReferenceDto.CodeRange?.End.Line,
                                endColumn: methodReferenceDto.CodeRange?.End.Column
                            ));

                            methodReferenceId++;
                        }
                    }
                }
            }

            return new TableSet(
                directories: new List<DbDirectory>(),
                files: dbFiles,
                classes: dbClasses,
                methods: dbMethods,
                arguments: dbArguments,
                classReferences: classReferences,
                fields: dbFields,
                methodReferences: methodReferences,
                types: dbTypes,
                sourceIndices: dbSourceIndices
            );
        }
    }
}