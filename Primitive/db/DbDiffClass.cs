using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    [PublicAPI]
    public class DbDiffClass
    {
        public readonly int Id, AccessFlags, Language, IsTestClass, BranchId;
        public readonly int? ParentFileId, ParentFileIdDiff, ParentClassId, ParentClassIdDiff;
        public readonly string Fqn, HeaderSource;

        public DbDiffClass(
            int id, int? parentClassId, int? parentFileId, int? parentClassIdDiff, int? parentFileIdDiff,
            string fqn, int accessFlags,
            string headerSource, int language,
            int isTestClass, int branchId)
        {
            Id = id;
            ParentClassId = parentClassId;
            ParentFileId = parentFileId;
            ParentFileIdDiff = parentFileIdDiff;
            Fqn = fqn;
            AccessFlags = accessFlags;
            HeaderSource = headerSource;
            Language = language;
            IsTestClass = isTestClass;
            BranchId = branchId;
        }

        public const string CreateTable = @"CREATE TABLE diff_classes (
                          id INTEGER PRIMARY KEY ASC,
                          parent_class_id INTEGER NULL,
                          parent_class_id_diff INTEGER NULL,
                          parent_file_id INTEGER NULL,
                          parent_file_id_diff INTEGER NULL,
                          fqn TEXT NOT NULL,
                          access_flags INTEGER NOT NULL,
                          header_source TEXT,
                          language INTEGER,
                          is_test_class INTEGER NOT NULL,
						  branch_id INTEGER NOT NULL,
                          FOREIGN KEY(parent_file_id) REFERENCES files(id) ON UPDATE CASCADE,
						  FOREIGN KEY(parent_file_id_diff) REFERENCES diff_files_added(id) ON UPDATE CASCADE,
						  FOREIGN KEY(branch_id) REFERENCES branches(id) ON UPDATE CASCADE)";

        public static void SaveAll(IEnumerable<DbDiffClass> dbDiffClasses, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO diff_classes (
                        id,
                        parent_class_id,
                        parent_class_id_diff,
                        parent_file_id,
                        parent_file_id_diff,
                        fqn,
                        access_flags,
                        header_source,
                        language,
                        is_test_class,
                        branch_id
                      ) VALUES (
                        @Id,
                        @ParentClassId,
                        @ParentClassIdDiff,
                        @ParentFileId,
                        @ParentFileIdDiff,
                        @Fqn,
                        @AccessFlags,
                        @HeaderSource,
                        @Language,
                        @IsTestClass,
                        @BranchId
                      )";

            foreach (DbDiffClass dbDiffClass in dbDiffClasses)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", dbDiffClass.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentClassId", dbDiffClass.ParentClassId);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentClassIdDiff", dbDiffClass.ParentClassIdDiff);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentFileId", dbDiffClass.ParentFileId);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentFileIdDiff", dbDiffClass.ParentFileIdDiff);
                cmd.AddParameter(System.Data.DbType.String, "@Fqn", dbDiffClass.Fqn);
                cmd.AddParameter(System.Data.DbType.Int32, "@AccessFlags", dbDiffClass.AccessFlags);
                cmd.AddParameter(System.Data.DbType.String, "@HeaderSource", dbDiffClass.HeaderSource);
                cmd.AddParameter(System.Data.DbType.Int32, "@Language", dbDiffClass.Language);
                cmd.AddParameter(System.Data.DbType.Int32, "@IsTestClass", dbDiffClass.IsTestClass);
                cmd.AddParameter(System.Data.DbType.Int32, "@BranchId", dbDiffClass.BranchId);
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
                        parent_class_id,
                        parent_class_id_diff,
                        parent_file_id,
                        parent_file_id_diff,
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
                parentClassId: row.GetIntOrNull("parent_class_id"),
                parentClassIdDiff: row.GetIntOrNull("parent_class_id_diff"),
                parentFileId: row.GetIntOrNull("parent_file_id"),
                parentFileIdDiff: row.GetIntOrNull("parent_file_id_diff"),
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