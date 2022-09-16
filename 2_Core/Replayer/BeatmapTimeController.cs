using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Utils;
using IPA.Loader;
using IPA.Utilities;
using HarmonyLib;
using UnityEngine;
using Zenject;
using System.Management;
using BeatLeader.Interop;

namespace BeatLeader.Replayer
{
    public class BeatmapTimeController : MonoBehaviour
    {
        [Inject] private readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        [Inject] private readonly AudioTimeSyncController _audioTimeSyncController;

        [Inject] private readonly BeatmapCallbacksController.InitData _beatmapCallbacksControllerInitData;
        [Inject] private readonly BeatmapCallbacksController _beatmapCallbacksController;
        [Inject] private readonly BeatmapCallbacksUpdater _beatmapCallbacksUpdater;

        public event Action<float> OnSongSpeedChanged;
        public event Action<float> OnSongRewind;

        private MemoryPoolContainer<NoteCutSoundEffect> _noteCutSoundPoolContainer;
        private BombCutSoundEffect.Pool _bombCutSoundPool;

        private List<IBeatmapObjectController> _spawnedBeatmapObjectControllers;
        private Dictionary<float, CallbacksInTime> _callbacksInTimes;
        private BombCutSoundEffectManager _bombCutSoundEffectManager;
        private AudioManagerSO _audioManagerSO;
        private AudioSource _beatmapAudioSource;

        private void Start()
        {
            _bombCutSoundEffectManager = Resources.FindObjectsOfTypeAll<BombCutSoundEffectManager>().First();
            _audioManagerSO = Resources.FindObjectsOfTypeAll<AudioManagerSO>().First();
            _beatmapAudioSource = _audioTimeSyncController.GetField<AudioSource, AudioTimeSyncController>("_audioSource");
            ResolveFields();
        }
        public void Rewind(float time, bool resume = true)
        {
            if (Math.Abs(time - _audioTimeSyncController.songTime) < 0.001f) return;

            bool flag = _audioTimeSyncController.state == AudioTimeSyncController.State.Paused;
            if (!flag) _audioTimeSyncController.Pause();

            DespawnAllNoteControllerSounds();
            DespawnAllBeatmapObjects();

            _audioTimeSyncController.SetField("_prevAudioSamplePos", -1);
            _audioTimeSyncController.SeekTo(time / _audioTimeSyncController.timeScale);
            _beatmapCallbacksControllerInitData.SetField("startFilterTime", time);
            NoodleExtensionsInterop.RequestReprocess();
            _beatmapCallbacksController.SetField("_startFilterTime", time);
            _beatmapCallbacksController.SetField("_prevSongTime", float.MinValue);
            _callbacksInTimes.ToList().ForEach(x => x.Value.lastProcessedNode = null);

            if (!flag && resume) _audioTimeSyncController.Resume();
            _beatmapCallbacksUpdater.Resume();

            OnSongRewind?.Invoke(time);
        }
        public void SetSpeedMultiplier(float multiplier, bool resume = true)
        {
            if (Math.Abs(multiplier - _audioTimeSyncController.timeScale) < 0.001f) return;
            bool flag = _audioTimeSyncController.state == AudioTimeSyncController.State.Paused;
            if (!flag) _audioTimeSyncController.Pause();

            DespawnAllNoteControllerSounds();
            _audioTimeSyncController.SetField("_timeScale", multiplier);
            _beatmapAudioSource.pitch = multiplier;
            _audioManagerSO.musicPitch = 1f / multiplier;

            if (!flag && resume) _audioTimeSyncController.Resume();

            OnSongSpeedChanged?.Invoke(multiplier);
        }

        private void DespawnAllBeatmapObjects() => _spawnedBeatmapObjectControllers.ForEach(x => x.Dissolve(0));
        private void DespawnAllNoteControllerSounds()
        {
            //don't have any sense because we can't access spawned members
            //_bombCutSoundPool.Clear();
            _noteCutSoundPoolContainer.activeItems.ForEach(x => x.StopPlayingAndFinish());
            _noteCutSoundEffectManager.SetField("_prevNoteATime", -1f);
            _noteCutSoundEffectManager.SetField("_prevNoteBTime", -1f);
        }
        private void ResolveFields()
        {
            _spawnedBeatmapObjectControllers = _beatmapObjectManager
                .GetField<List<IBeatmapObjectController>, BeatmapObjectManager>("_allBeatmapObjects");
            _callbacksInTimes = _beatmapCallbacksController
                .GetField<Dictionary<float, CallbacksInTime>, BeatmapCallbacksController>("_callbacksInTimes");
            _noteCutSoundPoolContainer = _noteCutSoundEffectManager
                .GetField<MemoryPoolContainer<NoteCutSoundEffect>, NoteCutSoundEffectManager>("_noteCutSoundEffectPoolContainer");
        }
    }
}