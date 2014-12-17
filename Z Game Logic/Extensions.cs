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
    }
}
