using System.Collections.Generic;
using System.Data;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    public class DbDiffFileModified
    {
        public readonly int Id;
        public readonly int BranchId;
        public readonly int FileId;
        public readonly string ChangeContent;
        public readonly string OldName;

        public DbDiffFileModified(int id, int branchId, int fileId, string changeContent, string oldName)
        {
            Id = id;
            BranchId = branchId;
            FileId = fileId;
            ChangeContent = changeContent;
            OldName = oldName;
        }

        public const string CreateTable =
            @"CREATE TABLE diff_files_modified (
                         id INTEGER PRIMARY KEY ASC,
                         branch_id INTEGER NOT NULL,
                         file_id INTEGER NOT NULL,
                         change_content TEXT NULL,
                         old_name TEXT NULL,
                         FOREIGN KEY(branch_id) REFERENCES branches(id) ON UPDATE CASCADE,
                         FOREIGN KEY(file_id) REFERENCES files(id) ON UPDATE CASCADE)";


        public static void SaveAll(IEnumerable<DbDiffFileModified> directories, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO diff_files_modified (
                        id,
                        branch_id,
                        file_id,
                        change_content,
                        old_name
                      ) VALUES (
                        @Id,
                        @BranchId,
                        @FileId,
                        @ChangeContent,
                        @OldName
                      )";

            foreach (DbDiffFileModified file in directories)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", file.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@BranchId", file.BranchId);
                cmd.AddParameter(System.Data.DbType.Int32, "@FileId", file.FileId);
                cmd.AddParameter(System.Data.DbType.String, "@ChangeContent", file.ChangeContent);
                cmd.AddParameter(System.Data.DbType.String, "@OldName", file.OldName);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbDiffFileModified> ReadAll(IDbConnection conn)
        {
            string query = @"
                    SELECT
                        id,
                        branch_id,
                        file_id,
                        change_content,
                        old_name
                    FROM diff_files_modified
            ";

            return conn.Execute(query).TransformRows(row => new DbDiffFileModified(
                id: row.GetInt32("id"),
                branchId: row.GetInt32("branch_id"),
                fileId: row.GetInt32("file_id"),
                changeContent: row.GetString("change_content"),
                oldName: row.GetString("old_name")
            ));
        }
    }
}