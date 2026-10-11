using System.Collections.Generic;
using System.Data;

using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    
    public class DbArgument
    {
        public readonly int Id;
        public readonly int MethodId;
        public readonly int ArgIndex;
        public readonly string Name;
        public readonly int TypeId;

        public DbArgument(int id, int methodId, int argIndex, string name, int typeId)
        {
            Id = id;
            MethodId = methodId;
            ArgIndex = argIndex;
            Name = name;
            TypeId = typeId;
        }

        public const string CreateTable = @"
            CREATE TABLE method_arguments ( 
                id INTEGER PRIMARY KEY ASC, 
                method_id INTEGER NOT NULL, 
                arg_index INTEGER NOT NULL, 
                name TEXT NOT NULL, 
                type_id INTEGER NOT NULL, 
                FOREIGN KEY(method_id) REFERENCES methods(id) ON UPDATE CASCADE, 
                FOREIGN KEY(type_id) REFERENCES types(id) ON UPDATE CASCADE
            )";

        public static void SaveAll(IEnumerable<DbArgument> arguments, IDbConnection conn)
        {
            IDbCommand insertArgCmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();


            insertArgCmd.CommandText =
                @"INSERT INTO method_arguments ( 
                          id,
                          method_id, 
                          arg_index, 
                          name, 
                          type_id 
                      ) VALUES (
                          @Id,
                          @MethodId, 
                          @ArgIndex, 
                          @Name, 
                          @TypeId
                      )";


            foreach (DbArgument argument in arguments)
            {
                insertArgCmd.AddParameter(System.Data.DbType.Int32, "@Id", argument.Id);
                insertArgCmd.AddParameter(System.Data.DbType.Int32, "@MethodId", argument.MethodId);
                insertArgCmd.AddParameter(System.Data.DbType.UInt32, "@ArgIndex", argument.ArgIndex);
                insertArgCmd.AddParameter(System.Data.DbType.String, "@Name", argument.Name);
                insertArgCmd.AddParameter(System.Data.DbType.Int32, "@TypeId", argument.TypeId);

                insertArgCmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            insertArgCmd.Dispose();
        }

        public static List<DbArgument> ReadAll(IDbConnection conn)
        {
            const string query = @"
                SELECT
                          id,
                          method_id, 
                          arg_index, 
                          name, 
                          type_id 
                FROM method_arguments
            ";

            return conn.Execute(query).TransformRows(row => new DbArgument(
                id: row.GetInt32("id"),
                methodId: row.GetInt32("method_id"),
                argIndex: row.GetInt32("arg_index"),
                name: row.GetString("name"),
                typeId: row.GetInt32("type_id")
            ));
        }
    }
}