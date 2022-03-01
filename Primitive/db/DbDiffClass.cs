using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    [PublicAPI]
    public class DbDiffClass
    {
        public readonly int Id;
        public readonly int ParentType;
        public readonly int? ParentId;
        public readonly int? ParentIdDiff;
        public readonly string Fqn;
        public readonly int AccessFlags;
        public readonly string HeaderSource;
        public readonly int Language;
        public readonly int IsTestClass;
        public readonly int BranchId;

        public DbDiffClass(int id, int parentType, int? parentId, int? parentIdDiff, string fqn, int accessFlags,
            string headerSource, int language, int isTestClass, int branchId)
        {
            Id = id;
            ParentType = parentType;
            ParentId = parentId;
            ParentIdDiff = parentIdDiff;
            Fqn = fqn;
            AccessFlags = accessFlags;
            HeaderSource = headerSource;
            Language = language;
            IsTestClass = isTestClass;
            BranchId = branchId;
        }

        public const string CreateTable = @"CREATE TABLE diff_classes (
                          id INTEGER PRIMARY KEY ASC,
                          parent_type INTEGER NOT NULL,
                          parent_id INTEGER NULL,
                          parent_id_diff INTEGER NULL,
                          fqn TEXT NOT NULL,
                          access_flags INTEGER NOT NULL,
                          header_source TEXT,
                          language INTEGER,
                          is_test_class INTEGER NOT NULL,
						  branch_id INTEGER NOT NULL,
                          FOREIGN KEY(parent_id) REFERENCES files(id) ON UPDATE CASCADE,
						  FOREIGN KEY(parent_id_diff) REFERENCES diff_files_added(id) ON UPDATE CASCADE,
						  FOREIGN KEY(branch_id) REFERENCES branches(id) ON UPDATE CASCADE)";


        public static void SaveAll(IEnumerable<DbDiffClass> directories, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO diff_classes (
                        id,
                        parent_type,
                        parent_id,
                        parent_id_diff,
                        fqn,
                        access_flags,
                        header_source,
                        language,
                        is_test_class,
                        branch_id
                      ) VALUES (
                        @Id,
                        @ParentType,
                        @ParentId,
                        @ParentIdDiff,
                        @Fqn,
                        @AccessFlags,
                        @HeaderSource,
                        @Language,
                        @IsTestClass,
                        @BranchId
                      )";

            foreach (DbDiffClass dir in directories)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", dir.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentType", dir.ParentType);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentId", dir.ParentId);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentIdDiff", dir.ParentIdDiff);
                cmd.AddParameter(System.Data.DbType.String, "@Fqn", dir.Fqn);
                cmd.AddParameter(System.Data.DbType.Int32, "@AccessFlags", dir.AccessFlags);
                cmd.AddParameter(System.Data.DbType.String, "@HeaderSource", dir.HeaderSource);
                cmd.AddParameter(System.Data.DbType.Int32, "@Language", dir.Language);
                cmd.AddParameter(System.Data.DbType.Int32, "@IsTestClass", dir.IsTestClass);
                cmd.AddParameter(System.Data.DbType.Int32, "@BranchId", dir.BranchId);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbDiffClass> ReadAll(IDbConnection conn)
        {
            const string query = @"
                    SELECT
                        id,
                        parent_type,
                        parent_id,
                        parent_id_diff,
                        fqn,
                        access_flags,
                        header_source,
                        language,
                        is_test_class,
                        branch_id
                    FROM diff_classes
            ";

            return conn.Execute(query).TransformRows(row => new DbDiffClass(
                id: row.GetInt32("id"),
                parentType: row.GetInt32("parent_type"),
                parentId: row.GetIntOrNull("parent_id"),
                parentIdDiff: row.GetIntOrNull("parent_id_diff"),
                fqn: row.GetString("fqn"),
                accessFlags: row.GetInt32("access_flags"),
                headerSource: row.GetString("header_source"),
                language: row.GetInt32("language"),
                isTestClass: row.GetInt32("is_test_class"),
                branchId: row.GetInt32("branch_id")
            ));
        }
    }
}