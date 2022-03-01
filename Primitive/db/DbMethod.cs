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
        public readonly int Id, ParentType, ParentId, ReturnTypeId, AccessFlags, Language;
        public readonly int? CyclomaticScore;
        public readonly string Name;

        public DbMethod(int id, int parentType, int parentId, string name, int returnTypeId, int accessFlags,
            int language, int? cyclomaticScore)
        {
            Id = id;
            ParentType = parentType;
            ParentId = parentId;
            Name = name;
            ReturnTypeId = returnTypeId;
            AccessFlags = accessFlags;
            Language = language;
            CyclomaticScore = cyclomaticScore;
        }

        public const string CreateTable = @"
            CREATE TABLE methods ( 
                id INTEGER PRIMARY KEY ASC, 
                parent_type INTEGER NOT NULL, 
                parent_id INTEGER NOT NULL, 
                name TEXT NOT NULL, 
                return_type_id INTEGER NOT NULL, 
                access_flags INTEGER NOT NULL, 
                field_id TEXT, 
                language INTEGER,
                cyclomatic_score INTEGER,
                FOREIGN KEY(parent_id) REFERENCES classes(id) ON UPDATE CASCADE
            )";

        public static void SaveAll(IEnumerable<DbMethod> methods, IDbConnection conn)
        {
            IDbCommand insertMethodCmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();

            insertMethodCmd.CommandText = @"INSERT INTO methods ( 
                          id,
                          parent_type, 
                          parent_id, 
                          name, 
                          return_type_id, 
                          access_flags,
                          language,
                          cyclomatic_score
                      ) VALUES ( 
                          @Id,
                          @ParentType, 
                          @ParentId, 
                          @Name, 
                          @ReturnTypeId, 
                          @AccessFlags,
                          @Language,
                          @CyclomaticScore
                      )";

            foreach (DbMethod method in methods)
            {
                insertMethodCmd.AddParameter(System.Data.DbType.Int32, "@Id", method.Id);
                insertMethodCmd.AddParameter(System.Data.DbType.Int32, "@ParentType", method.ParentType);

                // the MIN value of Cyclomatic Complexity Score is 1, so 0 means it's not defined
                bool isCycloDefined = method.CyclomaticScore > 0;
                object cycloScore = isCycloDefined ? (object)method.CyclomaticScore : DBNull.Value;

                insertMethodCmd.AddParameter(System.Data.DbType.Int32, "@CyclomaticScore", cycloScore);
                insertMethodCmd.AddParameter(System.Data.DbType.Int32, "@ParentId", method.ParentId);
                insertMethodCmd.AddParameter(System.Data.DbType.String, "@Name", method.Name);
                insertMethodCmd.AddParameter(System.Data.DbType.Int32, "@ReturnTypeId", method.ReturnTypeId);
                insertMethodCmd.AddParameter(System.Data.DbType.UInt32, "@AccessFlags", method.AccessFlags);
                insertMethodCmd.AddParameter(System.Data.DbType.Int32, "@Language", method.Language);

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
                        parent_type, 
                        parent_id, 
                        name, 
                        return_type_id, 
                        access_flags, 
                        language,
                        cyclomatic_score
                    FROM methods
            ";

            return conn.Execute(query).TransformRows(row => new DbMethod(
                id: row.GetInt32("id"),
                parentType: row.GetInt32("parent_type"),
                parentId: row.GetInt32("parent_id"),
                name: row.GetString("name"),
                returnTypeId: row.GetInt32("return_type_id"),
                accessFlags: row.GetInt32("access_flags"),
                language: row.GetInt32("language"),
                cyclomaticScore: row.GetIntOrNull("cyclomatic_score")
            ));
        }
    }
}