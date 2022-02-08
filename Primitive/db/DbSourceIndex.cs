using System.Collections.Generic;
using System.Data;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    public class DbSourceIndex
    {
        public readonly int ElementId;
        public readonly int FileId;
        public readonly string Type;
        public readonly int StartIdx;
        public readonly int EndIdx;
        public readonly int? StartLine;
        public readonly int? StartColumn;
        public readonly int? EndLine;
        public readonly int? EndColumn;

        public DbSourceIndex(int elementId, int fileId, string type, int startIdx, int endIdx, int? startLine,
            int? startColumn, int? endLine, int? endColumn)
        {
            ElementId = elementId;
            FileId = fileId;
            Type = type;
            StartIdx = startIdx;
            EndIdx = endIdx;
            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
        }

        public static void SaveAll(IEnumerable<DbSourceIndex> sourceIndices, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO source_index ( 
                          element_id,
                          file_id,
                          type,
                          start_idx,
                          end_idx,
                          start_line,
                          start_column,
                          end_line,
                          end_column
                      ) VALUES (
                          @ElementId,
                          @FileId,
                          @Type,
                          @StartIdx,
                          @EndIdx,
                          @StartLine,
                          @StartColumn,
                          @EndLine,
                          @EndColumn
                      )";

            foreach (DbSourceIndex sourceIndex in sourceIndices)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@ElementId", sourceIndex.ElementId);
                cmd.AddParameter(System.Data.DbType.Int32, "@FileId", sourceIndex.FileId);
                cmd.AddParameter(System.Data.DbType.String, "@Type", sourceIndex.Type);
                cmd.AddParameter(System.Data.DbType.Int32, "@StartIdx", sourceIndex.StartIdx);
                cmd.AddParameter(System.Data.DbType.Int32, "@EndIdx", sourceIndex.EndIdx);
                cmd.AddParameter(System.Data.DbType.Int32, "@EndIdx", sourceIndex.EndIdx);
                cmd.AddParameter(System.Data.DbType.Int32, "@StartLine", sourceIndex.StartLine);
                cmd.AddParameter(System.Data.DbType.Int32, "@StartColumn", sourceIndex.StartColumn);
                cmd.AddParameter(System.Data.DbType.Int32, "@EndLine", sourceIndex.EndLine);
                cmd.AddParameter(System.Data.DbType.Int32, "@EndColumn", sourceIndex.EndColumn);

                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static readonly string CreateTable = @"
            CREATE TABLE source_index (
                        element_id   INTEGER NOT NULL,
                        file_id      INTEGER NOT NULL,
                        type         TEXT    NOT NULL,
                        start_idx    INTEGER NOT NULL,
                        end_idx      INTEGER NOT NULL,
                        start_line   INTEGER,
                        start_column INTEGER,
                        end_line     INTEGER,
                        end_column   INTEGER,
                        FOREIGN KEY(file_id) REFERENCES files(id) ON UPDATE CASCADE
                    )
        ";

        public static List<DbSourceIndex> ReadAll(IDbConnection conn)
        {
            string query = @"
                    SELECT
                          element_id,
                          file_id,
                          type,
                          start_idx,
                          end_idx,
                          start_line,
                          start_column,
                          end_line,
                          end_column
                    FROM source_index
            ";

            return conn.Execute(query).TransformRows(row => new DbSourceIndex(
                elementId: row.GetInt32("element_id "),
                fileId: row.GetInt32("file_id"),
                type: row.GetString("type"),
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