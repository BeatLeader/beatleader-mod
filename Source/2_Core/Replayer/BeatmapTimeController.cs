using System;
using System.Collections.Generic;
using IPA.Utilities;
using UnityEngine;
using Zenject;
using BeatLeader.Utils;
using BeatLeader.Models;
using System.Reflection;
using System.Linq;
using JetBrains.Annotations;

namespace BeatLeader.Replayer {
    internal class BeatmapTimeController : MonoBehaviour, IBeatmapTimeController {
        #region Injection

        [Inject] protected readonly BeatmapObjectManager _beatmapObjectManager = null!;
        [Inject] protected readonly NoteCutSoundEffectManager _noteCutSoundEffectManager = null!;
        [Inject] protected readonly AudioTimeSyncController _audioTimeSyncController = null!;
        [Inject] protected readonly SongSpeedData _speedData = null!;
        [Inject] protected readonly IReadonlyBeatmapData _beatmapData = null!;

        [Inject] protected readonly BeatmapCallbacksController.InitData _beatmapCallbacksControllerInitData = null!;
        [Inject] protected readonly BeatmapCallbacksController _beatmapCallbacksController = null!;
        [Inject] protected readonly BeatmapCallbacksUpdater _beatmapCallbacksUpdater = null!;
        [Inject] protected readonly AudioManager _audioManager = null!;

        #endregion

        #region Time, EndTime, SpeedMultiplier

        public float SongEndTime => _audioTimeSyncController.songEndTime;
        public float SongStartTime => _audioTimeSyncController
            .GetField<float, AudioTimeSyncController>("_startSongTime");
        public float SongTime => _audioTimeSyncController.songTime;
        public float SongStartSpeedMultiplier => _speedData.speedMul;
        public float SongSpeedMultiplier => _audioTimeSyncController.timeScale;

        #endregion

        #region Events

        public event Action<float>? SongSpeedWasChangedEvent;
        public event Action<float>? EarlySongWasRewoundEvent;
        public event Action<float>? SongWasRewoundEvent;

        #endregion

        #region Setup

        private static MemoryPoolContainer<NoteCutSoundEffect> _noteCutSoundPoolContainer = null!;
        private Dictionary<float, CallbacksInTime> _callbacksInTimes = null!;
        private List<IBeatmapObjectController> _spawnedBeatmapObjectControllers = null!;
        private AudioSource _beatmapAudioSource = null!;

        private readonly HarmonyAutoPatch _fetchCutSoundPoolPatch = new HarmonyPatchDescriptor(
            typeof(NoteCutSoundEffectManager).GetMethod(nameof(
                NoteCutSoundEffectManager.Start), ReflectionUtils.DefaultFlags)!, postfix:
            typeof(BeatmapTimeController).GetMethod(nameof(
                NoteCutSoundEffectManagerStartPostfix), ReflectionUtils.StaticFlags));

        private void Awake() {
            _beatmapAudioSource = _audioTimeSyncController
                .GetField<AudioSource, AudioTimeSyncController>("_audioSource");
            _spawnedBeatmapObjectControllers = _beatmapObjectManager
                .GetField<List<IBeatmapObjectController>, BeatmapObjectManager>("_allBeatmapObjects");
            _callbacksInTimes = _beatmapCallbacksController
                .GetField<Dictionary<float, CallbacksInTime>, BeatmapCallbacksController>("_callbacksInTimes");
            _audioTimeSyncController.Start();
        }

        private void OnDestroy() {
            _soundSpawnerSilencer.Dispose();
            _fetchCutSoundPoolPatch.Dispose();
            _noteCutSoundPoolContainer = null!;
        }

        private static void NoteCutSoundEffectManagerStartPostfix(NoteCutSoundEffectManager __instance) {
            _noteCutSoundPoolContainer = __instance.GetField<
                MemoryPoolContainer<NoteCutSoundEffect>, NoteCutSoundEffectManager>("_noteCutSoundEffectPoolContainer");
        }

        #endregion

        #region Rewind

        public virtual void Rewind(float time, bool resumeAfterRewind = true) {
            if (Math.Abs(time - SongTime) < 0.001f || float.IsInfinity(time)
                || float.IsNaN(time) || !_audioTimeSyncController.isReady) return;
            time = Mathf.Clamp(time, SongStartTime, SongEndTime);

            EarlySongWasRewoundEvent?.Invoke(time);

            var wasPausedBeforeRewind = _audioTimeSyncController
                .state.Equals(AudioTimeSyncController.State.Paused);
            if (!wasPausedBeforeRewind) _audioTimeSyncController.Pause();

            _beatmapCallbacksUpdater.Pause();
            _soundSpawnerSilencer.Enabled = true;
            DespawnAllNoteControllerSounds();
            DespawnAllBeatmapObjects();

            _audioTimeSyncController.SetField("_prevAudioSamplePos", -1);
            _audioTimeSyncController.SeekTo((time - SongStartTime) / _audioTimeSyncController.timeScale);
            _beatmapCallbacksController.SetField("_prevSongTime", float.MinValue);
            foreach (var pair in _callbacksInTimes) {
                pair.Value.lastProcessedNode = FindBeatmapItem(time);
            }

            if (!wasPausedBeforeRewind && resumeAfterRewind)
                _audioTimeSyncController.Resume();

            _beatmapCallbacksUpdater.LateUpdate();
            _beatmapCallbacksUpdater.Resume();
            _soundSpawnerSilencer.Enabled = false;

            SongWasRewoundEvent?.Invoke(time);
        }

        private LinkedListNode<BeatmapDataItem>? FindBeatmapItem(float time) {
            LinkedListNode<BeatmapDataItem>? item = null;
            for (var node = _beatmapData.allBeatmapDataItems.First; node != null; node = node.Next) {
                var nodeTime = node.Value.time;
                var filterTime = _beatmapCallbacksControllerInitData.startFilterTime;
                if (nodeTime >= filterTime && nodeTime >= time) break;
                item = node;
            }
            return item;
        }

        #endregion

        #region Change Speed

        public virtual void SetSpeedMultiplier(float speedMultiplier, bool resumeAfterSpeedChange = true) {
            if (Math.Abs(speedMultiplier - _audioTimeSyncController.timeScale) < 0.001f) return;

            var wasPausedBeforeRewind = _audioTimeSyncController
                .state.Equals(AudioTimeSyncController.State.Paused);
            if (!wasPausedBeforeRewind) _audioTimeSyncController.Pause();

            DespawnAllNoteControllerSounds();
            _audioTimeSyncController.SetField("_timeScale", speedMultiplier);
            _beatmapAudioSource.pitch = speedMultiplier;
            _audioManager.musicPitch = 1f / speedMultiplier;

            SongSpeedWasChangedEvent?.Invoke(speedMultiplier);

            if (!wasPausedBeforeRewind && resumeAfterSpeedChange)
                _audioTimeSyncController.Resume();
        }

        #endregion

        #region Despawn

        private readonly HarmonySilencer _soundSpawnerSilencer = new(
            typeof(NoteCutSoundEffectManager).GetMethod(nameof(
                    NoteCutSoundEffectManager.HandleNoteWasSpawned),
                ReflectionUtils.DefaultFlags)!, false);

        private static readonly MethodInfo despawnNoteMethod =
            typeof(BeatmapObjectManager).GetMethod("Despawn",
                ReflectionUtils.DefaultFlags, new Type[] { typeof(NoteController) });

        private static readonly MethodInfo despawnSliderMethod =
            typeof(BeatmapObjectManager).GetMethod("Despawn",
                ReflectionUtils.DefaultFlags, new Type[] { typeof(SliderController) });

        private static readonly MethodInfo despawnObstacleMethod =
            typeof(BeatmapObjectManager).GetMethod("Despawn",
                ReflectionUtils.DefaultFlags, new Type[] { typeof(ObstacleController) });

        protected void DespawnAllBeatmapObjects() {
            var param = new object[1];
            foreach (var item in _spawnedBeatmapObjectControllers.ToList()) {
                param[0] = item;
                //TODO: potential bug
                item.Pause(false);
                (item switch {
                    NoteController => despawnNoteMethod,
                    SliderController => despawnSliderMethod,
                    ObstacleController => despawnObstacleMethod,
                    _ => null
                })?.Invoke(_beatmapObjectManager, param);
            }
        }

        protected void DespawnAllNoteControllerSounds() {
            _noteCutSoundPoolContainer?.activeItems.ForEach(x => x.StopPlayingAndFinish());
            _noteCutSoundEffectManager.SetField("_prevNoteATime", -1f);
            _noteCutSoundEffectManager.SetField("_prevNoteBTime", -1f);
        }

        #endregion
    }
}