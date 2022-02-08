using System.Collections.Generic;
using System.Data;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    public class DbDiffField
    {
        public readonly int Id;
        public readonly int ParentType;
        public readonly int? ParentId;
        public readonly int? ParentIdDiff;
        public readonly string Name;
        public readonly int TypeId;
        public readonly int AccessFlags;
        public readonly string SourceCode;
        public readonly int Language;
        public readonly int? OriginalFieldId;
        public readonly int BranchId;

        public DbDiffField(int id, int parentType, int? parentId, int? parentIdDiff, string name, int typeId, int accessFlags, string sourceCode, int language, int? originalFieldId, int branchId)
        {
            Id = id;
            ParentType = parentType;
            ParentId = parentId;
            ParentIdDiff = parentIdDiff;
            Name = name;
            TypeId = typeId;
            AccessFlags = accessFlags;
            SourceCode = sourceCode;
            Language = language;
            OriginalFieldId = originalFieldId;
            BranchId = branchId;
        }
        
        public static readonly string CreateTable =
            @"CREATE TABLE diff_fields (
                         id INTEGER PRIMARY KEY ASC,
                         parent_type INTEGER NOT NULL,
                         parent_id INTEGER NULL,
                         parent_id_diff INTEGER NULL,
                         name TEXT NOT NULL,
                         type_id INTEGER NOT NULL,
                         access_flags INTEGER NOT NULL,
                         source_code TEXT,
                         language INTEGER,
						 original_field_id INT NULL,
						 branch_id INT,
						 FOREIGN KEY(branch_id) REFERENCES diff_branches(id) ON UPDATE CASCADE,
                         FOREIGN KEY(parent_id) REFERENCES classes(id) ON UPDATE CASCADE,
						 FOREIGN KEY(parent_id_diff) REFERENCES diff_classes(id) ON UPDATE CASCADE,
						 FOREIGN KEY(original_field_id) REFERENCES fields(id) ON UPDATE CASCADE,
                         FOREIGN KEY(type_id) REFERENCES types(id) ON UPDATE CASCADE)";
        
        
        public static void SaveAll(IEnumerable<DbDiffField> directories, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO diff_fields (
                        id,
                        parent_type,
                        parent_id,
                        parent_id_diff,
                        name,
                        type_id,
                        access_flags,
                        source_code,
                        language,
                        original_field_id,
                        branch_id
                      ) VALUES (
                        @Id,
                        @ParentType,
                        @ParentId,
                        @ParentIdDiff,
                        @Name,
                        @TypeId,
                        @AccessFlags,
                        @SourceCode,
                        @Language,
                        @OriginalFieldId,
                        @BranchId
                      )";

            foreach (DbDiffField dir in directories)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", dir.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentType", dir.ParentType);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentId", dir.ParentId);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentIdDiff", dir.ParentIdDiff);
                cmd.AddParameter(System.Data.DbType.String, "@Name", dir.Name);
                cmd.AddParameter(System.Data.DbType.Int32, "@TypeId", dir.TypeId);
                cmd.AddParameter(System.Data.DbType.Int32, "@AccessFlags", dir.AccessFlags);
                cmd.AddParameter(System.Data.DbType.String, "@SourceCode", dir.SourceCode);
                cmd.AddParameter(System.Data.DbType.Int32, "@Language", dir.Language);
                cmd.AddParameter(System.Data.DbType.Int32, "@OriginalFieldId", dir.OriginalFieldId);
                cmd.AddParameter(System.Data.DbType.Int32, "@BranchId", dir.BranchId);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbDiffField> ReadAll(IDbConnection conn)
        {
            string query = @"
                    SELECT
                        id,
                        parent_type,
                        parent_id,
                        parent_id_diff,
                        name,
                        type_id,
                        access_flags,
                        source_code,
                        language,
                        original_field_id,
                        branch_id
                    FROM diff_fields
            ";

            return conn.Execute(query).TransformRows(row => new DbDiffField(
                id: row.GetInt32("id"),
                parentType: row.GetInt32("parent_type"),
                parentId: row.GetIntOrNull("parent_id"),
                parentIdDiff: row.GetIntOrNull("parent_id_diff"),
                name: row.GetString("name"),
                typeId: row.GetInt32("type_id"),
                accessFlags: row.GetInt32("access_flags"),
                sourceCode: row.GetString("source_code"),
                language: row.GetInt32("language"),
                originalFieldId: row.GetInt32("original_field_id"),
                branchId: row.GetInt32("branch_id")
            ));
        }
    }
}