using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    internal class AudioTimeSyncControllerTweak : GameTweak {
        private static readonly HarmonyPatchDescriptor audioTimeSyncControllerUpdatePatchDecriptor = new(
            typeof(AudioTimeSyncController).GetMethod(nameof(AudioTimeSyncController.Update), ReflectionUtils.DefaultFlags),
            typeof(AudioTimeSyncControllerTweak).GetMethod(nameof(AudioTimeSyncControllerUpdatePrefix), ReflectionUtils.StaticFlags));

        private HarmonyAutoPatch _harmonyPatch = null!;

        public override void Initialize() {
            _harmonyPatch = audioTimeSyncControllerUpdatePatchDecriptor;
        }

        public override void Dispose() {
            _harmonyPatch.Dispose();
        }

        private static bool AudioTimeSyncControllerUpdatePrefix(AudioTimeSyncController __instance) {
            return !__instance.state.Equals(AudioTimeSyncController.State.Paused);
        }
    }
}
