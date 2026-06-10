using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using HMUI;
using JetBrains.Annotations;

namespace BeatLeader {
    [HarmonyPatch]
    internal class MenuStatusPatch {
        [HarmonyPatch(typeof(MainFlowCoordinator), "DidActivate"), HarmonyPostfix]
        [UsedImplicitly]
        private static void MainMenuActivated(bool addedToHierarchy) {
            if (addedToHierarchy) ReplaySocket.SendStatus("MainMenu");
        }

        private static MethodInfo FindConcreteMethod(Type type, string name) {
            var t = type;
            while (t != null) {
                var m = t.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (m != null && !m.IsAbstract) return m;
                t = t.BaseType;
            }
            return null;
        }

        public static void ApplyManualPatches(Harmony harmony) {
            var activatePostfix = new HarmonyMethod(typeof(MenuStatusPatch), nameof(DidActivatePostfix));
            var deactivatePostfix = new HarmonyMethod(typeof(MenuStatusPatch), nameof(DidDeactivatePostfix));

            var types = new[] {
                typeof(SoloFreePlayFlowCoordinator),
                typeof(GameServerLobbyFlowCoordinator),
                typeof(CampaignFlowCoordinator)
            };

            var patched = new HashSet<MethodInfo>();
            foreach (var type in types) {
                var activate = FindConcreteMethod(type, "DidActivate");
                if (activate != null && patched.Add(activate))
                    harmony.Patch(activate, postfix: activatePostfix);

                var deactivate = FindConcreteMethod(type, "DidDeactivate");
                if (deactivate != null && patched.Add(deactivate))
                    harmony.Patch(deactivate, postfix: deactivatePostfix);
            }
        }

        private static void DidActivatePostfix(FlowCoordinator __instance, bool __1) {
            if (!__1) return;
            if (__instance is SoloFreePlayFlowCoordinator)
                ReplaySocket.SendStatus("SoloSongSelection");
            else if (__instance is GameServerLobbyFlowCoordinator)
                ReplaySocket.SendStatus("MultiplayerLobby");
            else if (__instance is CampaignFlowCoordinator)
                ReplaySocket.SendStatus("Campaign");
        }

        private static void DidDeactivatePostfix(FlowCoordinator __instance, bool __0) {
            if (!__0) return;
            if (__instance is SoloFreePlayFlowCoordinator
                || __instance is GameServerLobbyFlowCoordinator
                || __instance is CampaignFlowCoordinator) {
                ReplaySocket.SendStatus("MainMenu");
            }
        }
    }
}
