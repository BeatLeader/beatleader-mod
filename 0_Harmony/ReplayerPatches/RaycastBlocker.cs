using System.Collections.Generic;
using HarmonyLib;
using IPA.Utilities;
using UnityEngine;
using VRUIControls;

namespace BeatLeader {
    public static class RaycastBlocker {
        public static LayerMask BlockerMask {
            get => _blockerMask;
            set {
                _blockerMask = value;
                UpdateMasks();
            }
        }
        public static bool EnableBlocker {
            get => _enableBlocker;
            set {
                _enableBlocker = value;
                UpdateMasks();
            }
        }

        private static Dictionary<VRGraphicRaycaster, LayerMask> _raycasters = new();
        private static HashSet<(VRGraphicRaycaster, LayerMask)> _modificationCache = new();
        private static int _blockerMask = -1;
        private static bool _lastBlockerState;
        private static bool _enableBlocker;

        public static void ReleaseMemory() {
            _raycasters.Clear();
        }
        private static void UpdateMasks() {
            var enable = EnableBlocker;
            foreach (var raycaster in _raycasters) {
                if (enable && !_lastBlockerState)
                    _modificationCache.Add((raycaster.Key, GetMask(raycaster.Key)));
                SetMask(raycaster.Key, enable ? BlockerMask : raycaster.Value);
            }

            foreach (var item in _modificationCache) {
                _raycasters[item.Item1] = item.Item2;
            }
            _modificationCache.Clear();
            _lastBlockerState = enable;
        }
        private static void SetMask(VRGraphicRaycaster raycaster) {
            SetMask(raycaster, BlockerMask);
        }
        private static void SetMask(VRGraphicRaycaster raycaster, LayerMask mask) {
            raycaster.SetField("_blockingMask", mask);
        }
        private static LayerMask GetMask(VRGraphicRaycaster raycaster) {
            return raycaster.GetField<LayerMask, VRGraphicRaycaster>("_blockingMask");
        }

        [HarmonyPatch(typeof(VRGraphicRaycaster), MethodType.Constructor)]
        private static class RaycastersGrabber {
            [HarmonyPostfix]
            private static void GrabRaycaster(VRGraphicRaycaster __instance) {
                if (!_raycasters.ContainsKey(__instance))
                    _raycasters.Add(__instance, GetMask(__instance));
                SetMask(__instance);
            }
        }
    }
}
