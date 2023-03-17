using BeatLeader.Interop;
using BeatLeader.Models;
using BeatLeader.Utils;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    internal class JumpDistanceTweak : GameTweak {
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        public override bool CanBeInstalled => _launchData.Settings.LoadPlayerJumpDistance;

        private static readonly HarmonyPatchDescriptor _movementDataInitPatchDescriptor = new(
            typeof(BeatmapObjectSpawnMovementData).GetMethod(nameof(
                BeatmapObjectSpawnMovementData.Init), ReflectionUtils.DefaultFlags),
            typeof(JumpDistanceTweak).GetMethod(nameof(
                MovementDataInitPrefix), ReflectionUtils.StaticFlags));

        private static float _desiredJumpDistance;

        private HarmonyAutoPatch _movementDataInitPatch = null!;

        public override void Initialize() {
            JDFixerInterop.Enabled = false;
            _desiredJumpDistance = _launchData.MainReplay.info.jumpDistance;
            _movementDataInitPatch = _movementDataInitPatchDescriptor;
        }

        public override void Dispose() {
            _movementDataInitPatch.Dispose();
            JDFixerInterop.Enabled = true;
        }

        private static void MovementDataInitPrefix(ref float startNoteJumpMovementSpeed, ref BeatmapObjectSpawnMovementData.NoteJumpValueType noteJumpValueType, ref float noteJumpValue) {
            noteJumpValueType = BeatmapObjectSpawnMovementData.NoteJumpValueType.JumpDuration;
            noteJumpValue = _desiredJumpDistance / startNoteJumpMovementSpeed / 2;
            Plugin.Log.Notice($"[JumpDistanceTweak] Applied JD: {_desiredJumpDistance} | NJS: {startNoteJumpMovementSpeed} | NJV: {noteJumpValue}");
        }
    }
}
