using BeatLeader.Utils;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRUIControls;

namespace BeatLeader.Replayer.Tweaking {
    internal class RaycastBlockerTweak : GameTweak {
        public override bool CanBeInstalled => !InputUtils.IsInFPFC;

        public static int BlockingMask = 1 << 5;

        private static readonly HarmonyPatchDescriptor raycasterPatchDescriptor = new(
            typeof(VRGraphicRaycaster).GetConstructor(new Type[] { }), postfix:
            typeof(RaycastBlockerTweak).GetMethod(nameof(RaycasterConstructorPostfix), ReflectionUtils.StaticFlags));

        private HarmonyAutoPatch _raycasterConstructorPatch = null!;
        private static readonly Dictionary<VRGraphicRaycaster, LayerMask> _raycasters = new();

        public override void LateInitialize() {
            foreach (var raycaster in Resources.FindObjectsOfTypeAll<VRGraphicRaycaster>()) {
                PatchRaycaster(raycaster);
            }
        }

        public override void Initialize() {
            _raycasterConstructorPatch = raycasterPatchDescriptor;
        }

        public override void Dispose() {
            _raycasterConstructorPatch.Dispose();
            foreach (var pair in _raycasters.ToList()) {
                UnpatchRaycaster(pair.Key, pair.Value);
            }
            _raycasters.Clear();
        }

        private static void PatchRaycaster(VRGraphicRaycaster raycaster) {
            if (_raycasters.ContainsKey(raycaster)) return;
            var originalMask = GetMask(raycaster);
            SetMask(raycaster, BlockingMask);
            _raycasters.Add(raycaster, originalMask);
        }

        private static void UnpatchRaycaster(VRGraphicRaycaster raycaster, LayerMask mask) {
            SetMask(raycaster, mask);
            _raycasters.Remove(raycaster);
        }

        private static void SetMask(VRGraphicRaycaster raycaster, LayerMask mask) {
            raycaster.SetField("_blockingMask", mask);
        }

        private static LayerMask GetMask(VRGraphicRaycaster raycaster) {
            return raycaster.GetField<LayerMask, VRGraphicRaycaster>("_blockingMask");
        }

        private static void RaycasterConstructorPostfix(VRGraphicRaycaster __instance) {
            PatchRaycaster(__instance);
        }
    }
}
