using System;
using System.Collections;
using System.Collections.Generic;
using IPA.Utilities;
using UnityEngine;
using Zenject;
using BeatLeader.Interop;
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
        [Inject] protected readonly BeatmapObjectSpawnController _beatmapObjectSpawnController = null!;
        [Inject] protected readonly IVariableMovementDataProvider _variableMovementDataProvider = null!;
        [Inject] protected readonly DiContainer _container = null!;
        [Inject] protected readonly PlayerTransforms _playerTransforms = null!;

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
            var replayCustomEvents = HeckRewindInterop.NotifyRewind(_container);

            var wasPausedBeforeRewind = _audioTimeSyncController
                .state.Equals(IAudioTimeSource.State.Paused);
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
            if (replayCustomEvents) ReplayCustomEvents(time);

            if (!wasPausedBeforeRewind && resumeAfterRewind)
                _audioTimeSyncController.Resume();

            _beatmapCallbacksUpdater.LateUpdate();
            SpawnPassedObjects(time);
            _beatmapCallbacksUpdater.Resume();
            _soundSpawnerSilencer.Enabled = false;

            SongWasRewoundEvent?.Invoke(time);
            if (replayCustomEvents) CatchUpPrefabs(time);
            StartCoroutine(ClearTrailsCoroutine());
        }

        private Coroutine? _resumeParticlesCoroutine;

        private void CatchUpPrefabs(float time) {
            if (_resumeParticlesCoroutine != null) StopCoroutine(_resumeParticlesCoroutine);
            var particles = VivifyPrefabsCatchUp.CatchUp(VivifyInterop.GetPrefabs(_container), time);
            _resumeParticlesCoroutine = StartCoroutine(ResumeParticlesCoroutine(particles, time));
        }

        private IEnumerator ResumeParticlesCoroutine(List<ParticleSystem> particles, float time) {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            VivifyPrefabsCatchUp.Resume(particles, SongTime - time);
            _resumeParticlesCoroutine = null;
        }

        private static IEnumerator ClearTrailsCoroutine() {
            for (var i = 0; i < 2; i++) {
                yield return null;
                foreach (var trail in FindObjectsByType<TrailRenderer>(FindObjectsSortMode.None)) {
                    trail.Clear();
                }
            }
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

        private void ReplayCustomEvents(float time) {
            var eventType = HeckRewindInterop.CustomEventDataType;
            _beatmapCallbacksController._songTime = time;
            for (var node = _beatmapData.allBeatmapDataItems.First; node != null && node.Value.time < time; node = node.Next) {
                if (!eventType.IsInstanceOfType(node.Value)) continue;
                foreach (var callbacks in _callbacksInTimes.Values) {
                    callbacks.CallCallbacks(node.Value);
                }
            }
        }

        #endregion

        #region Spawn

        protected virtual bool ShouldRespawnNote(NoteData noteData) => true;

        protected virtual bool TryGetHeadPose(float time, out Pose pose) {
            pose = default;
            return false;
        }

        // Walls and notes stop following the head after their avoided or missed mark and keep the head position from that moment
        private bool MoveHeadToMark(float markTime, float time) {
            if (markTime >= time || !TryGetHeadPose(markTime, out var pose)) return false;
            _playerTransforms._headTransform.SetLocalPose(pose);
            _playerTransforms.Update();
            return true;
        }

        private void SpawnPassedObjects(float time) {
            var spawnedObjectDatas = new HashSet<BeatmapObjectData>();
            foreach (var controller in _spawnedBeatmapObjectControllers) {
                BeatmapObjectData? data = controller switch {
                    NoteController note => note.noteData,
                    ObstacleController obstacle => obstacle.obstacleData,
                    SliderController slider => slider.sliderData,
                    _ => null
                };
                if (data != null) spawnedObjectDatas.Add(data);
            }

            var headTransform = _playerTransforms._headTransform;
            var headPose = headTransform.GetLocalPose();
            var headMoved = false;

            var filterTime = _beatmapCallbacksControllerInitData.startFilterTime;
            for (var node = _beatmapData.allBeatmapDataItems.First; node != null && node.Value.time < time; node = node.Next) {
                if (node.Value is not BeatmapObjectData data || data.time < filterTime || spawnedObjectDatas.Contains(data)) continue;

                var halfJumpDuration = NoodleExtensionsInterop.TryGetSpawnAheadTime(data, out var aheadTime)
                    ? aheadTime - _variableMovementDataProvider.moveDuration
                    : _variableMovementDataProvider.halfJumpDuration;
                switch (data) {
                    case NoteData note when note.time + halfJumpDuration > time && ShouldRespawnNote(note):
                        headMoved |= MoveHeadToMark(note.time + NoteJump.kMissedTimeOffset, time);
                        _beatmapObjectSpawnController._noteDataCallbackWrapper.CallCallback(note);
                        break;
                    case ObstacleData obstacle when obstacle.time + obstacle.duration + halfJumpDuration > time:
                        headMoved |= MoveHeadToMark(obstacle.time + obstacle.duration + ObstacleController.kAvoidMarkTimeOffset, time);
                        _beatmapObjectSpawnController._obstacleDataCallbackWrapper.CallCallback(obstacle);
                        break;
                    case SliderData { sliderType: SliderData.Type.Burst } chain when chain.tailTime + halfJumpDuration > time:
                        headMoved |= MoveHeadToMark(chain.time + NoteJump.kMissedTimeOffset, time);
                        SpawnPassedChain(chain);
                        break;
                    case SliderData slider when slider.tailTime + halfJumpDuration * 0.5f > time:
                        _beatmapObjectSpawnController._sliderDataCallbackWrapper.CallCallback(slider);
                        break;
                }
            }

            if (headMoved) {
                headTransform.SetLocalPose(headPose);
                _playerTransforms.Update();
            }
        }

        private void SpawnPassedChain(SliderData chain) {
            var count = _spawnedBeatmapObjectControllers.Count;
            _beatmapObjectSpawnController._sliderDataCallbackWrapper.CallCallback(chain);
            for (var i = _spawnedBeatmapObjectControllers.Count - 1; i >= count; i--) {
                if (_spawnedBeatmapObjectControllers[i] is not NoteController note || ShouldRespawnNote(note.noteData)) continue;
                _beatmapObjectManager.Despawn(note);
            }
        }

        #endregion

        #region Change Speed

        public virtual void SetSpeedMultiplier(float speedMultiplier, bool resumeAfterSpeedChange = true) {
            if (Math.Abs(speedMultiplier - _audioTimeSyncController.timeScale) < 0.001f) return;

            var wasPausedBeforeRewind = _audioTimeSyncController
                .state.Equals(IAudioTimeSource.State.Paused);
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