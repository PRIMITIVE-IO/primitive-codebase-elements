using System.Collections.Generic;
using System.Data;

using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    
    public class DbDiffFileAdded
    {
        public readonly int Id;
        public readonly int BranchId;
        public readonly string Path;
        public readonly int? DirectoryId;
        public readonly int? DirectoryIdDiff;
        public readonly string Content;
        public readonly int LanguageId;

        public DbDiffFileAdded(int id, int branchId, string path, int? directoryId, int? directoryIdDiff,
            string content, int languageId)
        {
            Id = id;
            BranchId = branchId;
            Path = path;
            DirectoryId = directoryId;
            DirectoryIdDiff = directoryIdDiff;
            Content = content;
            LanguageId = languageId;
        }

        public const string CreateTable =
            @"CREATE TABLE diff_files_added (
                         id INTEGER PRIMARY KEY ASC,
                         branch_id INTEGER NOT NULL,
                         path TEXT NOT NULL,
                         directory_id INTEGER NULL,
                         directory_id_diff INTEGER NULL,
                         content INTEGER NOT NULL,
                         language_id INTEGER NOT NULL,
                         FOREIGN KEY(branch_id) REFERENCES branches(id) ON UPDATE CASCADE,
                         FOREIGN KEY(directory_id_diff) REFERENCES diff_directories_added (id) ON UPDATE CASCADE,
                         FOREIGN KEY(directory_id) REFERENCES directories(id) ON UPDATE CASCADE)";
        
        public static void SaveAll(IEnumerable<DbDiffFileAdded> directories, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO diff_files_added (
                        id,
                        branch_id,
                        path,
                        directory_id,
                        directory_id_diff,
                        content,
                        language_id
                      ) VALUES (
                        @Id,
                        @BranchId,
                        @Path,
                        @DirectoryId,
                        @DirectoryIdDiff,
                        @Content,
                        @LanguageId
                      )";

            foreach (DbDiffFileAdded file in directories)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", file.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@BranchId", file.BranchId);
                cmd.AddParameter(System.Data.DbType.String, "@Path", file.Path);
                cmd.AddParameter(System.Data.DbType.Int32, "@DirectoryId", file.DirectoryId);
                cmd.AddParameter(System.Data.DbType.Int32, "@DirectoryIdDiff", file.DirectoryIdDiff);
                cmd.AddParameter(System.Data.DbType.String, "@Content", file.Content);
                cmd.AddParameter(System.Data.DbType.Int32, "@LanguageId", file.LanguageId);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbDiffFileAdded> ReadAll(IDbConnection conn)
        {
            const string query = @"
                    SELECT
                        id,
                        branch_id,
                        path,
                        directory_id,
                        directory_id_diff,
                        content,
                        language_id
                    FROM diff_files_added
            ";

            return conn.Execute(query).TransformRows(row => new DbDiffFileAdded(
                id: row.GetInt32("id"),
                branchId: row.GetInt32("branch_id"),
                path: row.GetString("path"),
                directoryId: row.GetIntOrNull("directory_id"),
                directoryIdDiff: row.GetIntOrNull("directory_id_diff"),
                content: row.GetString("content")       ,
                languageId: row.GetInt32("language_id")
            ));
        }
        
    }
}