using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Emulating
{
    public class BeatmapTimeController : MonoBehaviour
    {
        [Inject] protected readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] protected readonly BeatmapObjectSpawnController _beatmapObjectSpawnController;
        [Inject] protected readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        [Inject] protected readonly AudioTimeSyncController _audioTimeSyncController;
        [Inject] protected readonly SimpleNoteComparatorsSpawner _simpleComparatorsSpawner;
        [Inject] protected readonly BeatmapCallbacksController.InitData _beatmapCallbacksControllerInitData;
        [Inject] protected readonly BeatmapCallbacksController _beatmapCallbacksController;
        [Inject] protected readonly BeatmapCallbacksUpdater _beatmapCallbacksUpdater;
        [Inject] protected readonly IReadonlyBeatmapData _beatmapData;

        public event Action<float> onSongTimeScale;
        public event Action<float> onSongRewind;

        protected BombCutSoundEffectManager _bombCutSoundEffectManager;
        protected AudioManagerSO _audioManagerSO;
        protected AudioSource _beatmapAudioSource;

        public void Start()
        {
            _bombCutSoundEffectManager = Resources.FindObjectsOfTypeAll<BombCutSoundEffectManager>().First();
            _audioManagerSO = Resources.FindObjectsOfTypeAll<AudioManagerSO>().First();
            _beatmapAudioSource = _audioTimeSyncController.GetField<AudioSource, AudioTimeSyncController>("_audioSource");
        }
        public void Rewind(float time)
        {
            if (time == _audioTimeSyncController.songTime) return;
            bool flag = _audioTimeSyncController.state == AudioTimeSyncController.State.Paused;

            if (!flag)
            {
                _audioTimeSyncController.Pause();
                _beatmapCallbacksUpdater.Pause();
            }
            KillAllSounds();
            DespawnAllBeatmapObjects();

            _audioTimeSyncController.SetField("_prevAudioSamplePos", -1);
            _audioTimeSyncController.SeekTo(time / _audioTimeSyncController.timeScale);
            _beatmapCallbacksControllerInitData.SetField("startFilterTime", time);
            _beatmapCallbacksController.SetField("_startFilterTime", time);
            _beatmapCallbacksController.SetField("_prevSongTime", float.MinValue);
            var beatmapEvents = _beatmapCallbacksController.GetField<Dictionary<float, CallbacksInTime>, BeatmapCallbacksController>("_callbacksInTimes");
            foreach (var item in beatmapEvents)
            {
                item.Value.lastProcessedNode = null;
            }

            onSongRewind?.Invoke(time);
            if (!flag)
            {
                _audioTimeSyncController.Resume();
                _beatmapCallbacksUpdater.Resume();
            }
        }
        public void SetTimeScale(float multiplier)
        {
            if (multiplier == _audioTimeSyncController.timeScale) return;
            bool flag = _audioTimeSyncController.state == AudioTimeSyncController.State.Paused;
            if (!flag) _audioTimeSyncController.Pause();

            KillAllSounds();
            _audioTimeSyncController.SetField("_timeScale", multiplier);
            _beatmapAudioSource.pitch = multiplier;
            _audioManagerSO.musicPitch = 1f / multiplier;

            onSongTimeScale?.Invoke(multiplier);
            if (!flag) _audioTimeSyncController.Resume();
        }
        protected void DespawnAllBeatmapObjects()
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
        protected void KillAllSounds()
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
