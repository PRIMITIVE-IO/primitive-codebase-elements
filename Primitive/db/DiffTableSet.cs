using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;
using static PrimitiveCodebaseElements.Primitive.db.util.SqLiteUtil;

namespace PrimitiveCodebaseElements.Primitive.db
{
    [PublicAPI]
    public class DiffTableSet
    {
        public readonly IEnumerable<DbBranch> Branches;
        public readonly IEnumerable<DbDiffDirectoryDeleted> DirectoryDeleted;
        public readonly IEnumerable<DbDiffDirectoryAdded> DirectoryAdded;
        public readonly IEnumerable<DbDiffFileAdded> FileAdded;
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
            DirectoryAdded = directoryAdds ?? Enumerable.Empty<DbDiffDirectoryAdded>();
            DirectoryDeleted = directoryDeletes ?? Enumerable.Empty<DbDiffDirectoryDeleted>();
            FileAdded = fileAdds ?? Enumerable.Empty<DbDiffFileAdded>();
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
            DbDiffDirectoryAdded.SaveAll(tableSet.DirectoryAdded, conn);
            DbDiffDirectoryDeleted.SaveAll(tableSet.DirectoryDeleted, conn);
            DbDiffFileAdded.SaveAll(tableSet.FileAdded, conn);
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

        public static DiffTableSet ReadAllParallel(
            Func<IDbConnection> connectionProvider,
            ProgressTracker tracker = default
        )
        {
            tracker ??= ProgressTracker.Dummy;
            ProgressStepper stepper = tracker.Steps(16);
            using IDbConnection conn = connectionProvider();
            conn.Open();
            if (!TableExists(conn))
            {
                stepper.Done();
                return new DiffTableSet();
            }

            Task<List<DbBranch>> branches = LoadAsync(DbBranch.ReadAll, connectionProvider, stepper);
            Task<List<DbDiffDirectoryAdded>> directoryAdds = LoadAsync(DbDiffDirectoryAdded.ReadAll, connectionProvider, stepper);
            Task<List<DbDiffDirectoryDeleted>> directoryDeletes = LoadAsync(DbDiffDirectoryDeleted.ReadAll, connectionProvider, stepper);
            Task<List<DbDiffFileAdded>> fileAdds = LoadAsync(DbDiffFileAdded.ReadAll, connectionProvider, stepper);
            Task<List<DbDiffFileDeleted>> fileDeletes = LoadAsync(DbDiffFileDeleted.ReadAll, connectionProvider, stepper);
            Task<List<DbDiffFileModified>> fileModifications = LoadAsync(DbDiffFileModified.ReadAll, connectionProvider, stepper);
            Task<List<DbType>> types = LoadAsync(DbType.ReadAll, connectionProvider, stepper);
            Task<List<DbDiffClassDeleted>> classDeletes = LoadAsync(DbDiffClassDeleted.ReadAll, connectionProvider, stepper);
            Task<List<DbDiffClass>> classes = LoadAsync(DbDiffClass.ReadAll, connectionProvider, stepper);
            Task<List<DbDiffClassModifiedProperty>> classModifiedProperties = LoadAsync(DbDiffClassModifiedProperty.ReadAll, connectionProvider, stepper);
            Task<List<DbDiffFieldDeleted>> fieldDeletes = LoadAsync(DbDiffFieldDeleted.ReadAll, connectionProvider, stepper);
            Task<List<DbDiffField>> fields = LoadAsync(DbDiffField.ReadAll, connectionProvider, stepper);
            Task<List<DbDiffMethodDeleted>> methodDeletes = LoadAsync(DbDiffMethodDeleted.ReadAll, connectionProvider, stepper);
            Task<List<DbDiffMethod>> methods = LoadAsync(DbDiffMethod.ReadAll, connectionProvider, stepper);
            Task<List<DbDiffArgument>> arguments = LoadAsync(DbDiffArgument.ReadAll, connectionProvider, stepper);
            Task<List<DbDiffDirectoryCoordinates>> layout = LoadAsync(DbDiffDirectoryCoordinates.ReadAll, connectionProvider, stepper);

            return new DiffTableSet(
                branches: branches.Result,
                directoryAdds: directoryAdds.Result,
                directoryDeletes: directoryDeletes.Result,
                fileAdds: fileAdds.Result,
                fileDeletes: fileDeletes.Result,
                fileModifications: fileModifications.Result,
                types: types.Result,
                classDeletes: classDeletes.Result,
                classes: classes.Result,
                classModifiedProperties: classModifiedProperties.Result,
                fieldDeletes: fieldDeletes.Result,
                fields: fields.Result,
                methodDeletes: methodDeletes.Result,
                methods: methods.Result,
                arguments: arguments.Result,
                layout: layout.Result
            );
        }

        public static DiffTableSet ReadAll(IDbConnection conn, ProgressTracker tracker = default)
        {
            tracker ??= ProgressTracker.Dummy;
            ProgressStepper stepper = tracker.Steps(16);
            if (!TableExists(conn))
            {
                stepper.Done();
                return new DiffTableSet();
            }

            List<DbBranch> branches = DbBranch.ReadAll(conn);
            stepper.Step();
            List<DbDiffDirectoryAdded> directoryAdds = DbDiffDirectoryAdded.ReadAll(conn);
            stepper.Step();
            List<DbDiffDirectoryDeleted> directoryDeletes = DbDiffDirectoryDeleted.ReadAll(conn);
            stepper.Step();
            List<DbDiffFileAdded> fileAdds = DbDiffFileAdded.ReadAll(conn);
            stepper.Step();
            List<DbDiffFileDeleted> fileDeletes = DbDiffFileDeleted.ReadAll(conn);
            stepper.Step();
            List<DbDiffFileModified> fileModifications = DbDiffFileModified.ReadAll(conn);
            stepper.Step();
            List<DbType> types = DbType.ReadAll(conn);
            stepper.Step();
            List<DbDiffClassDeleted> classDeletes = DbDiffClassDeleted.ReadAll(conn);
            stepper.Step();
            List<DbDiffClass> classes = DbDiffClass.ReadAll(conn);
            stepper.Step();
            List<DbDiffClassModifiedProperty> classModifiedProperties = DbDiffClassModifiedProperty.ReadAll(conn);
            stepper.Step();
            List<DbDiffFieldDeleted> fieldDeletes = DbDiffFieldDeleted.ReadAll(conn);
            stepper.Step();
            List<DbDiffField> fields = DbDiffField.ReadAll(conn);
            stepper.Step();
            List<DbDiffMethodDeleted> methodDeletes = DbDiffMethodDeleted.ReadAll(conn);
            stepper.Step();
            List<DbDiffMethod> methods = DbDiffMethod.ReadAll(conn);
            stepper.Step();
            List<DbDiffArgument> arguments = DbDiffArgument.ReadAll(conn);
            stepper.Step();
            List<DbDiffDirectoryCoordinates> layout = DbDiffDirectoryCoordinates.ReadAll(conn);
            stepper.Step();

            return new DiffTableSet(
                branches: branches,
                directoryAdds: directoryAdds,
                directoryDeletes: directoryDeletes,
                fileAdds: fileAdds,
                fileDeletes: fileDeletes,
                fileModifications: fileModifications,
                types: types,
                classDeletes: classDeletes,
                classes: classes,
                classModifiedProperties: classModifiedProperties,
                fieldDeletes: fieldDeletes,
                fields: fields,
                methodDeletes: methodDeletes,
                methods: methods,
                arguments: arguments,
                layout: layout
            );
        }

        static bool TableExists(IDbConnection conn)
        {
            using IDbCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT name              
                FROM sqlite_master
                WHERE type = 'table' AND name = 'branches'
            ";

            using IDataReader reader = cmd.ExecuteReader();
            return reader.Read();
        }
    }
}