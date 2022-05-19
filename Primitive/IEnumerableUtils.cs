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
            Dictionary<K, V> dict = new Dictionary<K, V>();
            foreach (T elem in elems)
            {
                K key = keyExtractor(elem);
                V value = valueTransformer(elem);
                
                if (!dict.TryAdd(key, value))
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
        
        public static R MaxOrDefault<T, R>(this List<T> list, Func<T, R> extractor)
        {
            if (!list.Any()) return default;
            return list.Max(extractor);
        }
    }
}