using BeatLeader.Interop;
using BeatLeader.Models;
using BeatLeader.Utils;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    internal class JumpDistanceTweak : GameTweak {
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        public override bool CanBeInstalled => _launchData.Settings.LoadPlayerJumpDistance;

        private static readonly HarmonyPatchDescriptor _movementDataInitPatchDescriptor = new(
            typeof(VariableMovementDataProvider).GetMethod(nameof(
                VariableMovementDataProvider.Init), ReflectionUtils.DefaultFlags)!,
            typeof(JumpDistanceTweak).GetMethod(nameof(
                MovementDataInitPrefix), ReflectionUtils.StaticFlags));

        private static float _desiredJumpDistance;

        private HarmonyAutoPatch _movementDataInitPatch = null!;

        public override void Initialize() {
            //TODO: add br (player switch) support
            JDFixerInterop.Enabled = false;
            _desiredJumpDistance = _launchData.MainReplay.ReplayData.JumpDistance;
            _movementDataInitPatch = _movementDataInitPatchDescriptor;
        }

        public override void Dispose() {
            _movementDataInitPatch.Dispose();
            JDFixerInterop.Enabled = true;
        }

        private static void MovementDataInitPrefix(ref float noteJumpMovementSpeed, ref BeatmapObjectSpawnMovementData.NoteJumpValueType noteJumpValueType, ref float noteJumpValue) {
            if (_desiredJumpDistance == 0) return;
            noteJumpValueType = BeatmapObjectSpawnMovementData.NoteJumpValueType.JumpDuration;
            noteJumpValue = _desiredJumpDistance / noteJumpMovementSpeed / 2;
            Plugin.Log.Notice($"[JumpDistanceTweak] Applied JD: {_desiredJumpDistance} | NJS: {noteJumpMovementSpeed} | NJV: {noteJumpValue}");
        }
    }
}
