using System.Collections.Generic;
using System.Data;

using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    
    public class DbDiffFileDeleted
    {
        public readonly int Id;
        public readonly int BranchId;
        public readonly int FileId;

        public DbDiffFileDeleted(int id, int branchId, int fileId)
        {
            Id = id;
            BranchId = branchId;
            FileId = fileId;
        }

        public const string CreateTable =
            @"CREATE TABLE diff_files_deleted (
                         id INTEGER PRIMARY KEY ASC,
                         branch_id INTEGER NOT NULL,
                         file_id INTEGER NOT NULL,
                         FOREIGN KEY(branch_id) REFERENCES branches(id) ON UPDATE CASCADE,
                         FOREIGN KEY(file_id) REFERENCES files(id) ON UPDATE CASCADE)";
        
        public static void SaveAll(IEnumerable<DbDiffFileDeleted> directories, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO diff_files_deleted (
                        id,
                        branch_id,
                        file_id
                      ) VALUES (
                        @Id,
                        @BranchId,
                        @FileId
                      )";

            foreach (DbDiffFileDeleted file in directories)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", file.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@BranchId", file.BranchId);
                cmd.AddParameter(System.Data.DbType.Int32, "@FileId", file.FileId);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbDiffFileDeleted> ReadAll(IDbConnection conn)
        {
            const string query = @"
                    SELECT
                        id,
                        branch_id,
                        file_id
                    FROM diff_files_deleted
            ";

            return conn.Execute(query).TransformRows(row => new DbDiffFileDeleted(
                id: row.GetInt32("id"),
                branchId: row.GetInt32("branch_id"),
                fileId: row.GetInt32("file_id")
            ));
        }
    }
}