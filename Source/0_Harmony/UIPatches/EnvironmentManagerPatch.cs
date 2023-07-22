using System;
using HarmonyLib;
using JetBrains.Annotations;

namespace BeatLeader.UIPatches {
    [HarmonyPatch(typeof(MenuEnvironmentManager), nameof(MenuEnvironmentManager.ShowEnvironmentType))]
    internal class EnvironmentManagerPatch {
        public static MenuEnvironmentManager.MenuEnvironmentType EnvironmentType {
            get => _environmentType;
            private set {
                _environmentType = value;
                EnvironmentTypeChangedEvent?.Invoke(value);
            }
        }

        public static event Action<MenuEnvironmentManager.MenuEnvironmentType>? EnvironmentTypeChangedEvent;

        private static MenuEnvironmentManager.MenuEnvironmentType _environmentType;
        
        [UsedImplicitly]
        private static void Postfix(MenuEnvironmentManager.MenuEnvironmentType menuEnvironmentType) {
            EnvironmentType = menuEnvironmentType;
        }
    }
}