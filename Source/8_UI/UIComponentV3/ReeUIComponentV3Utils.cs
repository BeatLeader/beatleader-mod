using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatLeader.Components {
    internal static class ReeUIComponentV3Utils {
        public static IEnumerable<T> GetReeComponentsInChildren<T>(this Transform transform) where T : class {
            return transform
                .GetComponentsInChildren<ReeUIComponentV3InstanceKeeper>()
                .Where(x => x.Instance is T)
                .Select(x => x.Instance as T)!;
        }
    }
}