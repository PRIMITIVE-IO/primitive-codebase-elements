using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    public class DiffTableSet
    {
        public readonly IEnumerable<DbBranch> Branches;
        public readonly IEnumerable<DbDiffDirectoryDeleted> DirectoryDeleted;
        public readonly IEnumerable<DbDiffDirectoryAdded> DirectoryAded;
        public readonly IEnumerable<DbDiffFileAdded> FileAded;
        public readonly IEnumerable<DbDiffFileDeleted> FileDeleted;
        public readonly IEnumerable<DbDiffFileModified> FileModified;
        public readonly IEnumerable<DbType> Types;
        public readonly IEnumerable<DbDiffClassDeleted> ClassDeleted;
        public readonly IEnumerable<DbDiffClass> Classes;
        public readonly IEnumerable<DbDiffClassModifiedProperty> ClassModifiedProperties;
        public readonly IEnumerable<DbDiffFieldDeleted> FieldDeleted;
        public readonly IEnumerable<DbDiffField> Fields;
        public readonly IEnumerable<DbDiffMethodDeleted> MethodDeleted;
        public readonly IEnumerable<DbDiffMethod> Methods;
        public readonly IEnumerable<DbDiffArgument> Arguments;
        public readonly IEnumerable<DbDiffDirectoryCoordinates> Layout;

        public DiffTableSet(
            IEnumerable<DbBranch> branches = null,
            IEnumerable<DbDiffDirectoryAdded> directoryAdds = null,
            IEnumerable<DbDiffDirectoryDeleted> directoryDeletes = null,
            IEnumerable<DbDiffFileAdded> fileAdds = null,
            IEnumerable<DbDiffFileDeleted> fileDeletes = null,
            IEnumerable<DbDiffFileModified> fileModifications = null,
            IEnumerable<DbType> types = null,
            IEnumerable<DbDiffClassDeleted> classDeletes = null,
            IEnumerable<DbDiffClass> classes = null,
            IEnumerable<DbDiffClassModifiedProperty> classModifiedProperties = null,
            IEnumerable<DbDiffFieldDeleted> fieldDeletes = null,
            IEnumerable<DbDiffField> fields = null,
            IEnumerable<DbDiffMethodDeleted> methodDeletes = null,
            IEnumerable<DbDiffMethod> methods = null,
            IEnumerable<DbDiffArgument> arguments = null,
            IEnumerable<DbDiffDirectoryCoordinates> layout = null
        )
        {
            Branches = branches ?? Enumerable.Empty<DbBranch>();
            DirectoryAded = directoryAdds ?? Enumerable.Empty<DbDiffDirectoryAdded>();
            DirectoryDeleted = directoryDeletes ?? Enumerable.Empty<DbDiffDirectoryDeleted>();
            FileAded = fileAdds ?? Enumerable.Empty<DbDiffFileAdded>();
            FileDeleted = fileDeletes ?? Enumerable.Empty<DbDiffFileDeleted>();
            FileModified = fileModifications ?? Enumerable.Empty<DbDiffFileModified>();
            Types = types ?? Enumerable.Empty<DbType>();
            ClassDeleted = classDeletes ?? Enumerable.Empty<DbDiffClassDeleted>();
            Classes = classes ?? Enumerable.Empty<DbDiffClass>();
            ClassModifiedProperties = classModifiedProperties ?? Enumerable.Empty<DbDiffClassModifiedProperty>();
            FieldDeleted = fieldDeletes ?? Enumerable.Empty<DbDiffFieldDeleted>();
            Fields = fields ?? Enumerable.Empty<DbDiffField>();
            MethodDeleted = methodDeletes ?? Enumerable.Empty<DbDiffMethodDeleted>();
            Methods = methods ?? Enumerable.Empty<DbDiffMethod>();
            Arguments = arguments ?? Enumerable.Empty<DbDiffArgument>();
            Layout = layout ?? Enumerable.Empty<DbDiffDirectoryCoordinates>();
        }

        public static void CreateTables(IDbConnection conn)
        {
            conn.ExecuteNonQuery(@"PRAGMA foreign_keys = ON;");

            conn.ExecuteNonQuery(DbBranch.CreateTable);
            conn.ExecuteNonQuery(DbDiffDirectoryAdded.CreateTable);
            conn.ExecuteNonQuery(DbDiffDirectoryDeleted.CreateTable);
            conn.ExecuteNonQuery(DbDiffFileAdded.CreateTable);
            conn.ExecuteNonQuery(DbDiffFileDeleted.CreateTable);
            conn.ExecuteNonQuery(DbDiffFileModified.CreateTable);
            conn.ExecuteNonQuery(DbDiffClassDeleted.CreateTable);
            conn.ExecuteNonQuery(DbDiffClass.CreateTable);
            conn.ExecuteNonQuery(DbDiffClassModifiedProperty.CreateTable);
            conn.ExecuteNonQuery(DbDiffFieldDeleted.CreateTable);
            conn.ExecuteNonQuery(DbDiffField.CreateTable);
            conn.ExecuteNonQuery(DbDiffMethodDeleted.CreateTable);
            conn.ExecuteNonQuery(DbDiffMethod.CreateTable);
            conn.ExecuteNonQuery(DbDiffArgument.CreateTable);
            conn.ExecuteNonQuery(DbDiffDirectoryCoordinates.CreateTable);
        }

        public static void Save(DiffTableSet tableSet, IDbConnection conn)
        {
            DbBranch.SaveAll(tableSet.Branches, conn);
            DbDiffDirectoryAdded.SaveAll(tableSet.DirectoryAded, conn);
            DbDiffDirectoryDeleted.SaveAll(tableSet.DirectoryDeleted, conn);
            DbDiffFileAdded.SaveAll(tableSet.FileAded, conn);
            DbDiffFileDeleted.SaveAll(tableSet.FileDeleted, conn);
            DbDiffFileModified.SaveAll(tableSet.FileModified, conn);
            DbType.SaveAll(tableSet.Types, conn);
            DbDiffClassDeleted.SaveAll(tableSet.ClassDeleted, conn);
            DbDiffClass.SaveAll(tableSet.Classes, conn);
            DbDiffClassModifiedProperty.SaveAll(tableSet.ClassModifiedProperties, conn);
            DbDiffFieldDeleted.SaveAll(tableSet.FieldDeleted, conn);
            DbDiffField.SaveAll(tableSet.Fields, conn);
            DbDiffMethodDeleted.SaveAll(tableSet.MethodDeleted, conn);
            DbDiffMethod.SaveAll(tableSet.Methods, conn);
            DbDiffArgument.SaveAll(tableSet.Arguments, conn);
            DbDiffDirectoryCoordinates.SaveAll(tableSet.Layout, conn);
        }

        public static DiffTableSet ReadAll(DbConnection conn)
        {
            return new DiffTableSet(
                branches: DbBranch.ReadAll(conn),
                directoryAdds: DbDiffDirectoryAdded.ReadAll(conn),
                directoryDeletes: DbDiffDirectoryDeleted.ReadAll(conn),
                fileAdds: DbDiffFileAdded.ReadAll(conn),
                fileDeletes: DbDiffFileDeleted.ReadAll(conn),
                fileModifications: DbDiffFileModified.ReadAll(conn),
                types: DbType.ReadAll(conn),
                classDeletes: DbDiffClassDeleted.ReadAll(conn),
                classes: DbDiffClass.ReadAll(conn),
                classModifiedProperties: DbDiffClassModifiedProperty.ReadAll(conn),
                fieldDeletes: DbDiffFieldDeleted.ReadAll(conn),
                fields: DbDiffField.ReadAll(conn),
                methodDeletes: DbDiffMethodDeleted.ReadAll(conn),
                methods: DbDiffMethod.ReadAll(conn),
                arguments: DbDiffArgument.ReadAll(conn),
                layout: DbDiffDirectoryCoordinates.ReadAll(conn)
            );
        }
    }
}