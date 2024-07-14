using BeatLeader.Utils;
using JetBrains.Annotations;

namespace BeatLeader.Replayer.Tweaking {
    internal class ScoringTweak : GameTweak {
        private static readonly HarmonyPatchDescriptor patchDescriptor = new(
            ReflectionUtils.GetMethod<GoodCutScoringElement>("Init")!,
            ReflectionUtils.GetMethod<ScoringTweak>("Prefix")
        );

        private HarmonyAutoPatch _patch = null!;

        public override void Initialize() {
            _patch = patchDescriptor;
        }

        public override void Dispose() {
            _patch.Dispose();
        }

        [UsedImplicitly]
        private static void Prefix(ref CutScoreBuffer ____cutScoreBuffer) {
            ____cutScoreBuffer = new();
        }
    }
}