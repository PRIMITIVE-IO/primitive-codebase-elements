using System;
using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    [PublicAPI]
    public class DbMethod
    {
        public readonly int Id, ParentFileId, ReturnTypeId, AccessFlags;
        public readonly int? CyclomaticScore, ParentClassId;
        public readonly string Name;

        public DbMethod(int id, int? parentClassId, int parentFileId, string name, int returnTypeId, int accessFlags,
            int? cyclomaticScore)
        {
            Id = id;
            ParentClassId = parentClassId;
            ParentFileId = parentFileId;
            Name = name;
            ReturnTypeId = returnTypeId;
            AccessFlags = accessFlags;
            CyclomaticScore = cyclomaticScore;
        }

        public const string CreateTable = @"
            CREATE TABLE methods ( 
                id INTEGER PRIMARY KEY ASC, 
                parent_class_id INTEGER, 
                parent_file_id INTEGER NOT NULL, 
                name TEXT NOT NULL, 
                return_type_id INTEGER NOT NULL, 
                access_flags INTEGER NOT NULL, 
                field_id TEXT, 
                cyclomatic_score INTEGER,                
                FOREIGN KEY(parent_class_id) REFERENCES classes(id) ON UPDATE CASCADE,
                FOREIGN KEY(parent_file_id) REFERENCES files(id) ON UPDATE CASCADE
            )";

        public static void SaveAll(IEnumerable<DbMethod> methods, IDbConnection conn)
        {
            IDbCommand insertMethodCmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();

            insertMethodCmd.CommandText = @"INSERT INTO methods ( 
                          id,
                          parent_class_id, 
                          parent_file_id, 
                          name, 
                          return_type_id, 
                          access_flags,
                          cyclomatic_score
                      ) VALUES ( 
                          @Id,
                          @ParentClassId, 
                          @ParentFileId, 
                          @Name, 
                          @ReturnTypeId, 
                          @AccessFlags,
                          @CyclomaticScore
                      )";

            foreach (DbMethod method in methods)
            {
                insertMethodCmd.AddParameter(System.Data.DbType.Int32, "@Id", method.Id);
                insertMethodCmd.AddParameter(System.Data.DbType.Int32, "@ParentClassId", method.ParentClassId);

                // the MIN value of Cyclomatic Complexity Score is 1, so 0 means it's not defined
                bool isCycloDefined = method.CyclomaticScore > 0;
                object cycloScore = isCycloDefined ? (object)method.CyclomaticScore : DBNull.Value;

                insertMethodCmd.AddParameter(System.Data.DbType.Int32, "@CyclomaticScore", cycloScore);
                insertMethodCmd.AddParameter(System.Data.DbType.Int32, "@ParentFileId", method.ParentFileId);
                insertMethodCmd.AddParameter(System.Data.DbType.String, "@Name", method.Name);
                insertMethodCmd.AddParameter(System.Data.DbType.Int32, "@ReturnTypeId", method.ReturnTypeId);
                insertMethodCmd.AddParameter(System.Data.DbType.UInt32, "@AccessFlags", method.AccessFlags);

                insertMethodCmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            insertMethodCmd.Dispose();
        }

        public static List<DbMethod> ReadAll(IDbConnection conn)
        {
            const string query = @"
                    SELECT
                        id,
                        parent_class_id, 
                        parent_file_id, 
                        name, 
                        return_type_id, 
                        access_flags, 
                        cyclomatic_score
                    FROM methods
            ";

            return conn.Execute(query).TransformRows(row => new DbMethod(
                id: row.GetInt32("id"),
                parentClassId: row.GetIntOrNull("parent_class_id"),
                parentFileId: row.GetInt32("parent_file_id"),
                name: row.GetString("name"),
                returnTypeId: row.GetInt32("return_type_id"),
                accessFlags: row.GetInt32("access_flags"),
                cyclomaticScore: row.GetIntOrNull("cyclomatic_score")
            ));
        }
    }
}