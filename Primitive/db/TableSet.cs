using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    public class TableSet
    {
        public readonly List<DbDirectory> Directories;
        public readonly List<DbArgument> Arguments;
        public readonly List<DbClass> Classes;
        public readonly List<DbClassReference> ClassReferences;
        public readonly List<DbField> Fields;
        public readonly List<DbFile> Files;
        public readonly List<DbMethod> Methods;
        public readonly List<DbMethodReference> MethodReferences;
        public readonly List<DbType> Types;
        public readonly List<DbSourceIndex> SourceIndices;

        public TableSet(
            [CanBeNull] List<DbDirectory> directories = null,
            [CanBeNull] List<DbArgument> arguments = null,
            [CanBeNull] List<DbClass> classes = null,
            [CanBeNull] List<DbClassReference> classReferences = null,
            [CanBeNull] List<DbField> fields = null,
            [CanBeNull] List<DbFile> files = null,
            [CanBeNull] List<DbMethod> methods = null,
            [CanBeNull] List<DbMethodReference> methodReferences = null,
            [CanBeNull] List<DbType> types = null,
            [CanBeNull] List<DbSourceIndex> sourceIndices = null
        )
        {
            Directories = directories ?? new List<DbDirectory>();
            Arguments = arguments ?? new List<DbArgument>();
            Classes = classes ?? new List<DbClass>();
            ClassReferences = classReferences ?? new List<DbClassReference>();
            Fields = fields ?? new List<DbField>();
            Files = files ?? new List<DbFile>();
            Methods = methods ?? new List<DbMethod>();
            MethodReferences = methodReferences ?? new List<DbMethodReference>();
            Types = types ?? new List<DbType>();
            SourceIndices = sourceIndices ?? new List<DbSourceIndex>();
        }

        public static TableSet ReadAll(IDbConnection conn, ProgressTracker tracker = default)
        {
            tracker ??= ProgressTracker.Dummy;
            ProgressStepper stepper = tracker.Steps(10);

            List<DbDirectory> directories = DbDirectory.ReadAll(conn);
            stepper.Step();
            List<DbArgument> arguments = DbArgument.ReadAll(conn);
            stepper.Step();
            List<DbClass> classes = DbClass.ReadAll(conn);
            stepper.Step();
            List<DbClassReference> classReferences = DbClassReference.ReadAll(conn);
            stepper.Step();
            List<DbField> fields = DbField.ReadAll(conn);
            stepper.Step();
            List<DbFile> files = DbFile.ReadAll(conn);
            stepper.Step();
            List<DbMethod> methods = DbMethod.ReadAll(conn);
            stepper.Step();
            List<DbMethodReference> methodReferences = DbMethodReference.ReadAll(conn);
            stepper.Step();
            List<DbType> types = DbType.ReadAll(conn);
            stepper.Step();
            List<DbSourceIndex> sourceIndices = DbSourceIndex.ReadAll(conn);
            stepper.Step();

            return new TableSet(
                directories: directories,
                arguments: arguments,
                classes: classes,
                classReferences: classReferences,
                fields: fields,
                files: files,
                methods: methods,
                methodReferences: methodReferences,
                types: types,
                sourceIndices: sourceIndices
            );
        }

        public static void CreateTables(IDbConnection conn)
        {
            conn.ExecuteNonQuery(@"PRAGMA foreign_keys = ON;");
            conn.ExecuteNonQuery(DbDirectory.CreateTable);
            conn.ExecuteNonQuery(DbFile.CreateTable);
            conn.ExecuteNonQuery(DbClass.CreateTable);
            conn.ExecuteNonQuery(DbType.CreateTable);
            conn.ExecuteNonQuery(DbMethod.CreateTable);
            conn.ExecuteNonQuery(DbArgument.CreateTable);
            conn.ExecuteNonQuery(DbField.CreateTable);
            conn.ExecuteNonQuery(DbMethodReference.CreateTable);
            conn.ExecuteNonQuery(DbClassReference.CreateTable);
            conn.ExecuteNonQuery(DbSourceIndex.CreateTable);
        }

        public static void Save(TableSet tableSet, IDbConnection conn)
        {
            DbDirectory.SaveAll(tableSet.Directories, conn);
            DbFile.SaveAll(tableSet.Files, conn);
            DbClass.SaveAll(tableSet.Classes, conn);
            DbType.SaveAll(tableSet.Types, conn);
            DbField.SaveAll(tableSet.Fields, conn);
            DbMethod.SaveAll(tableSet.Methods, conn);
            DbArgument.SaveAll(tableSet.Arguments, conn);
            DbMethodReference.SaveAll(tableSet.MethodReferences, conn);
            DbClassReference.SaveAll(tableSet.ClassReferences, conn);
            DbSourceIndex.SaveAll(tableSet.SourceIndices, conn);
        }
    }
}