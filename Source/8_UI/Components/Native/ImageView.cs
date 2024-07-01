using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine.UI;

namespace BeatLeader.Components {
    [HarmonyPatch(typeof(Image), "get_pixelsPerUnit")]
    internal class AdvancedImageView : HMUI.ImageView {
        [UsedImplicitly]
        private static void Postfix(Image __instance, ref float __result) {
            if (__instance is not AdvancedImageView) return;
            __result *= __instance.pixelsPerUnitMultiplier;
        }
    }
}