using BeatLeader.Models;
using BeatLeader.Utils;
using IPA.Utilities;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    internal class JumpDistanceTweak : GameTweak {
        [Inject] private readonly PlayerDataModel _playerData = null!; 
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        public override bool CanBeInstalled => _launchData.Settings.LoadPlayerJumpDistance;

        private static readonly HarmonyPatchDescriptor _movementDataInitPatchDescriptor = new(
            typeof(BeatmapObjectSpawnMovementData).GetMethod(nameof(
                BeatmapObjectSpawnMovementData.Init), ReflectionUtils.DefaultFlags), postfix:
            typeof(JumpDistanceTweak).GetMethod(nameof(
                MovementDataInitPostfix), ReflectionUtils.StaticFlags));

        private static float _desiredJumpDistance;

        private HarmonyAutoPatch _movementDataInitPatch = null!;

        public override void Initialize() {
            _desiredJumpDistance = _launchData.MainReplay.info.jumpDistance;
            _movementDataInitPatch = _movementDataInitPatchDescriptor;
        }

        public override void Dispose() {
            _movementDataInitPatch.Dispose();
        }

        private static void MovementDataInitPostfix(BeatmapObjectSpawnMovementData __instance) {
            __instance.SetField("_jumpDistance", _desiredJumpDistance);
        }
    }
}
