using System;
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

        protected float _lastRequestedTime;
        protected bool _spawningDisabled;

        public void ToTime(float time, float duration = 0f)
        {
            _lastRequestedTime = time;
            //_beatmapObjectSpawnController.SetField("_disableSpawning", true);
            KillAllSounds();
            DespawnAllBeatmapObjects();
            _audioTimeSyncController.Pause();
            _audioTimeSyncController.SeekTo(time);
            _audioTimeSyncController.Resume();
            _beatmapCallbacksControllerInitData.SetField("startFilterTime", time);
            _beatmapCallbacksController.SetField("_startFilterTime", time);
            foreach (var item in _beatmapCallbacksController.GetField<Dictionary<float, CallbacksInTime>, BeatmapCallbacksController>("_callbacksInTimes"))
            {
                if (item.Value.lastProcessedNode != null && item.Value.lastProcessedNode.Value.time > time)
                {
                    item.Value.lastProcessedNode = null;
                }
            }
            //_beatmapObjectSpawnController.SetField("_disableSpawning", false);
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
            List<NoteCutSoundEffect> sounds = new List<NoteCutSoundEffect>();
            sounds.AddRange(((MemoryPoolContainer<NoteCutSoundEffect>)_noteCutSoundEffectManager.GetType()
                .GetField("_noteCutSoundEffectPoolContainer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(_noteCutSoundEffectManager)).activeItems);

            foreach (var item in sounds)
            {
                item.StopPlayingAndFinish();
            }

            _noteCutSoundEffectManager.SetField("_prevNoteATime", 1f);
            _noteCutSoundEffectManager.SetField("_prevNoteBTime", 1f);
        }
    }
}
