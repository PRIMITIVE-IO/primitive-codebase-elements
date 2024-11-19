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
            return predicate(obj) ? obj : default;
        }
        public static T If<T>(bool condition,  Func<T> then, Func<T> @else)
        {
            return condition ? then() : @else();
        }
    }
}