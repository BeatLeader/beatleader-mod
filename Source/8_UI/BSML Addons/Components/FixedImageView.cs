using HarmonyLib;
using HMUI;
using JetBrains.Annotations;

namespace BeatLeader.UI.BSML_Addons.Components {
    [HarmonyPatch(typeof(UnityEngine.UI.Image), "get_pixelsPerUnit")]
    public class FixedImageView : ImageView {
        [UsedImplicitly]
        private static void Postfix(UnityEngine.UI.Image __instance, ref float __result) {
            if (__instance is not FixedImageView) return;
            __result *= __instance.pixelsPerUnitMultiplier;
        }
    }
}