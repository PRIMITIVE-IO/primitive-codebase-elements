using System.Collections.Generic;
using System.Data;

using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    
    public class DbType
    {
        public readonly int Id;
        public readonly string Signature;

        public const string CreateTable = @"
            CREATE TABLE types ( 
               id INTEGER PRIMARY KEY ASC, 
               signature TEXT NOT NULL
            )";

        public DbType(int id, string signature)
        {
            Id = id;
            Signature = signature;
        }

        public static void SaveAll(IEnumerable<DbType> types, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText = "INSERT INTO types (id, signature) VALUES (@id, @Signature)";

            foreach (DbType type in types)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@id", type.Id);
                cmd.AddParameter(System.Data.DbType.String, "@Signature", type.Signature);

                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbType> ReadAll(IDbConnection conn)
        {
            const string query = @"
                SELECT
                    id,
                    signature
                FROM types
            ";

            return conn.Execute(query).TransformRows(row => new DbType(
                id: row.GetInt32("id"),
                signature: row.GetString("signature")
            ));
        }
    }
}