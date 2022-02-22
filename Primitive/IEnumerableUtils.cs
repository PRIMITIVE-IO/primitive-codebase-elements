using System;
using System.Collections.Generic;
using System.Linq;

namespace PrimitiveCodebaseElements.Primitive
{
    public static class IEnumerableUtils
    {
        public static IEnumerable<R> SelectNotNull<T, R>(this IEnumerable<T> e, Func<T, R> f)
        {
            return e.Select(f)
                .Where(it => it != null)
                .Cast<R>();
        }

        public static IEnumerable<T> EnumerableOfNotNull<T>(params T[] list)
        {
            return list.Where(it => it != null)
                .Cast<T>();
        }

        public static Dictionary<K, V> ConcatDict<K, V>(this Dictionary<K, V> t, Dictionary<K, V> o)
        {
            return t.Concat(o).ToDictionary(it => it.Key, it => it.Value);
        }

        public static string JoinToString<T>(this IEnumerable<T> e, string separator)
        {
            return string.Join(separator, e);
        }

        public static Dictionary<K, V> ToDictIgnoringDuplicates<T, K, V>(
            this IEnumerable<T> elems,
            Func<T, K> keyExtractor,
            Func<T, V> valueTransformer,
            Action<T> onKeyDuplication = null)
        {
            var dict = new Dictionary<K, V>();
            foreach (var elem in elems)
            {
                var key = keyExtractor(elem);
                var value = valueTransformer(elem);
                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, value);
                }
                else
                {
                    onKeyDuplication?.Invoke(elem);
                }
            }

            return dict;
        }

        public static Dictionary<K, V> ToDictIgnoringDuplicates<K, V>(
            this IEnumerable<V> e,
            Func<V, K> keyExtractor,
            Action<V> onKeyDuplication = null)
        {
            return e.ToDictIgnoringDuplicates(
                keyExtractor: keyExtractor,
                valueTransformer: v => v,
                onKeyDuplication: onKeyDuplication
            );
        }
    }
}