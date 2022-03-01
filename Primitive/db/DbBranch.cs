using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    [PublicAPI]
    public class DbBranch
    {
        public readonly int Id;
        public readonly string Name;

        public DbBranch(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public const string CreateTable = @"
            CREATE TABLE branches (
                         id INTEGER PRIMARY KEY ASC,
                         name TEXT NOT NULL)
        ";

        public static void SaveAll(IEnumerable<DbBranch> branches, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO branches ( 
                          id,
                          name
                      ) VALUES ( 
                          @Id,
                          @Name)";

            foreach (DbBranch cls in branches)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", cls.Id);
                cmd.AddParameter(System.Data.DbType.String, "@Name", cls.Name);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbBranch> ReadAll(IDbConnection conn)
        {
            const string query = @"
                    SELECT
                          id,
                          name
                    FROM branches
            ";

            return conn.Execute(query).TransformRows(row => new DbBranch(
                id: row.GetInt32("id"),
                name: row.GetString("name")
            ));
        }
    }
}