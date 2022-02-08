namespace PrimitiveCodebaseElements.Primitive.db.util
{
    public class Param
    {
        public readonly System.Data.DbType Type;
        public readonly string Name;
        public readonly object Value;

        public Param(System.Data.DbType type, string name, object value)
        {
            Type = type;
            Name = name;
            Value = value;
        }
    }
}