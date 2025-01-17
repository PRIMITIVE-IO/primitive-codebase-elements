using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    [PublicAPI]
    public class DbDiffFieldDeleted
    {
        public readonly int FieldId;
        public readonly int BranchId;

        public DbDiffFieldDeleted(int fieldId, int branchId)
        {
            FieldId = fieldId;
            BranchId = branchId;
        }
        
        public const string CreateTable = 
            @"CREATE TABLE diff_fields_deleted (
                          field_id INTEGER NOT NULL,
						  branch_id INT,
						  FOREIGN KEY(field_id) REFERENCES fields(id) ON UPDATE CASCADE,
						  FOREIGN KEY(branch_id) REFERENCES branches(id) ON UPDATE CASCADE)";
        
        public static void SaveAll(IEnumerable<DbDiffFieldDeleted> fields, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO diff_fields_deleted (
                        field_id,
                        branch_id
                      ) VALUES (
                        @FieldId,
                        @BranchId
                      )";

            foreach (DbDiffFieldDeleted field in fields)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@FieldId", field.FieldId);
                cmd.AddParameter(System.Data.DbType.Int32, "@BranchId", field.BranchId);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbDiffFieldDeleted> ReadAll(IDbConnection conn)
        {
            const string query = @"
                    SELECT
                        field_id,
                        branch_id
                    FROM diff_fields_deleted
            ";

            return conn.Execute(query).TransformRows(row => new DbDiffFieldDeleted(
                fieldId: row.GetInt32("field_id"),
                branchId: row.GetInt32("branch_id")
            ));
        }
        
    }
}