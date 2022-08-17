using System;
using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive.dto
{
    [PublicAPI]
    public class CodeLocation : IComparable<CodeLocation>
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
            return obj.GetType() == this.GetType() && Equals((CodeLocation)obj);
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

        public int CompareTo(CodeLocation other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            int lineComparison = Line.CompareTo(other.Line);
            return lineComparison != 0 
                ? lineComparison 
                : Column.CompareTo(other.Column);
        }
    }
}