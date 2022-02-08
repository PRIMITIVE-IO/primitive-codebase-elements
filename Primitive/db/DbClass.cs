using System.Collections.Generic;
using System.Data;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    public class DbClass
    {
        public readonly int Id;
        public readonly int ParentType;
        public readonly int ParentId;
        public readonly string Fqn;
        public readonly int AccessFlags;
        public readonly string HeaderSource;
        public readonly int Language;
        public readonly int IsTestClass;

        public DbClass(int id, int parentType, int parentId, string fqn, int accessFlags, string headerSource,
            int language, int isTestClass)
        {
            Id = id;
            ParentType = parentType;
            ParentId = parentId;
            Fqn = fqn;
            AccessFlags = accessFlags;
            HeaderSource = headerSource;
            Language = language;
            IsTestClass = isTestClass;
        }

        public static readonly string CreateTable = @"CREATE TABLE classes ( 
                          id INTEGER PRIMARY KEY ASC, 
                          parent_type INTEGER NOT NULL, 
                          parent_id INTEGER NOT NULL, 
                          fqn TEXT NOT NULL, 
                          access_flags INTEGER NOT NULL, 
                          header_source TEXT, 
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
                          header_source, 
                          language, 
                          is_test_class 
                      ) VALUES (  
                          @Id,
                          @ParentType, 
                          @ParentId, 
                          @FQN, 
                          @AccessFlags, 
                          @HeaderSource, 
                          @Language, 
                          @IsTestClass)";

            foreach (DbClass cls in classes)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", cls.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentType", cls.ParentType);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentId", cls.ParentId);
                cmd.AddParameter(System.Data.DbType.String, "@FQN", cls.Fqn);
                cmd.AddParameter(System.Data.DbType.Int32, "@AccessFlags", cls.AccessFlags);
                cmd.AddParameter(System.Data.DbType.String, "@HeaderSource", cls.HeaderSource);
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
            string query = @"
                    SELECT
                          id,
                          parent_type, 
                          parent_id, 
                          fqn, 
                          access_flags, 
                          header_source, 
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
                headerSource: row.GetString("header_source"),
                language: row.GetInt32("language"),
                isTestClass: row.GetInt32("is_test_class")
            ));
        }
    }
}