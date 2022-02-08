using System.Collections.Generic;
using System.Data;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    public class DbClassReference
    {
        public readonly int Id;
        public readonly int Type;
        public readonly int FromId;
        public readonly int ToId;
        public readonly int StartPosition;
        public readonly int EndPosition;
        public readonly int StartIdx;
        public readonly int EndIdx;
        public readonly int? StartLine;
        public readonly int? StartColumn;
        public readonly int? EndLine;
        public readonly int? EndColumn;

        public DbClassReference(int id, int type, int fromId, int toId, int startPosition, int endPosition,
            int startIdx, int endIdx, int? startLine, int? startColumn, int? endLine, int? endColumn)
        {
            Id = id;
            Type = type;
            FromId = fromId;
            ToId = toId;
            StartPosition = startPosition;
            EndPosition = endPosition;
            StartIdx = startIdx;
            EndIdx = endIdx;
            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
        }

        public static readonly string CreateTable = @"
            CREATE TABLE class_reference ( 
                id INTEGER PRIMARY KEY ASC, 
                type INTEGER NOT NULL, 
                from_id INTEGER NOT NULL, 
                to_id INTEGER NOT NULL, 
                start_position INTEGER, 
                end_position INTEGER,
                start_idx INTEGER,
                end_idx INTEGER,
                start_line   INTEGER,
                start_column INTEGER,
                end_line     INTEGER,
                end_column   INTEGER,
                FOREIGN KEY(from_id) REFERENCES classes(id) ON UPDATE CASCADE, 
                FOREIGN KEY(to_id) REFERENCES methods(id) ON UPDATE CASCADE
            )";

        public static void SaveAll(IEnumerable<DbClassReference> classReferences, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO class_reference ( 
                          id,
                          type, 
                          from_id, 
                          to_id, 
                          start_position, 
                          end_position,
                          start_idx,
                          end_idx,
                          start_line,
                          start_column,
                          end_line,
                          end_column
                      ) VALUES ( 
                          @Id,
                          @Type, 
                          @FromId, 
                          @ToId, 
                          @StartPosition, 
                          @EndPosition,
                          @StartIdx,
                          @EndIdx,
                          @StartLine,
                          @StartColumn,
                          @EndLine,
                          @EndColumn)";

            foreach (DbClassReference cls in classReferences)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", cls.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@FromId", cls.FromId);
                cmd.AddParameter(System.Data.DbType.Int32, "@Type", cls.Type);
                cmd.AddParameter(System.Data.DbType.Int32, "@ToId", cls.ToId);
                cmd.AddParameter(System.Data.DbType.UInt32, "@StartPosition", cls.StartPosition);
                cmd.AddParameter(System.Data.DbType.UInt32, "@EndPosition", cls.EndPosition);
                cmd.AddParameter(System.Data.DbType.UInt32, "@StartIdx", cls.StartIdx);
                cmd.AddParameter(System.Data.DbType.UInt32, "@EndIdx", cls.EndIdx);
                cmd.AddParameter(System.Data.DbType.Int32, "@StartLine", cls.StartLine);
                cmd.AddParameter(System.Data.DbType.Int32, "@StartColumn", cls.StartColumn);
                cmd.AddParameter(System.Data.DbType.Int32, "@EndLine", cls.EndLine);
                cmd.AddParameter(System.Data.DbType.Int32, "@EndColumn", cls.EndColumn);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbClassReference> ReadAll(IDbConnection conn)
        {
            string query = @"
                    SELECT
                          id,
                          type, 
                          from_id, 
                          to_id, 
                          start_position, 
                          end_position,
                          start_idx,
                          end_idx,
                          start_line,
                          start_column,
                          end_line,
                          end_column
                    FROM class_reference
            ";

            return conn.Execute(query).TransformRows(row => new DbClassReference(
                id: row.GetInt32("id"),
                type: row.GetInt32("type"),
                fromId: row.GetInt32("from_id"),
                toId: row.GetInt32("to_id"),
                startPosition: row.GetInt32("start_position"),
                endPosition: row.GetInt32("end_position"),
                startIdx: row.GetInt32("start_idx"),
                endIdx: row.GetInt32("end_idx"),
                startLine: row.GetIntOrNull("start_line"),
                startColumn: row.GetIntOrNull("start_column"),
                endLine: row.GetIntOrNull("end_line"),
                endColumn: row.GetIntOrNull("end_column")
            ));
        }
    }
}