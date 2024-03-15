using HarmonyLib;
using JetBrains.Annotations;

namespace BeatLeader.UI.Reactive.Components {
    [HarmonyPatch(typeof(UnityEngine.UI.Image), "get_pixelsPerUnit")]
    internal class FixedImageView : HMUI.ImageView {
        [UsedImplicitly]
        private static void Postfix(UnityEngine.UI.Image __instance, ref float __result) {
            if (__instance is not FixedImageView) return;
            __result *= __instance.pixelsPerUnitMultiplier;
        }
    }
}