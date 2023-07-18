using System.Collections.Generic;
using System.Data;

using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    
    public class DbBranch
    {
        public readonly int Id;
        public readonly string Name;
        public readonly string Commit;

        public DbBranch(int id, string name, string commit)
        {
            Id = id;
            Name = name;
            Commit = commit;
        }

        // note "commit" is a protected keyword in sqlite
        public const string CreateTable = @"
            CREATE TABLE branches (
                         id INTEGER PRIMARY KEY ASC,
                         name TEXT NOT NULL,
                         sha TEXT NOT NULL)
        ";

        public static void SaveAll(IEnumerable<DbBranch> branches, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO branches ( 
                          id,
                          name,
                          sha
                      ) VALUES ( 
                          @Id,
                          @Name,
                          @Sha)";

            foreach (DbBranch cls in branches)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", cls.Id);
                cmd.AddParameter(System.Data.DbType.String, "@Name", cls.Name);
                cmd.AddParameter(System.Data.DbType.String, "@Sha", cls.Commit);
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
                          name,
                          sha
                    FROM branches
            ";

            return conn.Execute(query).TransformRows(row => new DbBranch(
                id: row.GetInt32("id"),
                name: row.GetString("name"),
                commit: row.GetString("sha")
            ));
        }
    }
}