using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fantastic.Utils
{
    public static class EnumerableUtils
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }
    }
}
