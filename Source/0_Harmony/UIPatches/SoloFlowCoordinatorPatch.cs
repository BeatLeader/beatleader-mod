using System;
using HarmonyLib;

namespace BeatLeader {
    [HarmonyPatch]
    internal static class SoloFlowCoordinatorPatch {
        public static event Action? PresentedEvent;
        public static event Action? DismissedEvent;
        
        [HarmonyPatch(typeof(SoloFreePlayFlowCoordinator), "SinglePlayerLevelSelectionFlowCoordinatorDidActivate")]
        [HarmonyPostfix]
        private static void PresentPostfix() {
            PresentedEvent?.Invoke();
        }
        
        [HarmonyPatch(typeof(SoloFreePlayFlowCoordinator), "SinglePlayerLevelSelectionFlowCoordinatorDidDeactivate")]
        [HarmonyPostfix]
        private static void DismissPostfix() {
            DismissedEvent?.Invoke();
        }
    }
}