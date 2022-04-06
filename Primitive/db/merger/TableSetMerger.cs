using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db.merger
{
    public static class TableSetMerger
    {
        public static TableSet Merge(TableSet a, TableSet b)
        {
            int maxTypeIdA = a.Types.MaxOrDefault(it => it.Id);

            List<DbDirectory> dirs = a.Directories.Any() ? a.Directories : b.Directories;

            Dictionary<string, int> pathToDirId = dirs.ToDictionary(dir => dir.Fqn, dir => dir.Id);

            int DirIdx(string path)
            {
                var res = pathToDirId.GetValueOrDefault(path);
                if (res == 0) return -1;
                return res;
            }

            int maxClassIdA = a.Classes.MaxOrDefault(it => it.Id);

            int maxFileIdA = a.Files.MaxOrDefault(it => it.Id);

            HashSet<string> aFilePaths = a.Files.Select(aFile => aFile.Path).ToHashSet();

            List<DbFile> newBFiles = b.Files
                .Where(bFile => !aFilePaths.Contains(bFile.Path))
                .Select((newFileB, i) => new DbFile(
                    id: i + maxFileIdA + 1,
                    directoryId: DirIdx(newFileB.Path.SanitizePathSeparators()),
                    name: newFileB.Name,
                    path: newFileB.Path,
                    sourceText: newFileB.SourceText,
                    language: newFileB.Language
                )).ToList();
            
            List<DbFile> newFiles = a.Files.Concat(newBFiles).ToList();
            Dictionary<string, int> filePathToId = newFiles.ToDictionary(file => file.Path, file => file.Id);
            Dictionary<int, int> bFileIdToNewFileId = b.Files.ToDictionary(file => file.Id, file => filePathToId[file.Path]);

            List<DbClass> newClassesB = b.Classes.Select(dbClass => new DbClass(
                id: dbClass.Id + maxClassIdA,
                parentType: dbClass.ParentType,
                parentId: bFileIdToNewFileId[dbClass.ParentId],
                fqn: dbClass.Fqn,
                accessFlags: dbClass.AccessFlags,
                language: dbClass.Language,
                isTestClass: dbClass.IsTestClass
            )).ToList();

            int maxFieldIdA = a.Fields.MaxOrDefault(it => it.Id);

            IEnumerable<DbField> newFieldsB = b.Fields.Select(field => new DbField(
                id: field.Id + maxFieldIdA,
                parentType: field.ParentType,
                parentId: field.ParentId + maxClassIdA,
                name: field.Name,
                typeId: field.TypeId + maxTypeIdA,
                accessFlags: field.AccessFlags,
                language: field.Language
            ));

            int maxMethodIdA = a.Methods.MaxOrDefault(it => it.Id);

            IEnumerable<DbMethod> newMethodsB = b.Methods.Select(method => new DbMethod(
                id: method.Id + maxMethodIdA,
                parentType: method.ParentType,
                parentId: method.ParentId + maxClassIdA,
                name: method.Name,
                returnTypeId: method.ReturnTypeId + maxTypeIdA,
                accessFlags: method.AccessFlags,
                language: method.Language,
                cyclomaticScore: method.CyclomaticScore
            ));

            IEnumerable<DbType> newTypesB = b.Types.Select(type => new DbType(
                id: type.Id + maxTypeIdA,
                signature: type.Signature
            ));

            int maxArgumentIdA = a.Arguments.MaxOrDefault(it => it.Id);

            IEnumerable<DbArgument> newArgumentsB = b.Arguments.Select(arg => new DbArgument(
                id: arg.Id + maxArgumentIdA,
                methodId: arg.MethodId + maxMethodIdA,
                argIndex: arg.ArgIndex,
                name: arg.Name,
                typeId: arg.TypeId + maxTypeIdA
            ));

            int maxClassRefIdA = a.ClassReferences.MaxOrDefault(it => it.Id);

            IEnumerable<DbClassReference> newClassReferencesB = b.ClassReferences.Select(classRef => new DbClassReference(
                id: classRef.Id + maxClassRefIdA,
                type: classRef.Type + maxTypeIdA,
                fromId: classRef.FromId + maxClassIdA,
                toId: classRef.ToId + maxClassIdA,
                startPosition: classRef.StartPosition,
                endPosition: classRef.EndPosition,
                startIdx: classRef.StartIdx,
                endIdx: classRef.EndIdx,
                startLine: classRef.StartLine,
                startColumn: classRef.StartColumn,
                endLine: classRef.EndLine,
                endColumn: classRef.EndColumn
            ));

            int maxMethodRefIdA = a.MethodReferences.MaxOrDefault(it => it.Id);

            IEnumerable<DbMethodReference> newMethodReferencesB = b.MethodReferences.Select(methodRef => new DbMethodReference(
                id: methodRef.Id + maxMethodRefIdA,
                type: methodRef.Type,
                fromId: methodRef.FromId + maxMethodIdA,
                toId: methodRef.ToId + maxMethodIdA,
                startPosition: methodRef.StartPosition,
                endPosition: methodRef.EndPosition,
                startIdx: methodRef.StartIdx,
                endIdx: methodRef.EndIdx,
                startLine: methodRef.StartLine,
                startColumn: methodRef.StartColumn,
                endLine: methodRef.EndLine,
                endColumn: methodRef.EndColumn
            ));

            IEnumerable<DbSourceIndex> newSourceIndicesB = b.SourceIndices.Select(bSourceIndex => new DbSourceIndex(
                elementId: bSourceIndex.ElementId + bSourceIndex.Type switch
                {
                    "CLASS" => maxClassIdA,
                    "METHOD" => maxMethodIdA,
                    "FIELD" => maxFieldIdA,
                },
                fileId: bFileIdToNewFileId[bSourceIndex.FileId],
                type: bSourceIndex.Type,
                startIdx: bSourceIndex.StartIdx,
                endIdx: bSourceIndex.EndIdx,
                startLine: bSourceIndex.StartLine,
                startColumn: bSourceIndex.StartColumn,
                endLine: bSourceIndex.EndLine,
                endColumn: bSourceIndex.EndColumn
            ));

            return new TableSet(
                directories: dirs,
                arguments: a.Arguments.Concat(newArgumentsB).ToList(),
                classes: a.Classes.Concat(newClassesB).ToList(),
                classReferences: a.ClassReferences.Concat(newClassReferencesB).ToList(),
                fields: a.Fields.Concat(newFieldsB).ToList(),
                files: newFiles,
                methods: a.Methods.Concat(newMethodsB).ToList(),
                methodReferences: a.MethodReferences.Concat(newMethodReferencesB).ToList(),
                types: a.Types.Concat(newTypesB).ToList(),
                sourceIndices: a.SourceIndices.Concat(newSourceIndicesB).ToList()
            );
        }
    }
}