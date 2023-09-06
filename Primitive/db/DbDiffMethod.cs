using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    [PublicAPI]
    public class DbDiffMethod
    {
        public readonly int Id, BranchId, ReturnTypeId, AccessFlags;
        public readonly int? ParentFileId, ParentClassIdDiff, OriginalMethodId, ParentClassId, ParentFileIdDiff;
        public readonly string Name, SourceCode;
        public readonly string? FieldId;

        public DbDiffMethod(
            int id, int? parentClassId, int? parentFileId,
            string name, int returnTypeId, int accessFlags,
            string? fieldId,
            string sourceCode,
            int? parentClassIdDiff, int? parentFileIdDiff, int? originalMethodId, int branchId)
        {
            Id = id;
            ParentClassId = parentClassId;
            ParentFileId = parentFileId;
            ParentFileIdDiff = parentFileIdDiff;
            Name = name;
            ReturnTypeId = returnTypeId;
            AccessFlags = accessFlags;
            FieldId = fieldId;
            SourceCode = sourceCode;
            ParentClassIdDiff = parentClassIdDiff;
            OriginalMethodId = originalMethodId;
            BranchId = branchId;
        }

        public const string CreateTable =
            @"CREATE TABLE diff_methods ( 
                         id INTEGER PRIMARY KEY ASC, 
                         parent_class_id INTEGER NULL,
                         parent_class_id_diff INTEGER NULL, 
                         parent_file_id INTEGER NULL, 
                         parent_file_id_diff INTEGER NULL,
                         name TEXT NOT NULL, 
                         return_type_id INTEGER NOT NULL, 
                         access_flags INTEGER NOT NULL, 
                         field_id TEXT, 
                         source_code TEXT, 
						 original_method_id INT NULL,
						 branch_id INT NOT NULL,
						 FOREIGN KEY(branch_id) REFERENCES branches(id) ON UPDATE CASCADE,
						 FOREIGN KEY(parent_class_id) REFERENCES classes(id) ON UPDATE CASCADE,
						 FOREIGN KEY(parent_class_id_diff) REFERENCES diff_classes(id) ON UPDATE CASCADE,
						 FOREIGN KEY(original_method_id) REFERENCES methods(id) ON UPDATE CASCADE)";


        public static void SaveAll(IEnumerable<DbDiffMethod> methods, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO diff_methods (
						id,
						parent_class_id,
                        parent_class_id_diff,
						parent_file_id,
                        parent_file_id_diff,
						name,
						return_type_id,
						access_flags,
						field_id,
						source_code,
						parent_class_id_diff,
						original_method_id,
						branch_id
                      ) VALUES (
						@Id,
						@ParentClassId,
                        @ParentClassIdDiff,
						@ParentFileId,
                        @ParentFileIdDiff,
						@Name,
						@ReturnTypeId,
						@AccessFlags,
						@FieldId,
						@SourceCode,
						@ParentClassIdDiff,
						@OriginalMethodId,
						@BranchId
                      )";

            foreach (DbDiffMethod method in methods)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", method.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentClassId", method.ParentClassId);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentClassIdDiff", method.ParentClassIdDiff);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentFileId", method.ParentFileId);
                cmd.AddParameter(System.Data.DbType.Int32, "@ParentFileIdDiff", method.ParentFileIdDiff);
                cmd.AddParameter(System.Data.DbType.String, "@Name", method.Name);
                cmd.AddParameter(System.Data.DbType.Int32, "@ReturnTypeId", method.ReturnTypeId);
                cmd.AddParameter(System.Data.DbType.Int32, "@AccessFlags", method.AccessFlags);
                cmd.AddParameter(System.Data.DbType.String, "@FieldId", method.FieldId);
                cmd.AddParameter(System.Data.DbType.String, "@SourceCode", method.SourceCode);
                cmd.AddParameter(System.Data.DbType.Int32, "@OriginalMethodId", method.OriginalMethodId);
                cmd.AddParameter(System.Data.DbType.Int32, "@BranchId", method.BranchId);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbDiffMethod> ReadAll(IDbConnection conn)
        {
            const string query = @"
                    SELECT
						id,
						parent_class_id,
						parent_class_id_diff,
						parent_file_id,
						parent_file_id_diff,
						name,
						return_type_id,
						access_flags,
						field_id,
						source_code,					
						original_method_id,
						branch_id
                    FROM diff_methods
            ";

            return conn.Execute(query).TransformRows(row => new DbDiffMethod(
                id: row.GetInt32("id"),
                parentClassId: row.GetIntOrNull("parent_class_id"),
                parentClassIdDiff: row.GetIntOrNull("parent_class_id_diff"),
                parentFileId: row.GetIntOrNull("parent_file_id"),
                parentFileIdDiff: row.GetIntOrNull("parent_file_id_diff"),
                name: row.GetString("name"),
                returnTypeId: row.GetInt32("return_type_id"),
                accessFlags: row.GetInt32("access_flags"),
                fieldId: row.GetString("field_id"),
                sourceCode: row.GetString("source_code"),
                originalMethodId: row.GetIntOrNull("original_method_id"),
                branchId: row.GetInt32("branch_id")
            ));
        }
    }
}