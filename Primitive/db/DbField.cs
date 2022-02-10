using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    [PublicAPI]
    public class DbField
    {
        public readonly int Id, ParentType, ParentId, TypeId, AccessFlags, Language;
        public readonly string Name;

        public DbField(int id, int parentType, int parentId, string name, int typeId, int accessFlags, int language)
        {
            Id = id;
            ParentType = parentType;
            ParentId = parentId;
            Name = name;
            TypeId = typeId;
            AccessFlags = accessFlags;
            Language = language;
        }

        public static readonly string CreateTable = @"
            CREATE TABLE fields ( 
                id INTEGER PRIMARY KEY ASC, 
                parent_type INTEGER NOT NULL, 
                parent_id INTEGER NOT NULL, 
                name TEXT NOT NULL, 
                type_id INTEGER NOT NULL, 
                access_flags INTEGER NOT NULL,
                language INTEGER, 
                FOREIGN KEY(parent_id) REFERENCES classes(id) ON UPDATE CASCADE, 
                FOREIGN KEY(type_id) REFERENCES types(id) ON UPDATE CASCADE
            )";

        public static void SaveAll(IEnumerable<DbField> fields, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();

            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO fields ( 
                          id,
                          parent_type, 
                          parent_id, 
                          name, 
                          type_id, 
                          access_flags,
                          language 
                      ) VALUES ( 
                          @Id,
                          2, 
                          @ParentId, 
                          @Name, 
                          @TypeId, 
                          @AccessFlags,
                          @Language)";

            foreach (DbField field in fields)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", field.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentId", field.ParentId);
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
                          parent_type, 
                          parent_id, 
                          name, 
                          type_id, 
                          access_flags, 
                          source_code, 
                          language 
                    FROM fields
                   ";

            return conn.Execute(query).TransformRows(row => new DbField(
                id: row.GetInt32("id "),
                parentType: row.GetInt32("parent_type"),
                parentId: row.GetInt32("parent_id"),
                name: row.GetString("name"),
                typeId: row.GetInt32("type_id"),
                accessFlags: row.GetInt32("access_flags"),
                language: row.GetInt32("language")
            ));
        }
    }
}