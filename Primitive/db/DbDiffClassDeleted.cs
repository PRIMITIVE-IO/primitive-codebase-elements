using System.Collections.Generic;
using System.Data;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    public class DbDiffClassDeleted
    {
        public readonly int ClassId;
        public readonly int BranchId;

        public DbDiffClassDeleted(int classId, int branchId)
        {
            ClassId = classId;
            BranchId = branchId;
        }
        
        public static readonly string CreateTable =
            @"CREATE TABLE diff_classes_deleted (
                          class_id INTEGER NOT NULL,
						  branch_id INTEGER NOT NULL,
						  FOREIGN KEY(class_id) REFERENCES classes(id) ON UPDATE CASCADE,
						  FOREIGN KEY(branch_id) REFERENCES branches(id) ON UPDATE CASCADE)";
        
        
        public static void SaveAll(IEnumerable<DbDiffClassDeleted> directories, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO diff_classes_deleted (
                        class_id,
                        branch_id
                      ) VALUES (
                        @ClassId,
                        @BranchId
                      )";

            foreach (DbDiffClassDeleted cls in directories)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@ClassId", cls.ClassId);
                cmd.AddParameter(System.Data.DbType.Int32, "@BranchId", cls.BranchId);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbDiffClassDeleted> ReadAll(IDbConnection conn)
        {
            string query = @"
                    SELECT
                        class_id,
                        branch_id
                    FROM diff_classes_deleted
            ";

            return conn.Execute(query).TransformRows(row => new DbDiffClassDeleted(
                classId: row.GetInt32("class_id"),
                branchId: row.GetInt32("branch_id")
            ));
        }
    }
}