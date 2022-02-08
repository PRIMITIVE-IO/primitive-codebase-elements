using System.Collections.Generic;
using System.Data;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    public class DbDiffArgument
    {
        public readonly int Id;
        public readonly int MethodId;
        public readonly int ArgIndex;
        public readonly string Name;
        public readonly int TypeId;

        public DbDiffArgument(int id, int methodId, int argIndex, string name, int typeId)
        {
            Id = id;
            MethodId = methodId;
            ArgIndex = argIndex;
            Name = name;
            TypeId = typeId;
        }

        public static readonly string CreateTable =
            @"CREATE TABLE diff_method_arguments ( 
                     id INTEGER PRIMARY KEY ASC, 
                     method_id INTEGER NOT NULL, 
                     arg_index INTEGER NOT NULL, 
                     name TEXT NOT NULL, 
                     type_id INTEGER NOT NULL, 
                     FOREIGN KEY(method_id) REFERENCES diff_methods(id) ON UPDATE CASCADE, 
                     FOREIGN KEY(type_id) REFERENCES types(id) ON UPDATE CASCADE)";
        
        
        
        public static void SaveAll(IEnumerable<DbDiffArgument> arguments, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO diff_method_arguments ( 
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

            foreach (DbDiffArgument argument in arguments)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@Id", argument.Id);
                cmd.AddParameter(System.Data.DbType.Int32, "@MethodId", argument.MethodId);
                cmd.AddParameter(System.Data.DbType.Int32, "@ArgIndex", argument.ArgIndex);
                cmd.AddParameter(System.Data.DbType.String, "@Name", argument.Name);
                cmd.AddParameter(System.Data.DbType.Int32, "@TypeId", argument.TypeId);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbDiffArgument> ReadAll(IDbConnection conn)
        {
            string query = @"
                    SELECT
                        id,
                        method_id,
                        arg_index,
                        name,
                        type_id
                    FROM diff_method_arguments
            ";

            return conn.Execute(query).TransformRows(row => new DbDiffArgument(
                id: row.GetInt32("id"),
                methodId: row.GetInt32("method_id"),
                argIndex: row.GetInt32("arg_index"),
                name: row.GetString("name"),
                typeId: row.GetInt32("type_id")
            ));
        }
    }
}