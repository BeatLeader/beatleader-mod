using System;
using System.Collections.Generic;
using IPA.Utilities;
using UnityEngine;
using Zenject;
using BeatLeader.Interop;
using BeatLeader.Utils;
using BeatLeader.Models;
using System.Reflection;
using System.Linq;

namespace BeatLeader.Replayer
{
    public class BeatmapTimeController : MonoBehaviour, IBeatmapTimeController
    {
        #region Injection

        [Inject] private readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        [Inject] private readonly AudioTimeSyncController _audioTimeSyncController;

        [Inject] private readonly BeatmapCallbacksController.InitData _beatmapCallbacksControllerInitData;
        [Inject] private readonly BeatmapCallbacksController _beatmapCallbacksController;
        [Inject] private readonly BeatmapCallbacksUpdater _beatmapCallbacksUpdater;

        //[FirstResource] private BombCutSoundEffectManager _bombCutSoundEffectManager;
        [FirstResource] private AudioManagerSO _audioManagerSO;

        #endregion

        #region Time, EndTime, SpeedMultiplier

        public float SongSpeedMultiplier => _audioTimeSyncController.timeScale;
        public float SongTime => _audioTimeSyncController.songTime;
        public float SongEndTime => _audioTimeSyncController.songEndTime;
        public float SongStartTime => _audioTimeSyncController
            .GetField<float, AudioTimeSyncController>("_startSongTime");

        #endregion

        #region Events

        public event Action<float> SongSpeedChangedEvent;
        public event Action<float> SongRewindEvent;

        #endregion

        #region Setup

        private MemoryPoolContainer<NoteCutSoundEffect> _noteCutSoundPoolContainer;
        //private BombCutSoundEffect.Pool _bombCutSoundPool;
        private List<IBeatmapObjectController> _spawnedBeatmapObjectControllers;
        private Dictionary<float, CallbacksInTime> _callbacksInTimes;
        private AudioSource _beatmapAudioSource;

        private void Start()
        {
            this.LoadResources();

            _beatmapAudioSource = _audioTimeSyncController
                .GetField<AudioSource, AudioTimeSyncController>("_audioSource");
            _spawnedBeatmapObjectControllers = _beatmapObjectManager
                .GetField<List<IBeatmapObjectController>, BeatmapObjectManager>("_allBeatmapObjects");
            _callbacksInTimes = _beatmapCallbacksController
                .GetField<Dictionary<float, CallbacksInTime>, BeatmapCallbacksController>("_callbacksInTimes");
            //thats why i can't move it to Awake instead of Start
            _noteCutSoundPoolContainer = _noteCutSoundEffectManager
                .GetField<MemoryPoolContainer<NoteCutSoundEffect>, NoteCutSoundEffectManager>("_noteCutSoundEffectPoolContainer");

            _despawnNoteMethod = typeof(BeatmapObjectManager)
                .GetMethod("Despawn", ReflectionUtils.DefaultFlags, null, new Type[] { typeof(NoteController) }, null);
            _despawnSliderMethod = typeof(BeatmapObjectManager)
                .GetMethod("Despawn", ReflectionUtils.DefaultFlags, null, new Type[] { typeof(SliderController) }, null);
            _despawnObstacleMethod = typeof(BeatmapObjectManager)
                .GetMethod("Despawn", ReflectionUtils.DefaultFlags, null, new Type[] { typeof(ObstacleController) }, null);
        }

        #endregion

        #region Rewind

        public void Rewind(float time, bool resumeAfterRewind = true)
        {
            if (Math.Abs(time - SongTime) < 0.001f) return;
            time = Mathf.Clamp(time, SongStartTime, SongEndTime);

            bool wasPausedBeforeRewind = _audioTimeSyncController
                .state.Equals(AudioTimeSyncController.State.Paused);
            if (!wasPausedBeforeRewind) _audioTimeSyncController.Pause();

            _beatmapCallbacksUpdater.Pause();

            DespawnAllNoteControllerSounds();
            DespawnAllBeatmapObjects();

            _audioTimeSyncController.SetField("_prevAudioSamplePos", -1);
            _audioTimeSyncController.SeekTo((time - SongStartTime) / _audioTimeSyncController.timeScale);

            //_beatmapCallbacksControllerInitData.SetField("startFilterTime", time);
            _beatmapCallbacksController.SetField("_startFilterTime", time);
            _beatmapCallbacksController.SetField("_prevSongTime", float.MinValue);
            foreach (var callback in _callbacksInTimes) callback.Value.lastProcessedNode = null;

            NoodleExtensionsInterop.RequestReprocess();
            SongRewindEvent?.Invoke(time);

            if (!wasPausedBeforeRewind && resumeAfterRewind) 
                _audioTimeSyncController.Resume();
            _beatmapCallbacksUpdater.Resume();
        }

        #endregion

        #region ChangeSpeed

        public void SetSpeedMultiplier(float speedMultiplier, bool resumeAfterSpeedChange = true)
        {
            if (Math.Abs(speedMultiplier - _audioTimeSyncController.timeScale) < 0.001f) return;

            bool wasPausedBeforeRewind = _audioTimeSyncController
                .state.Equals(AudioTimeSyncController.State.Paused);
            if (!wasPausedBeforeRewind) _audioTimeSyncController.Pause();

            DespawnAllNoteControllerSounds();
            _audioTimeSyncController.SetField("_timeScale", speedMultiplier);
            _beatmapAudioSource.pitch = speedMultiplier;
            _audioManagerSO.musicPitch = 1f / speedMultiplier;

            SongSpeedChangedEvent?.Invoke(speedMultiplier);

            if (!wasPausedBeforeRewind && resumeAfterSpeedChange) 
                _audioTimeSyncController.Resume();
        }

        #endregion

        #region Despawn

        private MethodInfo _despawnNoteMethod;
        private MethodInfo _despawnSliderMethod;
        private MethodInfo _despawnObstacleMethod;

        private void DespawnAllBeatmapObjects()
        {
            foreach (var item in _spawnedBeatmapObjectControllers.ToList())
            {
                var param = new object[] { item };

                NoteController note = item as NoteController;
                if (note != null)
                {
                    _despawnNoteMethod.Invoke(_beatmapObjectManager, param);
                    continue;
                }

                SliderController slider = item as SliderController;
                if (slider != null)
                {
                    _despawnSliderMethod.Invoke(_beatmapObjectManager, param);
                    continue;
                }

                ObstacleController obstacle = item as ObstacleController;
                if (obstacle != null)
                {
                    _despawnObstacleMethod.Invoke(_beatmapObjectManager, param);
                    continue;
                }
            }
        }
        private void DespawnAllNoteControllerSounds()
        {
            //don't have any sense because we can't access spawned members
            //_bombCutSoundPool.Clear();
            _noteCutSoundPoolContainer.activeItems.ForEach(x => x.StopPlayingAndFinish());
            _noteCutSoundEffectManager.SetField("_prevNoteATime", -1f);
            _noteCutSoundEffectManager.SetField("_prevNoteBTime", -1f);
        }

        #endregion 
    }
}