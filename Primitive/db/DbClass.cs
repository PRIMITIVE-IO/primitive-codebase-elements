using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    [PublicAPI]
    public class DbClass
    {
        public readonly int Id, ParentFileId, AccessFlags, IsTestClass;
        public readonly int? ParentClassId;
        public readonly string Fqn;

        public DbClass(int id, int? parentClassId, int parentFileId, string fqn, int accessFlags, int isTestClass)
        {
            Id = id;
            ParentClassId = parentClassId;
            ParentFileId = parentFileId;
            Fqn = fqn;
            AccessFlags = accessFlags;
            IsTestClass = isTestClass;
        }

        public const string CreateTable = @"CREATE TABLE classes ( 
                          id INTEGER PRIMARY KEY ASC, 
                          parent_class_id INTEGER, 
                          parent_file_id INTEGER NOT NULL, 
                          fqn TEXT NOT NULL, 
                          access_flags INTEGER NOT NULL,
                          is_test_class INTEGER NOT NULL, 
                          FOREIGN KEY(parent_class_id) REFERENCES classes(id) ON UPDATE CASCADE
                          FOREIGN KEY(parent_file_id) REFERENCES files(id) ON UPDATE CASCADE)";
        public static void SaveAll(IEnumerable<DbClass> classes, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO classes ( 
                          id,
                          parent_class_id, 
                          parent_file_id, 
                          fqn, 
                          access_flags, 
                          is_test_class 
                      ) VALUES (  
                          @Id,
                          @ParentClassId, 
                          @ParentFileId, 
                          @FQN, 
                          @AccessFlags, 
                          @IsTestClass)";

            foreach (DbClass cls in classes)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", cls.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentClassId", cls.ParentClassId);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentFileId", cls.ParentFileId);
                cmd.AddParameter(System.Data.DbType.String, "@FQN", cls.Fqn);
                cmd.AddParameter(System.Data.DbType.Int32, "@AccessFlags", cls.AccessFlags);
                cmd.AddParameter(System.Data.DbType.Int32, "@IsTestClass", cls.IsTestClass);

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
                          parent_class_id, 
                          parent_file_id, 
                          fqn, 
                          access_flags, 
                          is_test_class 
                    FROM classes
                ";
            return conn.Execute(query).TransformRows(row => new DbClass(
                id: row.GetInt32("id"),
                parentClassId: row.GetIntOrNull("parent_class_id"),
                parentFileId: row.GetInt32("parent_file_id"),
                fqn: row.GetString("fqn"),
                accessFlags: row.GetInt32("access_flags"),
                isTestClass: row.GetInt32("is_test_class")
            ));
        }
    }
}