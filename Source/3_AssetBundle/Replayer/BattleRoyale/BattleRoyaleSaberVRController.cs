using UnityEngine;

namespace BeatLeader {
    internal class BattleRoyaleSaberVRController : BattleRoyaleVRController {
        [SerializeField]
        private MeshRenderer bladeRenderer = null!;

        [SerializeField]
        private MeshRenderer handleRenderer = null!;

        protected override void ApplyBlock(MaterialPropertyBlock block) {
            bladeRenderer.SetPropertyBlock(block);
            handleRenderer.SetPropertyBlock(block);
        }
    }
}