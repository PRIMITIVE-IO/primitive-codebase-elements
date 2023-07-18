using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;


namespace PrimitiveCodebaseElements.Primitive.db.util
{
    
    public static class SqLiteUtil
    {
        public static ReaderWrapper Execute(this IDbConnection conn, string sql, params Param[] parameters)
        {
            IDbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            foreach (Param param in parameters)
            {
                IDbDataParameter p = cmd.CreateParameter();
                p.DbType = param.Type;
                p.ParameterName = param.Name;
                p.Value = param.Value;
                cmd.Parameters.Add(p);
            }

            ReaderWrapper readerWrapper = new ReaderWrapper(cmd.ExecuteReader(), cmd);
            cmd.Dispose();
            return readerWrapper;
        }

        public static void ExecuteNonQuery(this IDbConnection conn, string sql, params Param[] parameters)
        {
            IDbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            foreach (Param param in parameters)
            {
                IDbDataParameter p = cmd.CreateParameter();
                p.DbType = param.Type;
                p.ParameterName = param.Name;
                p.Value = param.Value;
                cmd.Parameters.Add(p);
            }

            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        public static ReaderWrapper Execute(this IDbConnection conn, string sql)
        {
            return conn.Execute(sql, Array.Empty<Param>());
        }

        public static List<R> TransformRows<R>(this ReaderWrapper reader, Func<ReaderWrapper, R> transformer)
        {
            List<R> res = new List<R>();
            while (reader.Read())
            {
                res.Add(transformer(reader));
            }

            reader.Dispose();
            
            return res;
        }

        public static void AddParameter(this IDbCommand cmd, System.Data.DbType type, string name, object value)
        {
            IDbDataParameter p = cmd.CreateParameter();
            p.DbType = type;
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
        
        public static Task<T> LoadAsync<T>(
            Func<IDbConnection, T> loader, 
            Func<IDbConnection> connectionProvider,
            ProgressStepper stepper)
        {
            return Task.Run(() =>
            {
                using IDbConnection conn = connectionProvider();
                conn.Open();
                T res = loader(conn);
                stepper.Step();
                return res;
            });
        }
    }
}