using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    public class DbDiffMethod
    {
        public readonly int Id;
        public readonly int ParentType;
        public readonly int? ParentId;
        public readonly string Name;
        public readonly int ReturnTypeId;
        public readonly int AccessFlags;
        [CanBeNull] public readonly string FieldId;
        public readonly string SourceCode;
        public readonly int Language;
        public readonly int? ParentIdDiff;
        public readonly int? OriginalMethodId;
        public readonly int BranchId;

        public DbDiffMethod(int id, int parentType, int? parentId, string name, int returnTypeId, int accessFlags, [CanBeNull] string fieldId, string sourceCode, int language, int? parentIdDiff, int? originalMethodId, int branchId)
        {
            Id = id;
            ParentType = parentType;
            ParentId = parentId;
            Name = name;
            ReturnTypeId = returnTypeId;
            AccessFlags = accessFlags;
            FieldId = fieldId;
            SourceCode = sourceCode;
            Language = language;
            ParentIdDiff = parentIdDiff;
            OriginalMethodId = originalMethodId;
            BranchId = branchId;
        }
        
        public static readonly string CreateTable =
                @"CREATE TABLE diff_methods ( 
                         id INTEGER PRIMARY KEY ASC, 
                         parent_type INTEGER NOT NULL, 
                         parent_id INTEGER NULL, 
                         name TEXT NOT NULL, 
                         return_type_id INTEGER NOT NULL, 
                         access_flags INTEGER NOT NULL, 
                         field_id TEXT, 
                         source_code TEXT, 
                         language INTEGER,
                         parent_id_diff INTEGER NULL, 
						 original_method_id INT NULL,
						 branch_id INT NOT NULL,
						 FOREIGN KEY(branch_id) REFERENCES diff_branches(id) ON UPDATE CASCADE,
                         FOREIGN KEY(parent_id) REFERENCES classes(id) ON UPDATE CASCADE,
						 FOREIGN KEY(parent_id_diff) REFERENCES diff_classes(id) ON UPDATE CASCADE,
						 FOREIGN KEY(original_method_id) REFERENCES methods(id) ON UPDATE CASCADE)";
        
        
        public static void SaveAll(IEnumerable<DbDiffMethod> methods, IDbConnection conn)
        {
	        IDbCommand cmd = conn.CreateCommand();
	        IDbTransaction transaction = conn.BeginTransaction();
	        cmd.CommandText =
		        @"INSERT INTO diff_methods (
						id,
						parent_type,
						parent_id,
						name,
						return_type_id,
						access_flags,
						field_id,
						source_code,
						language,
						parent_id_diff,
						original_method_id,
						branch_id
                      ) VALUES (
						@Id,
						@ParentType,
						@ParentId,
						@Name,
						@ReturnTypeId,
						@AccessFlags,
						@FieldId,
						@SourceCode,
						@Language,
						@ParentIdDiff,
						@OriginalMethodId,
						@BranchId
                      )";

	        foreach (DbDiffMethod method in methods)
	        {
		        cmd.AddParameter(System.Data.DbType.Int32, "@Id", method.Id);
		        cmd.AddParameter(System.Data.DbType.Int32, "@ParentType", method.ParentType);
		        cmd.AddParameter(System.Data.DbType.Int32, "@ParentId", method.ParentId);
		        cmd.AddParameter(System.Data.DbType.String, "@Name", method.Name);
		        cmd.AddParameter(System.Data.DbType.Int32, "@ReturnTypeId", method.ReturnTypeId);
		        cmd.AddParameter(System.Data.DbType.Int32, "@AccessFlags", method.AccessFlags);
		        cmd.AddParameter(System.Data.DbType.String, "@FieldId", method.FieldId);
		        cmd.AddParameter(System.Data.DbType.String, "@SourceCode", method.SourceCode);
		        cmd.AddParameter(System.Data.DbType.Int32, "@Language", method.Language);
		        cmd.AddParameter(System.Data.DbType.Int32, "@ParentIdDiff", method.ParentIdDiff);
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
	        string query = @"
                    SELECT
						id,
						parent_type,
						parent_id,
						name,
						return_type_id,
						access_flags,
						field_id,
						source_code,
						language,
						parent_id_diff,
						original_method_id,
						branch_id
                    FROM diff_methods
            ";

	        return conn.Execute(query).TransformRows(row => new DbDiffMethod(
		        id: row.GetInt32("id"),
		        parentType: row.GetInt32("parent_type"),
		        parentId: row.GetIntOrNull("parent_id"),
		        name: row.GetString("name"),
		        returnTypeId: row.GetInt32("return_type_id"),
		        accessFlags: row.GetInt32("access_flags"),
		        fieldId: row.GetString("field_id"),
		        sourceCode: row.GetString("source_code"),
		        language: row.GetInt32("language"),
		        parentIdDiff: row.GetIntOrNull("parent_id_diff"),
		        originalMethodId: row.GetIntOrNull("original_method_id"),
		        branchId: row.GetInt32("branch_id")
	        ));
        }
    }
}