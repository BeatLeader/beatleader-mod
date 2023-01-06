using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    internal class AudioTimeSyncControllerTweak : GameTweak {
        [Inject] private readonly IReplayTimeController _timeController = null!;
        [Inject] private readonly IReplayPauseController _pauseController = null!;
        [Inject] private readonly IReplayFinishController _finishController = null!;

        private static readonly HarmonyPatchDescriptor audioTimeSyncControllerUpdatePatchDecriptor = new(
            typeof(AudioTimeSyncController).GetMethod(nameof(AudioTimeSyncController.Update), ReflectionUtils.DefaultFlags),
            typeof(AudioTimeSyncControllerTweak).GetMethod(nameof(AudioTimeSyncControllerUpdatePrefix), ReflectionUtils.StaticFlags));

        private HarmonyAutoPatch _harmonyPatch = null!;
        private bool _wasFinished;

        public override void Initialize() {
            _finishController.ReplayWasFinishedEvent += HandleReplayFinished;
            _timeController.EarlySongWasRewoundEvent += HandleSongRewound;
            _harmonyPatch = audioTimeSyncControllerUpdatePatchDecriptor;
        }

        public override void Dispose() {
            _harmonyPatch.Dispose();
            _finishController.ReplayWasFinishedEvent -= HandleReplayFinished;
            _timeController.EarlySongWasRewoundEvent -= HandleSongRewound;
        }

        private void HandleReplayFinished() {
            if (_finishController.ExitAutomatically) return;
            _pauseController.Pause();
            _pauseController.LockUnpause = true;
            _wasFinished = true;
        }

        private void HandleSongRewound(float time) {
            if (_wasFinished) {
                _pauseController.LockUnpause = false;
                _wasFinished = false;
            }
        }

        private static bool AudioTimeSyncControllerUpdatePrefix(AudioTimeSyncController __instance) {
            return !__instance.state.Equals(AudioTimeSyncController.State.Paused);
        }
    }
}
