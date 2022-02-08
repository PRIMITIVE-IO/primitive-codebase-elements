using System.Collections.Generic;
using System.Data;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    public class DbDiffClassModifiedProperty
    {
        public readonly int OriginalClassId;
        public readonly int AccessFlags;
        public readonly string HeaderSource;
        public readonly int BranchId;

        public DbDiffClassModifiedProperty(int originalClassId, int accessFlags, string headerSource, int branchId)
        {
            OriginalClassId = originalClassId;
            AccessFlags = accessFlags;
            HeaderSource = headerSource;
            BranchId = branchId;
        }
        public static readonly string CreateTable =
            @"CREATE TABLE diff_class_modified_properties (
                          original_class_id INTEGER NOT NULL,
                          access_flags INTEGER NOT NULL,
                          header_source TEXT,
						  branch_id INT NOT NULL,
                          FOREIGN KEY(original_class_id) REFERENCES classes(id) ON UPDATE CASCADE,
						  FOREIGN KEY(branch_id) REFERENCES diff_branches(id) ON UPDATE CASCADE)";
        
        
        public static void SaveAll(IEnumerable<DbDiffClassModifiedProperty> props, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO diff_class_modified_properties (
                        original_class_id,
                        access_flags,
                        header_source,
                        branch_id
                      ) VALUES (
                        @OriginalClassId,
                        @AccessFlags,
                        @HeaderSource,
                        @BranchId
                      )";

            foreach (DbDiffClassModifiedProperty prop in props)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@OriginalClassId", prop.OriginalClassId);
                cmd.AddParameter(System.Data.DbType.Int32, "@AccessFlags", prop.AccessFlags);
                cmd.AddParameter(System.Data.DbType.String, "@HeaderSource", prop.HeaderSource);
                cmd.AddParameter(System.Data.DbType.Int32, "@BranchId", prop.BranchId);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbDiffClassModifiedProperty> ReadAll(IDbConnection conn)
        {
            string query = @"
                    SELECT
                        original_class_id,
                        access_flags,
                        header_source,
                        branch_id
                    FROM diff_class_modified_properties
            ";

            return conn.Execute(query).TransformRows(row => new DbDiffClassModifiedProperty(
                originalClassId: row.GetInt32("original_class_id"),
                accessFlags: row.GetInt32("access_flags"),
                headerSource: row.GetString("header_source"),
                branchId: row.GetInt32("branch_id")
            ));
        }
    }
}