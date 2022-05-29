﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.MapEmitating
{
    public class SimpleTimeController : MonoBehaviour
    {
        [Inject] protected readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] protected readonly BeatmapObjectSpawnController _beatmapObjectSpawnController;
        [Inject] protected readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        [Inject] protected readonly AudioTimeSyncController _audioTimeSyncController;
        [Inject] protected readonly BeatmapCallbacksController.InitData _beatmapCallbacksControllerInitData;
        [Inject] protected readonly BeatmapCallbacksController _beatmapCallbacksController;
        [Inject] protected readonly BeatmapCallbacksUpdater _beatmapCallbacksUpdater;
        [Inject] protected readonly IReadonlyBeatmapData _beatmapData;

        protected BombCutSoundEffectManager _bombCutSoundEffectManager;

        public void Start()
        {
            _bombCutSoundEffectManager = Resources.FindObjectsOfTypeAll<BombCutSoundEffectManager>().First();
        }

        public void Rewind(float time)
        {
            if (time == _audioTimeSyncController.songTime) return;
            _audioTimeSyncController.Pause();
            KillAllSounds();
            DespawnAllBeatmapObjects();
            _beatmapCallbacksUpdater.Pause();
            _beatmapCallbacksControllerInitData.SetField("startFilterTime", time);
            _beatmapCallbacksController.SetField("_startFilterTime", time);
            _beatmapCallbacksController.SetField("_prevSongTime", float.MinValue);
            var beatmapEvents = _beatmapCallbacksController.GetField<Dictionary<float, CallbacksInTime>, BeatmapCallbacksController>("_callbacksInTimes");
            foreach (var item in beatmapEvents)
            {
                if (item.Value.lastProcessedNode != null && item.Value.lastProcessedNode.Value.time < time)
                {
                    item.Value.lastProcessedNode = null;
                }
            }
            _audioTimeSyncController.SeekTo(time);
            _audioTimeSyncController.Resume();
            _beatmapCallbacksUpdater.Resume();
        }
        public void SetTimeScale(float multiplier)
        {
            if (multiplier != _audioTimeSyncController.timeScale)
            {
                _beatmapCallbacksUpdater.Pause();
                KillAllSounds();
                _audioTimeSyncController.SetField("_timeScale", multiplier);
                var audioSource = _audioTimeSyncController.GetField<AudioSource, AudioTimeSyncController>("_audioSource");
                audioSource.pitch = multiplier;
                Resources.FindObjectsOfTypeAll<AudioManagerSO>().First().musicPitch = 1f / multiplier;
                _beatmapCallbacksUpdater.Resume();
            }
        }
        protected virtual void DespawnAllBeatmapObjects()
        {
            List<NoteController> notes = new List<NoteController>();
            List<ObstacleController> obstacles = new List<ObstacleController>();

            notes.AddRange(((MemoryPoolContainer<GameNoteController>)_beatmapObjectManager.GetType()
                .GetField("_basicGameNotePoolContainer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);
            notes.AddRange(((MemoryPoolContainer<GameNoteController>)_beatmapObjectManager.GetType()
                .GetField("_burstSliderHeadGameNotePoolContainer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);
            notes.AddRange(((MemoryPoolContainer<BurstSliderGameNoteController>)_beatmapObjectManager.GetType()
                .GetField("_burstSliderGameNotePoolContainer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);
            notes.AddRange(((MemoryPoolContainer<BurstSliderGameNoteController>)_beatmapObjectManager.GetType()
                .GetField("_burstSliderFillPoolContainer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);
            notes.AddRange(((MemoryPoolContainer<BombNoteController>)_beatmapObjectManager.GetType()
                .GetField("_bombNotePoolContainer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);
            obstacles.AddRange(((MemoryPoolContainer<ObstacleController>)_beatmapObjectManager.GetType()
                .GetField("_obstaclePoolContainer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);

            foreach (var item in notes)
            {
                item.Dissolve(0f);
            }
            foreach (var item in obstacles)
            {
                item.Dissolve(0f);
            }
        }
        protected virtual void KillAllSounds()
        {
            List<NoteCutSoundEffect> notesSounds = new List<NoteCutSoundEffect>();
            notesSounds.AddRange(((MemoryPoolContainer<NoteCutSoundEffect>)_noteCutSoundEffectManager.GetType()
                .GetField("_noteCutSoundEffectPoolContainer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_noteCutSoundEffectManager)).activeItems);
            ((BombCutSoundEffect.Pool)_bombCutSoundEffectManager.GetType()
                .GetField("_bombCutSoundEffectPool", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_bombCutSoundEffectManager)).Clear();

            foreach (var item in notesSounds)
            {
                item.StopPlayingAndFinish();
            }

            _noteCutSoundEffectManager.SetField("_prevNoteATime", -1f);
            _noteCutSoundEffectManager.SetField("_prevNoteBTime", -1f);
        }
    }
}