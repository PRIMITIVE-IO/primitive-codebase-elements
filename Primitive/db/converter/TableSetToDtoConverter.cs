#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.dto;
using PrimitiveLogger;

namespace PrimitiveCodebaseElements.Primitive.db.converter
{
    [PublicAPI]
    public static class TableSetToDtoConverter
    {
        public static List<DirectoryDto> ToDirectoryDto(TableSet tableSet)
        {
            ILookup<string, FileDto> parentPathToFileDtos = ToFileDto(tableSet)
                .ToLookup(file => file.Path.SubstringBeforeLast("/"));

            return tableSet.Directories.Select(dir => new DirectoryDto(
                    dir.Fqn,
                    dir.PositionX,
                    dir.PositionY,
                    parentPathToFileDtos[dir.Fqn].ToList()
                ))
                .ToList();
        }

        public static List<FileDto> ToFileDto(TableSet tableSet)
        {
            try
            {
                ILookup<int, DbClass> classesByFileId = tableSet.Classes.ToLookup(it => it.ParentId);

                Dictionary<int, DbClass> classesById = tableSet.Classes.ToDictionary(it => it.Id);
                ILookup<int, DbMethod> methodByClassId = tableSet.Methods.ToLookup(it => it.ParentId);

                ILookup<int, DbField> fieldByClassId = tableSet.Fields.ToLookup(it => it.ParentId);
                Dictionary<int, DbType> types = tableSet.Types.ToDictionary(it => it.Id);
                ILookup<int, DbArgument> argsByMethodId = tableSet.Arguments.ToLookup(it => it.MethodId);
                Dictionary<int, string> methodSignaturesById = tableSet.Methods.ToDictionary(it => it.Id,
                    it => Signature(it, classesById, argsByMethodId[it.Id].ToList(), types));

                Dictionary<int, DbSourceIndex> methodIndices = tableSet.SourceIndices
                    .Where(it => it.Type == "METHOD")
                    .ToDictionary(it => it.ElementId);

                Dictionary<int, DbSourceIndex> fieldIndices = tableSet.SourceIndices
                    .Where(it => it.Type == "FIELD")
                    .ToDictionary(it => it.ElementId);

                Dictionary<int, DbSourceIndex> classIndices = tableSet.SourceIndices
                    .Where(it => it.Type == "CLASS")
                    .ToDictionary(it => it.ElementId);

                ILookup<int, DbClassReference> classReferencesByClassId =
                    tableSet.ClassReferences.ToLookup(it => it.FromId);

                ILookup<int, DbMethodReference> methodReferencesById =
                    tableSet.MethodReferences.ToLookup(it => it.FromId);

                return tableSet.Files.SelectNotNull(dbFile =>
                    {
                        try
                        {
                            List<ClassDto> classes = classesByFileId[dbFile.Id]
                                .SelectNotNull(dbClass =>
                                {
                                    try
                                    {
                                        List<MethodDto> methodDtos = methodByClassId[dbClass.Id].SelectNotNull(it =>
                                                ToMethodDto(
                                                    it,
                                                    argsByMethodId[it.Id].ToList(),
                                                    types,
                                                    methodIndices[it.Id],
                                                    dbClass,
                                                    methodReferencesById[it.Id].ToList(),
                                                    methodSignaturesById
                                                ))
                                            .ToList()!;

                                        List<FieldDto> fieldDtos = fieldByClassId[dbClass.Id].SelectNotNull(it =>
                                                ToFieldDto(
                                                    it,
                                                    types,
                                                    fieldIndices[it.Id]
                                                ))
                                            .ToList()!;

                                        List<ClassReferenceDto> classReferences = classReferencesByClassId[dbClass.Id]
                                            .SelectNotNull(
                                                it =>
                                                {
                                                    try
                                                    {
                                                        return new ClassReferenceDto(
                                                            type: (CodeReferenceType)it.Type,
                                                            fromFqn: dbClass.Fqn,
                                                            toFqn: classesById[it.ToId].Fqn,
                                                            startPosition: it.StartPosition,
                                                            endPosition: it.EndPosition,
                                                            startIdx: it.StartIdx,
                                                            endIdx: it.EndIdx,
                                                            codeRange: CodeRange(it)
                                                        );
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Logger.Instance().Warn($"Cannot create ClassReferenceDto id: {it.Id}", ex);
                                                        return null;
                                                    }
                                                }
                                            )
                                            .ToList()!;

                                        DbSourceIndex classIndex = classIndices[dbClass.Id];
                                        return new ClassDto(
                                            dbFile.Path,
                                            packageName: PackageName(dbClass.Fqn),
                                            name: ClassName(dbClass.Fqn),
                                            fullyQualifiedName: dbClass.Fqn,
                                            methods: methodDtos,
                                            fields: fieldDtos,
                                            modifier: (AccessFlags)dbClass.AccessFlags,
                                            startIdx: classIndex.StartIdx,
                                            endIdx: classIndex.EndIdx,
                                            codeRange: CodeRange(classIndex),
                                            header: string.Empty,
                                            referencesFromThis: classReferences,
                                            parentClassFqn: ParenClassFqn(dbClass.Fqn)
                                        );
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Instance().Warn($"Cannot crate ClassDto for {dbClass.Fqn}", ex);
                                        return null;
                                    }
                                })
                                .ToList();

                            bool isTest = classesByFileId[dbFile.Id].Any(it => it.IsTestClass == 1);

                            return new FileDto(
                                text: dbFile.SourceText,
                                path: dbFile.Path,
                                isTest: isTest,
                                classes: classes,
                                language: (SourceCodeLanguage)dbFile.Language
                            );
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance().Warn($"Cannot create FileDto for {dbFile.Path}", ex);
                            return null;
                        }
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.Instance().Warn("Cannot convert TableSet to file DTOs", ex);
                return new List<FileDto>();
            }
        }

        static string Signature(
            DbMethod method,
            IReadOnlyDictionary<int, DbClass> classes,
            IEnumerable<DbArgument> args,
            IReadOnlyDictionary<int, DbType> types)
        {
            string argsString = args
                .Select(it => types[it.TypeId].Signature)
                .JoinToString(",");

            string classFqn = classes[method.ParentId].Fqn;
            return $"{classFqn}.{method.Name}({argsString})";
        }


        static string? PackageName(string fqn)
        {
            return !fqn.Contains('.') ? null : fqn.SubstringBeforeLast(".");
        }

        static string ClassName(string fqn)
        {
            return fqn.SubstringAfterLast(fqn.Contains('$') ? "$" : ".");
        }

        static string? ParenClassFqn(string fqn)
        {
            return fqn.Contains('$') ? fqn.SubstringBeforeLast("$") : null;
        }

        static FieldDto? ToFieldDto(
            DbField field,
            IReadOnlyDictionary<int, DbType> types,
            DbSourceIndex index
        )
        {
            try
            {
                return new FieldDto(
                    field.Name,
                    type: types[field.TypeId].Signature,
                    (AccessFlags)field.AccessFlags,
                    sourceCode: string.Empty,
                    startIdx: index.StartIdx,
                    endIdx: index.EndIdx,
                    codeRange: CodeRange(index)
                );
            }
            catch (Exception ex)
            {
                Logger.Instance().Warn($"Cannot create FieldDto, id: {field.Id}", ex);
                return null;
            }
        }

        static MethodDto? ToMethodDto(
            DbMethod method,
            List<DbArgument> arguments,
            Dictionary<int, DbType> types,
            DbSourceIndex dbSourceIndex,
            DbClass cls,
            List<DbMethodReference> methodReferences,
            Dictionary<int, string> methodSignaturesById)
        {
            try
            {
                List<ArgumentDto> args = arguments
                    .Select(it => new ArgumentDto(
                        index: it.ArgIndex,
                        name: it.Name,
                        type: types[it.TypeId].Signature
                    ))
                    .ToList();

                string methodSignature = MethodDto.MethodSignature(cls.Fqn, method.Name, args);

                List<MethodReferenceDto> methodReferenceDtos = methodReferences.SelectNotNull(it =>
                    {
                        try
                        {
                            return new MethodReferenceDto(
                                type: (CodeReferenceType)it.Type,
                                fromMethodSignature: methodSignature,
                                toMethodSignature: methodSignaturesById[it.ToId],
                                startPosition: it.StartPosition,
                                endPosition: it.EndPosition,
                                startIdx: it.StartIdx,
                                endIdx: it.EndIdx,
                                codeRange: CodeRange(it)
                            );
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance().Warn($"Cannot create MethodReferenceDto, id: {it.Id}", ex);
                            return null;
                        }
                    })
                    .ToList();

                return new MethodDto(
                    signature: methodSignature,
                    name: method.Name,
                    accFlag: (AccessFlags)method.AccessFlags,
                    arguments: args,
                    returnType: types[method.ReturnTypeId].Signature,
                    sourceCode: string.Empty,
                    startIdx: dbSourceIndex.StartIdx,
                    endIdx: dbSourceIndex.EndIdx,
                    codeRange: CodeRange(dbSourceIndex),
                    methodReferences: methodReferenceDtos,
                    cyclomaticScore: method.CyclomaticScore
                );
            }
            catch (Exception ex)
            {
                Logger.Instance().Warn($"Cannot create MethodDto id: {method.Id}", ex);
                return null;
            }
        }

        static dto.CodeRange? CodeRange(DbClassReference dbSourceIndex)
        {
            if (dbSourceIndex is { StartLine: { }, StartColumn: { }, EndLine: { }, EndColumn: { } }) // check all not null
            {
                return new dto.CodeRange(
                    new CodeLocation(dbSourceIndex.StartLine.Value, dbSourceIndex.StartColumn.Value),
                    new CodeLocation(dbSourceIndex.EndLine.Value, dbSourceIndex.EndColumn.Value)
                );
            }

            return null;
        }

        static dto.CodeRange? CodeRange(DbMethodReference dbSourceIndex)
        {
            if (dbSourceIndex is { StartLine: { }, StartColumn: { }, EndLine: { }, EndColumn: { } }) // check all not null
            {
                return new dto.CodeRange(
                    new CodeLocation(dbSourceIndex.StartLine.Value, dbSourceIndex.StartColumn.Value),
                    new CodeLocation(dbSourceIndex.EndLine.Value, dbSourceIndex.EndColumn.Value)
                );
            }

            return null;
        }

        static dto.CodeRange? CodeRange(DbSourceIndex dbSourceIndex)
        {
            if (dbSourceIndex is { StartLine: { }, StartColumn: { }, EndLine: { }, EndColumn: { } }) // check all not null
            {
                return new dto.CodeRange(
                    new CodeLocation(dbSourceIndex.StartLine.Value, dbSourceIndex.StartColumn.Value),
                    new CodeLocation(dbSourceIndex.EndLine.Value, dbSourceIndex.EndColumn.Value)
                );
            }

            return null;
        }
    }
}