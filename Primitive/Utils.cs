using System;

namespace PrimitiveCodebaseElements.Primitive
{
    public static class Utils
    {
        public static R Let<T, R>(this T t, Func<T, R> block)
        {
            return block(t);
        }
    }
}