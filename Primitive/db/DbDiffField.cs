using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    [PublicAPI]
    public class DbDiffField
    {
        public readonly int Id, TypeId, AccessFlags, BranchId;
        public readonly int? ParentFileId, ParentClassIdDiff, OriginalFieldId, ParentClassId, ParentFileIdDiff;
        public readonly string Name, SourceCode;

        public DbDiffField(
            int id, int? parentClassId, int? parentFileId, int? parentClassIdDiff, int? parentFileIdDiff,
            string name,
            int typeId,
            int accessFlags,
            string sourceCode,
            int? originalFieldId, int branchId)
        {
            Id = id;
            ParentClassId = parentClassId;
            ParentFileId = parentFileId;
            ParentClassIdDiff = parentClassIdDiff;
            ParentFileIdDiff = parentFileIdDiff;
            Name = name;
            TypeId = typeId;
            AccessFlags = accessFlags;
            SourceCode = sourceCode;
            OriginalFieldId = originalFieldId;
            BranchId = branchId;
        }

        public const string CreateTable =
            @"CREATE TABLE diff_fields (
                         id INTEGER PRIMARY KEY ASC,
                         parent_class_id INTEGER NULL,
                         parent_class_id_diff INTEGER NULL,
                         parent_file_id INTEGER NULL,
                         parent_file_id_diff INTEGER NULL,
                         name TEXT NOT NULL,
                         type_id INTEGER NOT NULL,
                         access_flags INTEGER NOT NULL,
                         source_code TEXT,
						 original_field_id INT NULL,
						 branch_id INT,
						 FOREIGN KEY(branch_id) REFERENCES branches(id) ON UPDATE CASCADE,
                         FOREIGN KEY(parent_class_id) REFERENCES classes(id) ON UPDATE CASCADE,
						 FOREIGN KEY(parent_class_id_diff) REFERENCES diff_classes(id) ON UPDATE CASCADE,
						 FOREIGN KEY(original_field_id) REFERENCES fields(id) ON UPDATE CASCADE,
                         FOREIGN KEY(type_id) REFERENCES types(id) ON UPDATE CASCADE)";

        public static void SaveAll(IEnumerable<DbDiffField> dbDiffFields, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO diff_fields (
                        id,
                        parent_class_id,
                        parent_class_id_diff,
                        parent_file_id,
                        parent_file_id_diff,
                        name,
                        type_id,
                        access_flags,
                        source_code,
                        original_field_id,
                        branch_id
                      ) VALUES (
                        @Id,
                        @ParentClassId,
                        @ParentClassIdDiff,
                        @ParentFileId,
                        @ParentFileIdDiff,
                        @Name,
                        @TypeId,
                        @AccessFlags,
                        @SourceCode,
                        @OriginalFieldId,
                        @BranchId
                      )";

            foreach (DbDiffField dbDiffField in dbDiffFields)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", dbDiffField.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentClassId", dbDiffField.ParentClassId);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentFileId", dbDiffField.ParentFileId);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentClassIdDiff", dbDiffField.ParentClassIdDiff);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentFileIdDiff", dbDiffField.ParentFileIdDiff);
                cmd.AddParameter(System.Data.DbType.String, "@Name", dbDiffField.Name);
                cmd.AddParameter(System.Data.DbType.Int32, "@TypeId", dbDiffField.TypeId);
                cmd.AddParameter(System.Data.DbType.Int32, "@AccessFlags", dbDiffField.AccessFlags);
                cmd.AddParameter(System.Data.DbType.String, "@SourceCode", dbDiffField.SourceCode);
                cmd.AddParameter(System.Data.DbType.Int32, "@OriginalFieldId", dbDiffField.OriginalFieldId);
                cmd.AddParameter(System.Data.DbType.Int32, "@BranchId", dbDiffField.BranchId);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbDiffField> ReadAll(IDbConnection conn)
        {
            const string query = @"
                    SELECT
                        id,
                        parent_class_id,
                        parent_class_id_diff,
                        parent_file_id,
                        parent_file_id_diff,
                        name,
                        type_id,
                        access_flags,
                        source_code,
                        original_field_id,
                        branch_id
                    FROM diff_fields
            ";

            return conn.Execute(query).TransformRows(row => new DbDiffField(
                id: row.GetInt32("id"),
                parentClassId: row.GetIntOrNull("parent_class_id"),
                parentFileId: row.GetIntOrNull("parent_file_id"),
                parentClassIdDiff: row.GetIntOrNull("parent_class_id_diff"),
                parentFileIdDiff: row.GetIntOrNull("parent_file_id_diff"),
                name: row.GetString("name"),
                typeId: row.GetInt32("type_id"),
                accessFlags: row.GetInt32("access_flags"),
                sourceCode: row.GetString("source_code"),
                originalFieldId: row.GetIntOrNull("original_field_id"),
                branchId: row.GetInt32("branch_id")
            ));
        }
    }
}