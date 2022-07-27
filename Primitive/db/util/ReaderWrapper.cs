using System;
using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive.db.util
{
    [PublicAPI]
    public class ReaderWrapper : IDisposable
    {
        readonly IDataReader reader;
        readonly IDbCommand cmd;
        readonly Dictionary<string, int> ordinalCache = new Dictionary<string, int>();

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
            return reader.GetInt16(GetOrdinal(columnName));
        }

        public int GetInt32(string columnName)
        {
            return reader.GetInt32(GetOrdinal(columnName));
        }

        public long GetInt64(string columnName)
        {
            return reader.GetInt64(GetOrdinal(columnName));
        }

        public int? GetIntOrNull(string columnName)
        {
            return reader.IsDBNull(GetOrdinal(columnName))
                ? (int?)null
                : reader.GetInt32(GetOrdinal(columnName));
        }

        public bool GetBoolean(string columnName)
        {
            return reader.GetBoolean(GetOrdinal(columnName));
        }

        public string GetString(string columnName)
        {
            return reader.IsDBNull(GetOrdinal(columnName))
                ? null
                : reader.GetString(GetOrdinal(columnName));
        }

        public float GetFloat(string columnName)
        {
            return reader.GetFloat(GetOrdinal(columnName));
        }
        
        public double GetDouble(string columnName)
        {
            return reader.GetDouble(GetOrdinal(columnName));
        }

        public object GetValue(string columnName)
        {
            return reader.GetValue(GetOrdinal(columnName));
        }
        
        int GetOrdinal(string columnName)
        {
            if (!ordinalCache.TryGetValue(columnName, out int  ordinal))
            {
                ordinal = reader.GetOrdinal(columnName);
                ordinalCache.Add(columnName, ordinal);
            }
            return ordinal;
        }
    }
}