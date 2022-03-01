using System;
using System.Data;

namespace PrimitiveCodebaseElements.Primitive.db.util
{
    public class ReaderWrapper : IDisposable
    {
        readonly IDataReader reader;
        readonly IDbCommand cmd;

        public ReaderWrapper(IDataReader reader, IDbCommand cmd)
        {
            this.reader = reader;
            this.cmd = cmd;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            reader.Dispose();
            cmd.Dispose();
        }

        public bool Read()
        {
            return reader.Read();
        }

        public short GetInt16(string columnName)
        {
            return reader.GetInt16(reader.GetOrdinal(columnName));
        }

        public int GetInt32(string columnName)
        {
            return reader.GetInt32(reader.GetOrdinal(columnName));
        }

        public long GetInt64(string columnName)
        {
            return reader.GetInt64(reader.GetOrdinal(columnName));
        }

        public int? GetIntOrNull(string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName))
                ? (int?)null
                : reader.GetInt32(reader.GetOrdinal(columnName));
        }

        public bool GetBoolean(string columnName)
        {
            return reader.GetBoolean(reader.GetOrdinal(columnName));
        }

        public string GetString(string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName))
                ? null
                : reader.GetString(reader.GetOrdinal(columnName));
        }

        public float GetFloat(string columnName)
        {
            return reader.GetFloat(reader.GetOrdinal(columnName));
        }
        
        public double GetDouble(string columnName)
        {
            return reader.GetDouble(reader.GetOrdinal(columnName));
        }

        public object GetValue(string columnName)
        {
            return reader.GetValue(reader.GetOrdinal(columnName));
        }
    }
}