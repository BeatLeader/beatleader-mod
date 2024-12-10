using HarmonyLib;
using VRUIControls;

namespace BeatLeader {
    [HarmonyPatch]
    internal static class VRRaycasterPatch {
        public static float RaycastDistance;
        public static bool OverrideRaycastDistance;

        [HarmonyPatch(typeof(PhysicsRaycasterWithCache), "Raycast"), HarmonyPrefix]
        private static void Prefix(ref float maxDistance) {
            if (OverrideRaycastDistance) {
                maxDistance = RaycastDistance;
            }
        }

        [HarmonyPatch(typeof(VRGraphicRaycaster), "RaycastCanvas"), HarmonyPrefix]
        private static void RaycastCanvasPrefix(ref float hitDistance) {
            if (OverrideRaycastDistance) {
                hitDistance = RaycastDistance;
            }
        }
    }
}