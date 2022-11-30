namespace BeatLeader.Replayer.Tweaking {
    internal class AudioTimeSyncControllerTweak : GameTweak {
        private static readonly HarmonyPatchDescriptor audioTimeSyncControllerUpdatePatchDecriptor = new(
            typeof(AudioTimeSyncController).GetMethod(nameof(AudioTimeSyncController.Update)),
            typeof(AudioTimeSyncControllerTweak).GetMethod(nameof(AudioTimeSyncControllerUpdatePrefix)));

        private HarmonyAutoPatch _harmonyPatch;

        public override void Initialize() {
            _harmonyPatch = new(audioTimeSyncControllerUpdatePatchDecriptor);
        }

        public override void Dispose() {
            _harmonyPatch.Dispose();
        }

        private static bool AudioTimeSyncControllerUpdatePrefix(AudioTimeSyncController __instance) {
            return !__instance.state.Equals(AudioTimeSyncController.State.Paused);
        }
    }
}
