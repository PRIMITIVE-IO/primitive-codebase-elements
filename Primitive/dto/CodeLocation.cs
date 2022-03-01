using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive.dto
{
    [PublicAPI]
    public class CodeLocation
    {
        //1-based
        public readonly int Line;
        //1-based
        public readonly int Column;

        public CodeLocation(int line, int column)
        {
            Line = line;
            Column = column;
        }

        protected bool Equals(CodeLocation other)
        {
            return Line == other.Line && Column == other.Column;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CodeLocation)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Line * 397) ^ Column;
            }
        }

        public override string ToString()
        {
            return $"{nameof(Line)}: {Line}, {nameof(Column)}: {Column}";
        }
    }
}