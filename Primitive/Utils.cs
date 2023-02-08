using System;

namespace PrimitiveCodebaseElements.Primitive
{
    public static class Utils
    {
        public static R Let<T, R>(this T t, Func<T, R> block)
        {
            return block(t);
        }
        public static T? TakeIf<T>(this T obj, Func<T, bool> predicate)
        {
            if (predicate(obj)) return obj;
            return default;
        }
        public static T If<T>(bool condition,  Func<T> then, Func<T> @else)
        {
            if (condition) return then();
            return @else();
        }
    }
}