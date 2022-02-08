using System.Collections.Generic;
using System.Data;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    public class DbDiffDirectoryAdded
    {
        public readonly int Id;
        public readonly string Fqn;
        public readonly int BranchId;

        public DbDiffDirectoryAdded(int id, string fqn, int branchId)
        {
            Id = id;
            Fqn = fqn;
            BranchId = branchId;
        }

        public static string CreateTable = @"
            CREATE TABLE diff_directories_added ( 
                         id INTEGER PRIMARY KEY ASC, 
                         fqn TEXT NOT NULL, 
                         branch_id INTEGER NOT NULL,
                         FOREIGN KEY(branch_id) REFERENCES diff_branches(id) ON UPDATE CASCADE)
                         ";
        
        
        public static void SaveAll(IEnumerable<DbDiffDirectoryAdded> directories, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO diff_directories_added (
                          id,
                          fqn,
                          branch_id
                      ) VALUES (
                          @Id,
                          @FQN,
                          @BranchId,
                          @PositionY
                      )";

            foreach (DbDiffDirectoryAdded dir in directories)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", dir.Id);
                cmd.AddParameter(System.Data.DbType.String, "@FQN", dir.Fqn);
                cmd.AddParameter(System.Data.DbType.Int32, "@BranchId", dir.BranchId);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbDiffDirectoryAdded> ReadAll(IDbConnection conn)
        {
            string query = @"
                    SELECT
                          id,
                          fqn,
                          branch_id
                    FROM diff_directories_added
            ";

            return conn.Execute(query).TransformRows(row => new DbDiffDirectoryAdded(
                id: row.GetInt32("id"),
                fqn: row.GetString("fqn"),
                branchId: row.GetInt32("branch_id")
            ));
        }
    }
}