using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Utils
{
    public static class CollectionsExtension
    {
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }
            else return false;
        }
        public static bool TryPeek<TValue>(this Stack<TValue> stack, out TValue value)
        {
            value = default;
            if (stack.Count >= 1)
            {
                value = stack.Peek();
                return true;
            }
            else return false;
        }
        public static bool TryPop<TValue>(this Stack<TValue> stack, out TValue value)
        {
            value = default;
            if (stack.Count >= 1)
            {
                value = stack.Pop();
                return true;
            }
            else return false;
        }
    }
}
