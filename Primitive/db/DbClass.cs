using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    [PublicAPI]
    public class DbClass
    {
        public readonly int Id, ParentType, ParentId, AccessFlags, Language, IsTestClass;
        public readonly string Fqn;

        public DbClass(int id, int parentType, int parentId, string fqn, int accessFlags, int language, int isTestClass)
        {
            Id = id;
            ParentType = parentType;
            ParentId = parentId;
            Fqn = fqn;
            AccessFlags = accessFlags;
            Language = language;
            IsTestClass = isTestClass;
        }

        public const string CreateTable = @"CREATE TABLE classes ( 
                          id INTEGER PRIMARY KEY ASC, 
                          parent_type INTEGER NOT NULL, 
                          parent_id INTEGER NOT NULL, 
                          fqn TEXT NOT NULL, 
                          access_flags INTEGER NOT NULL,
                          language INTEGER, 
                          is_test_class INTEGER NOT NULL, 
                          FOREIGN KEY(parent_id) REFERENCES files(id) ON UPDATE CASCADE)";
        public static void SaveAll(IEnumerable<DbClass> classes, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO classes ( 
                          id,
                          parent_type, 
                          parent_id, 
                          fqn, 
                          access_flags, 
                          language, 
                          is_test_class 
                      ) VALUES (  
                          @Id,
                          @ParentType, 
                          @ParentId, 
                          @FQN, 
                          @AccessFlags, 
                          @Language, 
                          @IsTestClass)";

            foreach (DbClass cls in classes)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", cls.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentType", cls.ParentType);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentId", cls.ParentId);
                cmd.AddParameter(System.Data.DbType.String, "@FQN", cls.Fqn);
                cmd.AddParameter(System.Data.DbType.Int32, "@AccessFlags", cls.AccessFlags);
                cmd.AddParameter(System.Data.DbType.Int32, "@IsTestClass", cls.IsTestClass);
                cmd.AddParameter(System.Data.DbType.Int32, "@Language", cls.Language);

                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbClass> ReadAll(IDbConnection conn)
        {
            const string query = @"
                    SELECT
                          id,
                          parent_type, 
                          parent_id, 
                          fqn, 
                          access_flags, 
                          language, 
                          is_test_class 
                    FROM classes
                ";
            return conn.Execute(query).TransformRows(row => new DbClass(
                id: row.GetInt32("id"),
                parentType: row.GetInt32("parent_type"),
                parentId: row.GetInt32("parent_id"),
                fqn: row.GetString("fqn"),
                accessFlags: row.GetInt32("access_flags"),
                language: row.GetInt32("language"),
                isTestClass: row.GetInt32("is_test_class")
            ));
        }
    }
}