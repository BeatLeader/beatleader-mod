using System;
using System.Collections.Generic;

namespace BeatLeader.Utils {
    public static class CollectionsExtension {
        public static LinkedListNode<T>? FindNode<T>(this LinkedList<T> list, Func<LinkedListNode<T>, bool> predicate) {
            var node = list.First;
            while (node is not null) {
                if (predicate(node)) return node;
                node = node.Next;
            }
            return null;
        }
        
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> range) {
            foreach (var item in range) list.Add(item);
        }

        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value) {
            if (!dictionary.ContainsKey(key)) {
                dictionary.Add(key, value);
                return true;
            } else return false;
        }

        public static bool TryPeek<TValue>(this Stack<TValue> stack, out TValue value) {
            value = default;
            if (stack.Count >= 1) {
                value = stack.Peek();
                return true;
            } else return false;
        }

        public static bool TryPop<TValue>(this Stack<TValue> stack, out TValue value) {
            value = default;
            if (stack.Count >= 1) {
                value = stack.Pop();
                return true;
            } else return false;
        }
    }
}