using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using FluentAssertions;
using PrimitiveCodebaseElements.Primitive.db;
using Xunit;

namespace PrimitiveCodebaseElements.Tests.db;

public class DbTest
{
    [Fact]
    public void DbReadWrite()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            using (DbConnection conn = new SQLiteConnection($"URI=file:{dbPath}"))
            {
                conn.Open();
                // @formatter:off
                var ts = new TableSet(
                    files: new List<DbFile> { new(id: 1, directoryId: 1, name: "", path: "", sourceText: "", language: 1) },
                    types: new List<DbType> { new(id: 1, signature: "") },
                    classes: new List<DbClass> { new(id: 1, parentType: 1, parentId: 1, fqn: "", accessFlags: 1, language: 1, isTestClass: 1) },
                    methods: new List<DbMethod> {
                        new (id: 1, parentType: 1, parentId: 1, name: "", returnTypeId: 1, accessFlags: 1, language: 1, cyclomaticScore: 1), 
                        new (id: 2, parentType: 1, parentId: 1, name: "", returnTypeId: 1, accessFlags: 1, language: 1, cyclomaticScore: null) 
                    },
                    directories: new List<DbDirectory>{new(id: 1, fqn:"", positionX:1.0, positionY: 1.0)},
                    arguments: new List<DbArgument>{new (id: 1, methodId:1, argIndex: 0, name: "", typeId:1)},
                    classReferences: new List<DbClassReference>{new(id: 1, type:1, fromId:1, toId:1, startPosition:1, endPosition:1, startIdx: 1, endIdx: 1, startLine:1, startColumn:1, endLine:1, endColumn:1)},
                    fields: new List<DbField>{new (id: 1, parentType:1, parentId:1, name:"", typeId:1, accessFlags:1, language:1)},
                    methodReferences: new List<DbMethodReference>{new(id: 1, type:1, fromId:1, toId:1,startPosition:1, endPosition:1, startIdx: 1, endIdx: 1, startLine:1, startColumn:1, endLine:1, endColumn:1)},
                    sourceIndices: new List<DbSourceIndex>{new (elementId:1, fileId:1, type: "", startIdx: 1, endIdx: 1, startLine:1, startColumn:1, endLine:1, endColumn:1)}
                );
                // @formatter:on
                TableSet.CreateTables(conn);
                TableSet.Save(ts, conn);
                var readTs = TableSet.ReadAll(conn);

                readTs.Directories.Should().HaveCount(1);
                readTs.Arguments.Should().HaveCount(1);
                readTs.Classes.Should().HaveCount(1);
                readTs.ClassReferences.Should().HaveCount(1);
                readTs.Fields.Should().HaveCount(1);
                readTs.Files.Should().HaveCount(1);
                readTs.Files[0].DirectoryId.Should().Be(1);
                readTs.Methods.Should().HaveCount(2);
                readTs.MethodReferences.Should().HaveCount(1);
                readTs.Types.Should().HaveCount(1);
                readTs.SourceIndices.Should().HaveCount(1);

                // @formatter:off
                var dts = new DiffTableSet(
                        branches: new List<DbBranch>{new(1, "")},
                        directoryAdds: new List<DbDiffDirectoryAdded>{new(id:1, fqn: "", branchId:1)},
                        directoryDeletes: new List<DbDiffDirectoryDeleted>{new(directoryId:1, branchId:1)},
                        fileAdds: new List<DbDiffFileAdded>{new(id: 1, branchId: 1, path: "", directoryId:1, directoryIdDiff: 1, content:"", languageId:1)},
                        fileDeletes: new List<DbDiffFileDeleted>{new(id: 1, branchId: 1, fileId:1)},
                        fileModifications:new List<DbDiffFileModified>{new(id: 1, branchId: 1, fileId: 1, changeContent:"", oldName: "")},
                        types: new List<DbType>{new(id: 2, signature:"")},
                        classDeletes: new List<DbDiffClassDeleted>{new(classId:1, branchId:1)},
                        classes: new List<DbDiffClass>{new(id: 1, parentType:1, parentId:1, parentIdDiff:1, fqn:"", accessFlags:1, headerSource:"", language:1,isTestClass:1, branchId:1 )},
                        classModifiedProperties: new List<DbDiffClassModifiedProperty>{new(originalClassId:1, accessFlags:1, headerSource:"",branchId:1 )},
                        fieldDeletes: new List<DbDiffFieldDeleted>{new(fieldId:1, branchId:1)},
                        fields: new List<DbDiffField>{new(id: 1, parentType:1, parentId:1, parentIdDiff: 1, name:"", typeId:1, accessFlags:1, sourceCode:"", language:1, originalFieldId:1, branchId:1)},
                        methodDeletes: new List<DbDiffMethodDeleted>{new(methodId:1, branchId:1)},
                        methods: new List<DbDiffMethod>{new(id: 1, parentType:1, parentId:1, parentIdDiff: 1, name:"", accessFlags:1, sourceCode:"", language:1, branchId:1, returnTypeId:1, originalMethodId: 1, fieldId:"1")},
                        arguments: new List<DbDiffArgument>{new(id: 1, methodId:1, argIndex:1, name: "", typeId:1)},
                        layout: new List<DbDiffDirectoryCoordinates>{new(branchId:1, originalDirectoryId:1, addedDirectoryId:1, isDeleted:1, positionX:1, positionY:1)}
                );
                // @formatter:on
                DiffTableSet.CreateTables(conn);
                DiffTableSet.Save(dts, conn);
                var readDts = DiffTableSet.ReadAll(conn);

                readDts.Branches.Should().HaveCount(1);
                readDts.DirectoryDeleted.Should().HaveCount(1);
                readDts.DirectoryAded.Should().HaveCount(1);
                readDts.FileAded.Should().HaveCount(1);
                readDts.FileDeleted.Should().HaveCount(1);
                readDts.FileModified.Should().HaveCount(1);
                readDts.Types.Should().HaveCount(2);
                readDts.ClassDeleted.Should().HaveCount(1);
                readDts.Classes.Should().HaveCount(1);
                readDts.ClassModifiedProperties.Should().HaveCount(1);
                readDts.FieldDeleted.Should().HaveCount(1);
                readDts.Fields.Should().HaveCount(1);
                readDts.MethodDeleted.Should().HaveCount(1);
                readDts.Methods.Should().HaveCount(1);
                readDts.Arguments.Should().HaveCount(1);
                readDts.Layout.Should().HaveCount(1);
            }
        }
        finally
        {
            File.Delete(dbPath);
        }
    }
}