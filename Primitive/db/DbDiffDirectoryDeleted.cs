using System.Collections.Generic;
using System.Data;

using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    
    public class DbDiffDirectoryDeleted
    {
        public readonly int DirectoryId;
        public readonly int BranchId;

        public DbDiffDirectoryDeleted(int directoryId, int branchId)
        {
            DirectoryId = directoryId;
            BranchId = branchId;
        }

        public const string CreateTable = @"
            CREATE TABLE diff_directories_deleted (
                          directory_id INTEGER NOT NULL,
						  branch_id INTEGER NOT NULL,
						  FOREIGN KEY(directory_id) REFERENCES directories(id) ON UPDATE CASCADE,
						  FOREIGN KEY(branch_id) REFERENCES branches(id) ON UPDATE CASCADE)";
        
        public static void SaveAll(IEnumerable<DbDiffDirectoryDeleted> directories, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO diff_directories_deleted (
                          directory_id,
                          branch_id
                      ) VALUES (
                          @DirectoryId,
                          @BranchId
                      )";

            foreach (DbDiffDirectoryDeleted dir in directories)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@DirectoryId", dir.DirectoryId);
                cmd.AddParameter(System.Data.DbType.Int32, "@BranchId", dir.BranchId);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbDiffDirectoryDeleted> ReadAll(IDbConnection conn)
        {
            const string query = @"
                    SELECT
                          directory_id,
                          branch_id
                    FROM diff_directories_deleted
            ";

            return conn.Execute(query).TransformRows(row => new DbDiffDirectoryDeleted(
                directoryId: row.GetInt32("directory_id"),
                branchId: row.GetInt32("branch_id")
            ));
        }
    }
}