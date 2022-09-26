using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    [PublicAPI]
    public class DbMethodReference
    {
        public readonly int Id;
        public readonly int Type;
        public readonly int FromId;
        public readonly int ToId;
        public readonly int StartLine;
        public readonly int StartColumn;
        public readonly int EndLine;
        public readonly int EndColumn;

        public DbMethodReference(int id, int type, int fromId, int toId, int startLine, int startColumn, int endLine, int endColumn)
        {
            Id = id;
            Type = type;
            FromId = fromId;
            ToId = toId;
            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
        }

        public const string CreateTable = @"
            CREATE TABLE method_reference ( 
                id INTEGER PRIMARY KEY ASC, 
                type INTEGER NOT NULL, 
                from_id INTEGER NOT NULL, 
                to_id INTEGER NOT NULL,
                start_line   INTEGER NOT NULL,
                start_column INTEGER NOT NULL,
                end_line     INTEGER NOT NULL,
                end_column   INTEGER NOT NULL,                
                FOREIGN KEY(from_id) REFERENCES methods(id) ON UPDATE CASCADE, 
                FOREIGN KEY(to_id) REFERENCES methods(id) ON UPDATE CASCADE
            )";

        public static void SaveAll(IEnumerable<DbMethodReference> methodReferences, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO method_reference ( 
                          id,
                          type, 
                          from_id, 
                          to_id, 
                          start_line,
                          start_column,
                          end_line,
                          end_column
                      ) VALUES (
                          @Id,
                          @Type, 
                          @FromId, 
                          @ToId, 
                          @StartLine,
                          @StartColumn,
                          @EndLine,
                          @EndColumn)";

            foreach (DbMethodReference methodReference in methodReferences)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", methodReference.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@Type", methodReference.Type);
                cmd.AddParameter(System.Data.DbType.Int32, "@FromId", methodReference.FromId);
                cmd.AddParameter(System.Data.DbType.Int32, "@ToId", methodReference.ToId);
                cmd.AddParameter(System.Data.DbType.UInt32, "@StartLine", methodReference.StartLine);
                cmd.AddParameter(System.Data.DbType.UInt32, "@StartColumn", methodReference.StartColumn);
                cmd.AddParameter(System.Data.DbType.UInt32, "@EndLine", methodReference.EndLine);
                cmd.AddParameter(System.Data.DbType.UInt32, "@EndColumn", methodReference.EndColumn);

                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbMethodReference> ReadAll(IDbConnection conn)
        {
            const string query = @"
                    SELECT
                          id,
                          type, 
                          from_id, 
                          to_id, 
                          start_line,
                          start_column,
                          end_line,
                          end_column
                    FROM method_reference
            ";

            return conn.Execute(query).TransformRows(row => new DbMethodReference(
                id: row.GetInt32("id"),
                type: row.GetInt32("type"),
                fromId: row.GetInt32("from_id"),
                toId: row.GetInt32("to_id"),
                startLine: row.GetInt32("start_line"),
                startColumn: row.GetInt32("start_column"),
                endLine: row.GetInt32("end_line"),
                endColumn: row.GetInt32("end_column")
            ));
        }
    }
}