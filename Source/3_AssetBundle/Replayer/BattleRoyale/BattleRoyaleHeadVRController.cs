using UnityEngine;

namespace BeatLeader {
    internal class BattleRoyaleHeadVRController : BattleRoyaleVRController {
        [SerializeField]
        private MeshRenderer headRenderer = null!;

        protected override void ApplyBlock(MaterialPropertyBlock block) {
            headRenderer.SetPropertyBlock(block);
        }
    }
}