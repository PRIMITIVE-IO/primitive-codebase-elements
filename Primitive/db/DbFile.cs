using System.Collections.Generic;
using System.Data;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    public class DbFile
    {
        public readonly int Id;
        public readonly int DirectoryId;
        public readonly string Name;
        public readonly string Path;
        public readonly string SourceText;
        public readonly int Language;


        public DbFile(int id, int directoryId, string name, string path, string sourceText, int language)
        {
            Id = id;
            DirectoryId = directoryId;
            Name = name;
            Path = path;
            SourceText = sourceText;
            Language = language;
        }

        public static void SaveAll(IEnumerable<DbFile> files, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO files ( 
                          id,
                          directory_id, 
                          name, 
                          path, 
                          source_text, 
                          language 
                      ) VALUES ( 
                          @Id,
                          @DirectoryId, 
                          @Name, 
                          @Path, 
                          @SourceText, 
                          @Language)";

            foreach (DbFile file in files)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", file.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@DirectoryId", file.DirectoryId);
                cmd.AddParameter(System.Data.DbType.String, "@Name", file.Name);
                cmd.AddParameter(System.Data.DbType.String, "@Path", file.Path);
                cmd.AddParameter(System.Data.DbType.String, "@SourceText", file.SourceText);
                cmd.AddParameter(System.Data.DbType.Int32, "@Language", file.Language);

                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static readonly string CreateTable = @"
            CREATE TABLE files ( 
                id INTEGER PRIMARY KEY ASC, 
                directory_id INTEGER NOT NULL, 
                name TEXT NOT NULL, 
                path TEXT NOT NULL, 
                source_text TEXT, 
                language INTEGER
            )";

        public static List<DbFile> ReadAll(IDbConnection conn)
        {
            string query = @"
                    SELECT
                          id,
                          directory_id, 
                          name, 
                          path, 
                          source_text, 
                          language 
                    FROM files
            ";

            return conn.Execute(query).TransformRows(row => new DbFile(
                id: row.GetInt32("id"),
                directoryId: row.GetInt32("directory_id"),
                name: row.GetString("name"),
                path: row.GetString("path"),
                sourceText: row.GetString("source_text"),
                language: row.GetInt32("language")
            ));
        }
    }
}