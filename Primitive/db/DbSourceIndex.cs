using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    [PublicAPI]
    public class DbSourceIndex
    {
        public readonly int ElementId;
        public readonly int FileId;
        public readonly string Type;
        public readonly int StartLine;
        public readonly int StartColumn;
        public readonly int EndLine;
        public readonly int EndColumn;

        public DbSourceIndex(int elementId, int fileId, string type, int startLine,
            int startColumn, int endLine, int endColumn)
        {
            ElementId = elementId;
            FileId = fileId;
            Type = type;
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
                          start_line,
                          start_column,
                          end_line,
                          end_column
                      ) VALUES (
                          @ElementId,
                          @FileId,
                          @Type,
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

        public const string CreateTable = @"
            CREATE TABLE source_index (
                        element_id   INTEGER NOT NULL,
                        file_id      INTEGER NOT NULL,
                        type         TEXT    NOT NULL,
                        start_idx    INTEGER NOT NULL default -1,
                        end_idx      INTEGER NOT NULL default -1,
                        start_line   INTEGER NOT NULL,
                        start_column INTEGER NOT NULL,
                        end_line     INTEGER NOT NULL,
                        end_column   INTEGER NOT NULL,
                        FOREIGN KEY(file_id) REFERENCES files(id) ON UPDATE CASCADE
                    )
        ";

        public static List<DbSourceIndex> ReadAll(IDbConnection conn)
        {
            const string query = @"
                    SELECT
                          element_id,
                          file_id,
                          type,
                          start_line,
                          start_column,
                          end_line,
                          end_column
                    FROM source_index
            ";

            return conn.Execute(query).TransformRows(row => new DbSourceIndex(
                elementId: row.GetInt32("element_id"),
                fileId: row.GetInt32("file_id"),
                type: row.GetString("type"),
                startLine: row.GetInt32("start_line"),
                startColumn: row.GetInt32("start_column"),
                endLine: row.GetInt32("end_line"),
                endColumn: row.GetInt32("end_column")
            ));
        }
    }
}