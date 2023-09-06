using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using FluentAssertions;
using PrimitiveCodebaseElements.Primitive.db;
using Xunit;
using DbType = PrimitiveCodebaseElements.Primitive.db.DbType;

namespace PrimitiveCodebaseElements.Tests.db;

public class DbTest
{
    [Fact]
    public void DbReadWrite()
    {
        string dbPath = Path.GetTempFileName();
        try
        {
            using IDbConnection conn = new SQLiteConnection($"URI=file:{dbPath}");
            conn.Open();
            // @formatter:off
            TableSet ts = new TableSet(
                files: new List<DbFile> { new(id: 1, directoryId: 1, name: string.Empty, path: string.Empty, sourceText: string.Empty, language: 1) },
                types: new List<DbType> { new(id: 1, signature: string.Empty) },
                classes: new List<DbClass> { new(id: 1, parentClassId: 1, parentFileId: 1, fqn: string.Empty, accessFlags: 1, isTestClass: 1) },
                methods: new List<DbMethod> {
                    new (id: 1, parentClassId: 1, parentFileId: 1, name: string.Empty, returnTypeId: 1, accessFlags: 1, cyclomaticScore: 1), 
                    new (id: 2, parentClassId: 1, parentFileId: 1, name: string.Empty, returnTypeId: 1, accessFlags: 1, cyclomaticScore: null) 
                },
                directories: new List<DbDirectory>{new(id: 1, fqn:string.Empty, positionX:1.0, positionY: 1.0)},
                arguments: new List<DbArgument>{new (id: 1, methodId:1, argIndex: 0, name: string.Empty, typeId:1)},
                classReferences: new List<DbClassReference>{new(id: 1, type:1, fromId:1, toId:1, startLine:1, startColumn:1, endLine:1, endColumn:1)},
                fields: new List<DbField>{new (id: 1, parentClassId:1, parentFileId:1, name:string.Empty, typeId:1, accessFlags:1)},
                methodReferences: new List<DbMethodReference>{new(id: 1, type:1, fromId:1, toId:1, startLine:1, startColumn:1, endLine:1, endColumn:1)},
                sourceIndices: new List<DbSourceIndex>{new (elementId:1, fileId:1, type: 0, startLine:1, startColumn:1, endLine:1, endColumn:1)}
            );
            // @formatter:on
            TableSet.CreateTables(conn);
            TableSet.Save(ts, conn);
            TableSet readTs = TableSet.ReadAll(conn);

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
            DiffTableSet dts = new DiffTableSet(
                branches: new List<DbBranch>{new(1, string.Empty, string.Empty)},
                directoryAdds: new List<DbDiffDirectoryAdded>{new(id:1, fqn: string.Empty, branchId:1)},
                directoryDeletes: new List<DbDiffDirectoryDeleted>{new(directoryId:1, branchId:1)},
                fileAdds: new List<DbDiffFileAdded>{new(id: 1, branchId: 1, path: string.Empty, directoryId:1, directoryIdDiff: 1, content:string.Empty, languageId:1)},
                fileDeletes: new List<DbDiffFileDeleted>{new(id: 1, branchId: 1, fileId:1)},
                fileModifications:new List<DbDiffFileModified>{new(id: 1, branchId: 1, fileId: 1, changeContent:string.Empty, oldName: string.Empty)},
                types: new List<DbType>{new(id: 2, signature:string.Empty)},
                classDeletes: new List<DbDiffClassDeleted>{new(classId:1, branchId:1)},
                classes: new List<DbDiffClass>{new(id: 1, parentClassId:1, parentFileId:1, parentClassIdDiff:1, parentFileIdDiff:1, fqn:string.Empty, accessFlags:1, headerSource:string.Empty,isTestClass:1, branchId:1 )},
                classModifiedProperties: new List<DbDiffClassModifiedProperty>{new(originalClassId:1, accessFlags:1, headerSource:string.Empty,branchId:1 )},
                fieldDeletes: new List<DbDiffFieldDeleted>{new(fieldId:1, branchId:1)},
                fields: new List<DbDiffField>{new(id: 1, parentClassId:1, parentFileId:1, parentClassIdDiff: 1,parentFileIdDiff:1, name:string.Empty, typeId:1, accessFlags:1, sourceCode:string.Empty, originalFieldId:1, branchId:1)},
                methodDeletes: new List<DbDiffMethodDeleted>{new(methodId:1, branchId:1)},
                methods: new List<DbDiffMethod>{new(id: 1, parentClassId:1, parentFileId:1, parentClassIdDiff: 1,parentFileIdDiff:1, name:string.Empty, accessFlags:1, sourceCode:string.Empty, branchId:1, returnTypeId:1, originalMethodId: 1, fieldId:"1")},
                arguments: new List<DbDiffArgument>{new(id: 1, methodId:1, argIndex:1, name: string.Empty, typeId:1)},
                layout: new List<DbDiffDirectoryCoordinates>{new(branchId:1, originalDirectoryId:1, addedDirectoryId:1, isDeleted:1, positionX:1, positionY:1)}
            );
            // @formatter:on
            DiffTableSet.CreateTables(conn);
            /*
            DiffTableSet.Save(dts, conn);
            DiffTableSet readDts = DiffTableSet.ReadAll(conn);

            readDts.Branches.Should().HaveCount(1);
            readDts.DirectoryDeleted.Should().HaveCount(1);
            readDts.DirectoryAdded.Should().HaveCount(1);
            readDts.FileAdded.Should().HaveCount(1);
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
            */
        }
        finally
        {
            File.Delete(dbPath);
        }
    }
}