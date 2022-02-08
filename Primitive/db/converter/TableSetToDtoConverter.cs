using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.dto;

namespace PrimitiveCodebaseElements.Primitive.db.converter
{
    public static class TableSetToDtoConverter
    {
        public static List<FileDto> ToFileDto(TableSet tableSet)
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

            var fieldIndices = tableSet.SourceIndices
                .Where(it => it.Type == "FIELD")
                .ToDictionary(it => it.ElementId);

            var classIndices = tableSet.SourceIndices
                .Where(it => it.Type == "CLASS")
                .ToDictionary(it => it.ElementId);

            ILookup<int, DbClassReference> classReferencesByClassId =
                tableSet.ClassReferences.ToLookup(it => it.FromId);

            ILookup<int, DbMethodReference> methodReferencesById = tableSet.MethodReferences.ToLookup(it => it.FromId);

            return tableSet.Files.Select(dbFile =>
                {
                    List<ClassDto> classes = classesByFileId[dbFile.Id]
                        .Select(dbClass =>
                        {
                            List<MethodDto> methodDtos = methodByClassId[dbClass.Id].Select(it => ToMethodDto(
                                    it,
                                    argsByMethodId[it.Id].ToList(),
                                    types,
                                    methodIndices[it.Id],
                                    dbClass,
                                    methodReferencesById[it.Id].ToList(),
                                    methodSignaturesById
                                ))
                                .ToList();

                            List<FieldDto> fieldDtos = fieldByClassId[dbClass.Id].Select(it => ToFieldDto(
                                    it,
                                    types,
                                    fieldIndices[it.Id]
                                ))
                                .ToList();

                            List<ClassReferenceDto> classReferences = classReferencesByClassId[dbClass.Id].Select(it =>
                                    new ClassReferenceDto(
                                        type: (CodeReferenceType)it.Type,
                                        fromFqn: dbClass.Fqn,
                                        toFqn: classesById[it.ToId].Fqn,
                                        startPosition: it.StartPosition,
                                        endPosition: it.EndPosition,
                                        startIdx: it.StartIdx,
                                        endIdx: it.EndIdx,
                                        codeRange: CodeRange(it)
                                    ))
                                .ToList();

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
                                header: dbClass.HeaderSource,
                                referencesFromThis: classReferences,
                                parentClassFqn: ParenClassFqn(dbClass.Fqn)
                            );
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
                })
                .ToList();
        }

        static string Signature(DbMethod method, Dictionary<int, DbClass> classes, List<DbArgument> args,
            Dictionary<int, DbType> types)
        {
            string argsString = string.Join(",", args.Select(it => types[it.TypeId]));
            string classFqn = classes[method.ParentId].Fqn;
            return $"{classFqn}.{method.Name}({argsString})";
        }


        [CanBeNull]
        static string PackageName(string fqn)
        {
            if (!fqn.Contains('.')) return null;
            return fqn.SubstringBeforeLast(".");
        }

        static string ClassName(string fqn)
        {
            if (fqn.Contains("$")) return fqn.SubstringAfterLast("$");
            return fqn.SubstringAfterLast(".");
        }

        static string ParenClassFqn(string fqn)
        {
            if (fqn.Contains("$")) return fqn.SubstringBeforeLast("$");
            return null;
        }

        static FieldDto ToFieldDto(
            DbField field,
            Dictionary<int, DbType> types,
            DbSourceIndex index
        )
        {
            return new FieldDto(
                field.Name,
                type: types[field.TypeId].Signature,
                (AccessFlags)field.AccessFlags,
                sourceCode: field.SourceCode,
                startIdx: index.StartIdx,
                endIdx: index.EndIdx,
                codeRange: CodeRange(index)
            );
        }

        static MethodDto ToMethodDto(
            DbMethod method,
            List<DbArgument> arguments,
            Dictionary<int, DbType> types,
            DbSourceIndex dbSourceIndex,
            DbClass cls,
            List<DbMethodReference> methodReferences,
            Dictionary<int, string> methodSignaturesById)
        {
            List<ArgumentDto> args = arguments
                .Select(it => new ArgumentDto(
                    index: it.ArgIndex,
                    name: it.Name,
                    type: types[it.TypeId].Signature
                ))
                .ToList();

            string methodSignature = MethodDto.MethodSignature(cls.Fqn, method.Name, args);

            List<MethodReferenceDto> methodReferenceDtos = methodReferences.Select(it => new MethodReferenceDto(
                    type: (CodeReferenceType)it.Type,
                    fromMethodSignature: methodSignature,
                    toMethodSignature: methodSignaturesById[it.ToId],
                    startPosition: it.StartPosition,
                    endPosition: it.EndPosition,
                    startIdx: it.StartIdx,
                    endIdx: it.EndIdx,
                    codeRange: CodeRange(it)
                ))
                .ToList();

            return new MethodDto(
                signature: methodSignature,
                name: method.Name,
                accFlag: (AccessFlags)method.AccessFlags,
                arguments: args,
                returnType: types[method.ReturnTypeId].Signature,
                sourceCode: method.SourceCode,
                startIdx: dbSourceIndex.StartIdx,
                endIdx: dbSourceIndex.EndIdx,
                codeRange: CodeRange(dbSourceIndex),
                methodReferences: methodReferenceDtos
            );
        }

        [CanBeNull]
        static dto.CodeRange CodeRange(DbClassReference dbSourceIndex)
        {
            if (dbSourceIndex.StartLine != null)
            {
                return new dto.CodeRange(
                    new CodeLocation(dbSourceIndex.StartLine.Value, dbSourceIndex.StartColumn.Value),
                    new CodeLocation(dbSourceIndex.EndLine.Value, dbSourceIndex.EndColumn.Value)
                );
            }

            return null;
        }

        [CanBeNull]
        static dto.CodeRange CodeRange(DbMethodReference dbSourceIndex)
        {
            if (dbSourceIndex.StartLine != null)
            {
                return new dto.CodeRange(
                    new CodeLocation(dbSourceIndex.StartLine.Value, dbSourceIndex.StartColumn.Value),
                    new CodeLocation(dbSourceIndex.EndLine.Value, dbSourceIndex.EndColumn.Value)
                );
            }

            return null;
        }

        [CanBeNull]
        static dto.CodeRange CodeRange(DbSourceIndex dbSourceIndex)
        {
            if (dbSourceIndex.StartLine != null)
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