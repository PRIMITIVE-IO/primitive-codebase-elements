using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using PrimitiveCodebaseElements.Primitive.dto;

namespace PrimitiveCodebaseElements.Primitive.db.converter
{
    
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
            
            List<DbFile> dbFiles = new List<DbFile>();
            List<DbClass> dbClasses = new List<DbClass>();
            List<DbMethod> dbMethods = new List<DbMethod>();
            List<DbArgument> dbArguments = new List<DbArgument>();
            List<DbField> dbFields = new List<DbField>();
            List<DbType> dbTypes = typeToId.Select(it => new DbType(it.Value, it.Key)).ToList();
            List<DbSourceIndex> dbSourceIndices = new List<DbSourceIndex>();

            // because the solvers do not have an absolute id for the classes, the fqns and signatures are still required for matching
            Dictionary<string, int> classFqnToId = new();
            Dictionary<string, int> methodSignatureToId = new();

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
                    dbMethodsAcc: dbMethods,
                    parentFileId: fileId,
                    parentClassId: null,
                    typeToId: typeToId,
                    fileDto: fileDto,
                    dbArgumentsAcc: dbArguments,
                    dbSourceIndicesAcc: dbSourceIndices,
                    fileId: fileId,
                    methodSignatureToId: methodSignatureToId
                );

                ProcessFields(
                    fields: fileDto.Fields,
                    dbFieldsAcc: dbFields,
                    parentFileId: fileId,
                    parentClassId: null,
                    typeToId: typeToId,
                    fileDto: fileDto,
                    dbSourceIndicesAcc: dbSourceIndices,
                    fileId: fileId
                );

                foreach (ClassDto classDto in fileDto.Classes)
                {
                    dbClasses.Add(new DbClass(
                        id: classDto.ClassId,
                        parentClassId: classDto.ParentClassId,
                        parentFileId: fileId,
                        fqn: classDto.FullyQualifiedName,
                        accessFlags: (int)classDto.Modifier,
                        isTestClass: fileDto.IsTest ? 1 : 0
                    ));

                    ProcessMethods(
                        methodDtos: classDto.Methods,
                        dbMethodsAcc: dbMethods,
                        parentFileId: fileId,
                        parentClassId: classDto.ClassId,
                        typeToId: typeToId,
                        fileDto: fileDto,
                        dbArgumentsAcc: dbArguments,
                        dbSourceIndicesAcc: dbSourceIndices,
                        fileId: fileId,
                        methodSignatureToId: methodSignatureToId
                    );

                    ProcessFields(
                        fields: classDto.Fields,
                        dbFieldsAcc: dbFields,
                        parentFileId: fileId,
                        parentClassId: classDto.ClassId,
                        typeToId: typeToId,
                        fileDto: fileDto,
                        dbSourceIndicesAcc: dbSourceIndices,
                        fileId: fileId
                    );
                    
                    classFqnToId.TryAdd(classDto.FullyQualifiedName, classDto.ClassId);

                    dbSourceIndices.Add(new DbSourceIndex(
                        elementId: classDto.ClassId,
                        fileId: fileId,
                        type: SourceCodeType.Class,
                        startLine: classDto.CodeRange.Start.Line,
                        startColumn: classDto.CodeRange.Start.Column,
                        endLine: classDto.CodeRange.End.Line,
                        endColumn: classDto.CodeRange.End.Column
                    ));
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
                    foreach (ClassReferenceDto referenceFromThis in classDto.ReferencesFromThis)
                    {
                        if (!classFqnToId.ContainsKey(referenceFromThis.FromFqn) ||
                            !classFqnToId.ContainsKey(referenceFromThis.ToFqn))
                            continue;
                        
                        classReferences.Add(new DbClassReference(
                            id: classReferenceId,
                            type: (int)referenceFromThis.Type,
                            fromId: referenceFromThis.FromId ?? classFqnToId[referenceFromThis.FromFqn],
                            toId: referenceFromThis.ToId ?? classFqnToId[referenceFromThis.ToFqn],
                            startLine: referenceFromThis.CodeRange.Start.Line,
                            startColumn: referenceFromThis.CodeRange.Start.Column,
                            endLine: referenceFromThis.CodeRange.End.Line,
                            endColumn: referenceFromThis.CodeRange.End.Column
                        ));

                        classReferenceId++;
                    }

                    foreach (MethodDto methodDto in classDto.Methods)
                    {
                        foreach (MethodReferenceDto methodReferenceDto in methodDto.MethodReferences)
                        {
                            if (!methodSignatureToId.ContainsKey(methodReferenceDto.FromMethodSignature) ||
                                !methodSignatureToId.ContainsKey(methodReferenceDto.ToMethodSignature)) continue;

                            methodReferences.Add(new DbMethodReference(
                                id: methodReferenceId,
                                type: (int)methodReferenceDto.Type,
                                fromId: methodReferenceDto.FromMethodId ?? methodSignatureToId[methodReferenceDto.FromMethodSignature],
                                toId: methodReferenceDto.ToMethodId ??  methodSignatureToId[methodReferenceDto.ToMethodSignature],
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
                    id: fieldDto.FieldId,
                    parentClassId: parentClassId,
                    parentFileId: parentFileId,
                    name: fieldDto.Name,
                    typeId: typeToId[fieldDto.Type],
                    accessFlags: (int)fieldDto.AccFlag
                ));
                dbSourceIndicesAcc.Add(new DbSourceIndex(
                    elementId: fieldDto.FieldId,
                    fileId: fileId,
                    type: SourceCodeType.Field,
                    startLine: fieldDto.CodeRange.Start.Line,
                    startColumn: fieldDto.CodeRange.Start.Column,
                    endLine: fieldDto.CodeRange.End.Line,
                    endColumn: fieldDto.CodeRange.End.Column
                ));
            }
        }

        static void ProcessMethods(
            List<MethodDto> methodDtos,
            List<DbMethod> dbMethodsAcc,
            int parentFileId,
            int? parentClassId,
            Dictionary<string, int> typeToId,
            FileDto fileDto,
            List<DbArgument> dbArgumentsAcc,
            List<DbSourceIndex> dbSourceIndicesAcc,
            int fileId,
            Dictionary<string, int> methodSignatureToId)
        {
            foreach (MethodDto methodDto in methodDtos)
            {
                dbMethodsAcc.Add(new DbMethod(
                    id: methodDto.MethodId,
                    parentClassId: parentClassId,
                    parentFileId: parentFileId,
                    name: methodDto.Name,
                    returnTypeId: typeToId[methodDto.ReturnType],
                    accessFlags: (int)methodDto.AccFlag,
                    cyclomaticScore: methodDto.CyclomaticScore
                ));
                int argIndex = 0;
                foreach (ArgumentDto argumentDto in methodDto.Arguments)
                {
                    dbArgumentsAcc.Add(new DbArgument(
                        id: argumentDto.ArgumentId,
                        methodId: methodDto.MethodId,
                        argIndex: argIndex,
                        name: argumentDto.Name,
                        typeId: typeToId[argumentDto.Type]
                    ));
                    
                    argIndex++;
                }
                
                methodSignatureToId.TryAdd(methodDto.Signature, methodDto.MethodId);

                dbSourceIndicesAcc.Add(new DbSourceIndex(
                    elementId: methodDto.MethodId,
                    fileId: fileId,
                    type: SourceCodeType.Method,
                    startLine: methodDto.CodeRange.Start.Line,
                    startColumn: methodDto.CodeRange.Start.Column,
                    endLine: methodDto.CodeRange.End.Line,
                    endColumn: methodDto.CodeRange.End.Column
                ));
            }
        }
    }
}