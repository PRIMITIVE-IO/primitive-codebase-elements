using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    [PublicAPI]
    public class DbModifiedAndDeletedFiles
    {
        public readonly int Id;
        public readonly int BranchId;
        public readonly int FileId;
        public readonly int ChangeType;
        public readonly string ChangeContent;
        public readonly string OldName;

        public DbModifiedAndDeletedFiles(int id, int branchId, int fileId, int changeType, string changeContent,
            string oldName)
        {
            Id = id;
            BranchId = branchId;
            FileId = fileId;
            ChangeType = changeType;
            ChangeContent = changeContent;
            OldName = oldName;
        }

        public const string CreateTable = @"
            CREATE TABLE modified_and_deleted_files (
                         id INTEGER PRIMARY KEY ASC,
                         branch_id INTEGER NOT NULL,
                         file_id INTEGER NOT NULL,
                         change_type INTEGER NOT NULL,
                         change_content TEXT NULL,
                         old_name TEXT NULL,
                         FOREIGN KEY(branch_id) REFERENCES branches(id) ON UPDATE CASCADE,
                         FOREIGN KEY(file_id) REFERENCES files(id) ON UPDATE CASCADE)
        ";

        public static void SaveAll(IEnumerable<DbModifiedAndDeletedFiles> branches, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO modified_and_deleted_files ( 
                          id,
                          branch_id,
                          file_id,
                          change_type,
                          change_content,
                          old_name
                      ) VALUES ( 
                          @Id,
                          @Branch_id,
                          @File_id,
                          @Change_type,
                          @Change_content,
                          @Old_name
                      )";

            foreach (DbModifiedAndDeletedFiles file in branches)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", file.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@BranchId", file.BranchId);
                cmd.AddParameter(System.Data.DbType.Int32, "@FileId", file.FileId);
                cmd.AddParameter(System.Data.DbType.Int32, "@ChangeType", file.ChangeType);
                cmd.AddParameter(System.Data.DbType.String, "@ChangeContent", file.ChangeContent);
                cmd.AddParameter(System.Data.DbType.String, "@OldName", file.OldName);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbModifiedAndDeletedFiles> ReadAll(IDbConnection conn)
        {
            const string query = @"
                    SELECT
                          id,
                          branch_id,
                          file_id,
                          change_type,
                          change_content,
                          old_name
                    FROM modified_and_deleted_files
            ";

            return conn.Execute(query).TransformRows(row => new DbModifiedAndDeletedFiles(
                id: row.GetInt32("id"),
                branchId: row.GetInt32("branch_id"),
                fileId: row.GetInt32("file_id"),
                changeType: row.GetInt32("change_type"),
                changeContent: row.GetString("change_content"),
                oldName: row.GetString("old_name")
            ));
        }
    }
}