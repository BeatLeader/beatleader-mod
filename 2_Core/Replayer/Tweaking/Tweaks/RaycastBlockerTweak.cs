using BeatLeader.Utils;
using IPA.Utilities;
using System.Collections.Generic;
using UnityEngine;
using VRUIControls;

namespace BeatLeader.Replayer.Tweaking {
    internal class RaycastBlockerTweak : GameTweak {
        public override bool CanBeInstalled => !InputUtils.IsInFPFC;

        public static int BlockingMask = 1 << 5;

        private readonly List<VRGraphicRaycaster> _raycasters = new();

        public override void LateInitialize() {
            var raycasters = Resources.FindObjectsOfTypeAll<VRGraphicRaycaster>();
            _raycasters.AddRange(raycasters);
            PatchRaycasters(raycasters, BlockingMask);
        }

        public override void Dispose() {
            _raycasters.Clear();
        }

        private void PatchRaycasters(IEnumerable<VRGraphicRaycaster> raycasters, int mask) {
            foreach (var raycaster in raycasters) {
                PatchRaycaster(raycaster, mask);
            }
        }

        private void PatchRaycaster(VRGraphicRaycaster raycaster, LayerMask mask) {
            raycaster.SetField("_blockingMask", mask);
        }
    }
}
