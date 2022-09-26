using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;
using static PrimitiveCodebaseElements.Primitive.db.util.SqLiteUtil;

namespace PrimitiveCodebaseElements.Primitive.db
{
    [PublicAPI]
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
            List<DbDirectory>? directories = null,
            List<DbArgument>? arguments = null,
            List<DbClass>? classes = null,
            List<DbClassReference>? classReferences = null,
            List<DbField>? fields = null,
            List<DbFile>? files = null,
            List<DbMethod>? methods = null,
            List<DbMethodReference>? methodReferences = null,
            List<DbType>? types = null,
            List<DbSourceIndex>? sourceIndices = null)
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

        public static TableSet ReadAllParallel(
            Func<IDbConnection> connProvider,
            ProgressTracker? tracker = default)
        {
            tracker ??= ProgressTracker.Dummy;
            ProgressStepper stepper = tracker.Steps(10);

            Task<List<DbDirectory>> directories = LoadAsync(DbDirectory.ReadAll, connProvider, stepper);
            Task<List<DbArgument>> arguments = LoadAsync(DbArgument.ReadAll, connProvider, stepper);
            Task<List<DbClass>> classes = LoadAsync(DbClass.ReadAll, connProvider, stepper);
            Task<List<DbClassReference>> classReferences = LoadAsync(DbClassReference.ReadAll, connProvider, stepper);
            Task<List<DbField>> fields = LoadAsync(DbField.ReadAll, connProvider, stepper);
            Task<List<DbFile>> files = LoadAsync(DbFile.ReadAll, connProvider, stepper);
            Task<List<DbMethod>> methods = LoadAsync(DbMethod.ReadAll, connProvider, stepper);

            Task<List<DbMethodReference>> methodReferences = LoadAsync(
                DbMethodReference.ReadAll,
                connProvider,
                stepper
            );

            Task<List<DbType>> types = LoadAsync(DbType.ReadAll, connProvider, stepper);
            Task<List<DbSourceIndex>> sourceIndices = LoadAsync(DbSourceIndex.ReadAll, connProvider, stepper);

            return new TableSet(
                directories: directories.Result,
                arguments: arguments.Result,
                classes: classes.Result,
                classReferences: classReferences.Result,
                fields: fields.Result,
                files: files.Result,
                methods: methods.Result,
                methodReferences: methodReferences.Result,
                types: types.Result,
                sourceIndices: sourceIndices.Result
            );
        }


        public static TableSet ReadAll(IDbConnection conn, ProgressTracker? tracker = default)
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