using System;
using System.Collections.Generic;
using System.Data;

using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    
    public class DbDirectory
    {
        public readonly int Id;
        public readonly string Fqn;
        public readonly double PositionX;
        public readonly double PositionY;

        public DbDirectory(int id, string fqn, double positionX, double positionY)
        {
            Id = id;
            Fqn = fqn;
            PositionX = positionX;
            PositionY = positionY;
        }

        public const string CreateTable = @"
            CREATE TABLE directories (
                id INTEGER PRIMARY KEY ASC,
                fqn        TEXT NOT NULL,
                position_x REAL NOT NULL,
                position_y REAL NOT NULL
        )";

        public static void SaveAll(IEnumerable<DbDirectory> directories, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO directories (
                          id,
                          fqn,
                          position_x,
                          position_y
                      ) VALUES (
                          @Id,
                          @FQN,
                          @PositionX,
                          @PositionY
                      )";

            foreach (DbDirectory cls in directories)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", cls.Id);
                cmd.AddParameter(System.Data.DbType.String, "@FQN", cls.Fqn);
                cmd.AddParameter(System.Data.DbType.Double, "@PositionX", cls.PositionX);
                cmd.AddParameter(System.Data.DbType.Double, "@PositionY", cls.PositionY);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbDirectory> ReadAll(IDbConnection conn)
        {
            const string query = @"
                    SELECT
                          id,
                          fqn,
                          position_x,
                          position_y
                    FROM directories
            ";

            try
            {
                return conn.Execute(query).TransformRows(row => new DbDirectory(
                    id: row.GetInt32("id"),
                    fqn: row.GetString("fqn"),
                    positionX: row.GetDouble("position_x"),
                    positionY: row.GetDouble("position_y")
                ));
            }
            catch (Exception ex)
            {
                PrimitiveLogger.Logger.Instance()
                    .Warn($"Cannot read directories: {ex.Message}", ex);
                return new List<DbDirectory>();
            }
        }
    }
}