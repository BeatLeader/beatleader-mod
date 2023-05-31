using System;
using System.Collections.Generic;
using BeatLeader.Utils;
using JetBrains.Annotations;

namespace BeatLeader {
    [PublicAPI]
    internal class StaticMemoryPool<T, TItem> : Singleton<T> {
        protected static readonly Stack<TItem> items = new();
        
        public virtual IList<TItem> Shrink(int count) {
            var arr = new TItem[Math.Min(items.Count, count)];
            for (var i = 0; i < count; i++) {
                if (!items!.TryPop(out arr[i])) break;
            }
            return arr;
        }

        public virtual void Expand(int count) {
            for (var i = 0; i < count; i++) {
                items.Push(Activator.CreateInstance<TItem>());
            }
        }
    }
}