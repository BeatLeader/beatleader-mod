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

        public static IEnumerator AnimateGroupCoroutine(this CanvasGroup group, 
            float startPoint, float endPoint, float duration = 0.5f, int fps = 120) {
            yield return new WaitForEndOfFrame();
            group.alpha = startPoint;
            var totalFramesCount = Mathf.FloorToInt(duration * fps);
            var frameDuration = duration / totalFramesCount;
            var alphaStep = 1f / (startPoint < endPoint ? totalFramesCount : -totalFramesCount);
            for (int frame = 0; frame < totalFramesCount; frame++) {
                group.alpha += alphaStep;
                yield return new WaitForSeconds(frameDuration);
            }
            group.alpha = endPoint;
        }
    }
}
