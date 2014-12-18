using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic
{
    static class Extensions
    {
        public static string ToStringList<T>(this ICollection<T> list)
        {
            List<string> res = new List<string>();

            list.ToList().ForEach(a => res.Add(a.ToString()));

            return string.Join(" | ", res);
        }

        public static IEnumerable<TKey> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> getKey)
        {
            Dictionary<TKey, TSource> dictionary = new Dictionary<TKey, TSource>();

            foreach (TSource item in source)
            {
                TKey key = getKey(item);
                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, item);
                }
            }

            return dictionary.Select(item => item.Key);
        }
    }
}
