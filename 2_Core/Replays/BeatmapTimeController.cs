using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Replays.Emulating;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class BeatmapTimeController : MonoBehaviour
    {
        [Inject] private readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] private readonly BeatmapObjectSpawnController _beatmapObjectSpawnController;
        [Inject] private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        [Inject] private readonly AudioTimeSyncController _audioTimeSyncController;
        [Inject] private readonly SimpleNoteComparatorsSpawner _simpleComparatorsSpawner;
        [Inject] private readonly BeatmapCallbacksController.InitData _beatmapCallbacksControllerInitData;
        [Inject] private readonly BeatmapCallbacksController _beatmapCallbacksController;
        [Inject] private readonly BeatmapCallbacksUpdater _beatmapCallbacksUpdater;
        [Inject] private readonly IReadonlyBeatmapData _beatmapData;

        private BombCutSoundEffectManager _bombCutSoundEffectManager;
        private AudioManagerSO _audioManagerSO;
        private AudioSource _beatmapAudioSource;

        public event Action<float> OnSongSpeedChanged;
        public event Action<float> OnSongRewind;

        public void Start()
        {
            _bombCutSoundEffectManager = Resources.FindObjectsOfTypeAll<BombCutSoundEffectManager>().First();
            _audioManagerSO = Resources.FindObjectsOfTypeAll<AudioManagerSO>().First();
            _beatmapAudioSource =
                _audioTimeSyncController.GetField<AudioSource, AudioTimeSyncController>("_audioSource");
        }
        public void Rewind(float time)
        {
            if (Math.Abs(time - _audioTimeSyncController.songTime) < 0.001f) return;
            var flag = _audioTimeSyncController.state == AudioTimeSyncController.State.Paused;

            if (!flag) _audioTimeSyncController.Pause();
            KillAllSounds();
            DespawnAllBeatmapObjects();

            _audioTimeSyncController.SetField("_prevAudioSamplePos", -1);
            _audioTimeSyncController.SeekTo(time / _audioTimeSyncController.timeScale);
            _beatmapCallbacksControllerInitData.SetField("startFilterTime", time);
            _beatmapCallbacksController.SetField("_startFilterTime", time);
            _beatmapCallbacksController.SetField("_prevSongTime", float.MinValue);
            foreach (var item in _beatmapCallbacksController
                         .GetField<Dictionary<float, CallbacksInTime>, BeatmapCallbacksController>("_callbacksInTimes"))
            {
                item.Value.lastProcessedNode = null;
            }
            OnSongRewind?.Invoke(time);
            if (!flag) _audioTimeSyncController.Resume();
            _beatmapCallbacksUpdater.Resume();
        }
        public void SetSpeedMultiplier(float multiplier)
        {
            if (multiplier == _audioTimeSyncController.timeScale) return;
            bool flag = _audioTimeSyncController.state == AudioTimeSyncController.State.Paused;
            if (!flag) _audioTimeSyncController.Pause();

            KillAllSounds();
            _audioTimeSyncController.SetField("_timeScale", multiplier);
            _beatmapAudioSource.pitch = multiplier;
            _audioManagerSO.musicPitch = 1f / multiplier;

            OnSongSpeedChanged?.Invoke(multiplier);
            if (!flag) _audioTimeSyncController.Resume();
        }

        private void DespawnAllBeatmapObjects()
        {
            var notes = new List<NoteController>();
            var obstacles = new List<ObstacleController>();

            notes.AddRange(((MemoryPoolContainer<GameNoteController>)_beatmapObjectManager.GetType()
                .GetField("_basicGameNotePoolContainer",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);
            notes.AddRange(((MemoryPoolContainer<GameNoteController>)_beatmapObjectManager.GetType()
                .GetField("_burstSliderHeadGameNotePoolContainer",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);
            notes.AddRange(((MemoryPoolContainer<BurstSliderGameNoteController>)_beatmapObjectManager.GetType()
                .GetField("_burstSliderGameNotePoolContainer",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);
            notes.AddRange(((MemoryPoolContainer<BurstSliderGameNoteController>)_beatmapObjectManager.GetType()
                .GetField("_burstSliderFillPoolContainer",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);
            notes.AddRange(((MemoryPoolContainer<BombNoteController>)_beatmapObjectManager.GetType()
                .GetField("_bombNotePoolContainer",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);
            obstacles.AddRange(((MemoryPoolContainer<ObstacleController>)_beatmapObjectManager.GetType()
                .GetField("_obstaclePoolContainer",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);

            notes.ForEach(x => x.Dissolve(0));
            obstacles.ForEach(x => x.Dissolve(0));
        }
        private void KillAllSounds()
        {
            List<NoteCutSoundEffect> notesSounds = new List<NoteCutSoundEffect>();
            notesSounds.AddRange(((MemoryPoolContainer<NoteCutSoundEffect>)_noteCutSoundEffectManager.GetType()
                .GetField("_noteCutSoundEffectPoolContainer",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_noteCutSoundEffectManager)).activeItems);
            ((BombCutSoundEffect.Pool)_bombCutSoundEffectManager.GetType()
                .GetField("_bombCutSoundEffectPool",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_bombCutSoundEffectManager)).Clear();

            notesSounds.ForEach(x => x.StopPlayingAndFinish());
            _noteCutSoundEffectManager.SetField("_prevNoteATime", -1f);
            _noteCutSoundEffectManager.SetField("_prevNoteBTime", -1f);
        }
    }
}