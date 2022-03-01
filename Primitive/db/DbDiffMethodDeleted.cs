using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
	[PublicAPI]
    public class DbDiffMethodDeleted
    {
        public readonly int MethodId;
        public readonly int BranchId;

        public DbDiffMethodDeleted(int methodId, int branchId)
        {
            MethodId = methodId;
            BranchId = branchId;
        }
        
        public const string CreateTable =
            @"CREATE TABLE diff_methods_deleted (
                          method_id INTEGER NOT NULL,
						  branch_id INTEGER NOT NULL,
						  FOREIGN KEY(method_id) REFERENCES methods(id) ON UPDATE CASCADE,
						  FOREIGN KEY(branch_id) REFERENCES branches(id) ON UPDATE CASCADE)";
        
        
        public static void SaveAll(IEnumerable<DbDiffMethodDeleted> methods, IDbConnection conn)
        {
	        IDbCommand cmd = conn.CreateCommand();
	        IDbTransaction transaction = conn.BeginTransaction();
	        cmd.CommandText =
		        @"INSERT INTO diff_methods_deleted (
						method_id,
						branch_id
                      ) VALUES (
						@MethodId,
						@BranchId
                      )";

	        foreach (DbDiffMethodDeleted method in methods)
	        {
		        cmd.AddParameter(System.Data.DbType.Int32, "@MethodId", method.MethodId);
		        cmd.AddParameter(System.Data.DbType.Int32, "@BranchId", method.BranchId);
		        cmd.ExecuteNonQuery();
	        }

	        transaction.Commit();
	        transaction.Dispose();
	        cmd.Dispose();
        }

        public static List<DbDiffMethodDeleted> ReadAll(IDbConnection conn)
        {
	        const string query = @"
                    SELECT
						method_id,
						branch_id
                    FROM diff_methods_deleted
            ";

	        return conn.Execute(query).TransformRows(row => new DbDiffMethodDeleted(
		        methodId: row.GetInt32("method_id"),
		        branchId: row.GetInt32("branch_id")
	        ));
        }
    }
}