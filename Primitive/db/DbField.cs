using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    [PublicAPI]
    public class DbField
    {
        public readonly int Id, ParentFileId, TypeId, AccessFlags, Language;
        public readonly int? ParentClassId;
        public readonly string Name;

        public DbField(int id, int? parentClassId, int parentFileId, string name, int typeId, int accessFlags, int language)
        {
            Id = id;
            ParentClassId = parentClassId;
            ParentFileId = parentFileId;
            Name = name;
            TypeId = typeId;
            AccessFlags = accessFlags;
            Language = language;
        }

        // foreign keys are broken since a field and method can have either a class parent or a file parent
        public const string CreateTable = @"
            CREATE TABLE fields ( 
                id INTEGER PRIMARY KEY ASC, 
                parent_class_id INTEGER, 
                parent_file_id INTEGER NOT NULL, 
                name TEXT NOT NULL, 
                type_id INTEGER NOT NULL, 
                access_flags INTEGER NOT NULL,
                language INTEGER,
                FOREIGN KEY(parent_class_id) REFERENCES classes(id) ON UPDATE CASCADE, 
                FOREIGN KEY(parent_file_id) REFERENCES files(id) ON UPDATE CASCADE, 
                FOREIGN KEY(type_id) REFERENCES types(id) ON UPDATE CASCADE
            )";

        public static void SaveAll(IEnumerable<DbField> fields, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();

            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO fields ( 
                          id,
                          parent_class_id, 
                          parent_file_id, 
                          name, 
                          type_id, 
                          access_flags,
                          language 
                      ) VALUES ( 
                          @Id,
                          @ParentClassId, 
                          @ParentFileId, 
                          @Name, 
                          @TypeId, 
                          @AccessFlags,
                          @Language)";

            foreach (DbField field in fields)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", field.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentClassId", field.ParentClassId);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentFileId", field.ParentFileId);
                cmd.AddParameter(System.Data.DbType.String, "@Name", field.Name);
                cmd.AddParameter(System.Data.DbType.Int32, "@TypeId", field.TypeId);
                cmd.AddParameter(System.Data.DbType.UInt32, "@AccessFlags", field.AccessFlags);
                cmd.AddParameter(System.Data.DbType.Int32, "@Language", field.Language);

                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbField> ReadAll(IDbConnection conn)
        {
            const string query = @"
                    SELECT
                          id,
                          parent_class_id, 
                          parent_file_id, 
                          name, 
                          type_id, 
                          access_flags, 
                          language 
                    FROM fields
                   ";

            return conn.Execute(query).TransformRows(row => new DbField(
                id: row.GetInt32("id"),
                parentClassId: row.GetIntOrNull("parent_class_id"),
                parentFileId: row.GetInt32("parent_file_id"),
                name: row.GetString("name"),
                typeId: row.GetInt32("type_id"),
                accessFlags: row.GetInt32("access_flags"),
                language: row.GetInt32("language")
            ));
        }
    }
}