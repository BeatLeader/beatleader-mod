using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Utils {
    internal static class BasicCoroutines {
        public static IEnumerator RebuildUICoroutine(RectTransform rect) {
            if (rect == null) yield break;
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
    }
}
