using System.Collections.Generic;
using System.Data;
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
            List<DbDirectory> directories,
            List<DbArgument> arguments, List<DbClass> classes, List<DbClassReference> classReferences,
            List<DbField> fields, List<DbFile> files, List<DbMethod> methods, List<DbMethodReference> methodReferences,
            List<DbType> types, List<DbSourceIndex> sourceIndices)
        {
            Directories = directories;
            Arguments = arguments;
            Classes = classes;
            ClassReferences = classReferences;
            Fields = fields;
            Files = files;
            Methods = methods;
            MethodReferences = methodReferences;
            Types = types;
            SourceIndices = sourceIndices;
        }

        public static TableSet ReadAll(IDbConnection conn)
        {
            return new TableSet(
                directories: DbDirectory.ReadAll(conn),
                arguments: DbArgument.ReadAll(conn),
                classes: DbClass.ReadAll(conn),
                classReferences: DbClassReference.ReadAll(conn),
                fields: DbField.ReadAll(conn),
                files: DbFile.ReadAll(conn),
                methods: DbMethod.ReadAll(conn),
                methodReferences: DbMethodReference.ReadAll(conn),
                types: DbType.ReadAll(conn),
                sourceIndices: DbSourceIndex.ReadAll(conn)
            );
        }
        
        static void CreateTables(IDbConnection conn)
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

        static void Save(TableSet tableSet, IDbConnection conn)
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