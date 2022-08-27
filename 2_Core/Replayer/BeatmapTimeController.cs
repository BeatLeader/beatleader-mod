using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Replayer.Emulation;
using IPA.Loader;
using IPA.Utilities;
using HarmonyLib;
using UnityEngine;
using Zenject;
using System.Management;

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

        private BombCutSoundEffectManager _bombCutSoundEffectManager;
        private AudioManagerSO _audioManagerSO;
        private AudioSource _beatmapAudioSource;

        public event Action<float> OnSongSpeedChanged;
        public event Action<float> OnSongRewind;

        private void Start()
        {
            _bombCutSoundEffectManager = Resources.FindObjectsOfTypeAll<BombCutSoundEffectManager>().First();
            _audioManagerSO = Resources.FindObjectsOfTypeAll<AudioManagerSO>().First();
            _beatmapAudioSource = _audioTimeSyncController.GetField<AudioSource, AudioTimeSyncController>("_audioSource");
            PatchNoodle();
        }
        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
        public void Rewind(float time, bool resume = true)
        {
            if (Math.Abs(time - _audioTimeSyncController.songTime) < 0.001f) return;

            bool flag = _audioTimeSyncController.state == AudioTimeSyncController.State.Paused;
            if (!flag) _audioTimeSyncController.Pause();

            KillAllSounds();
            DespawnAllBeatmapObjects();

            _audioTimeSyncController.SetField("_prevAudioSamplePos", -1);
            _audioTimeSyncController.SeekTo(time / _audioTimeSyncController.timeScale);
            _beatmapCallbacksControllerInitData.SetField("startFilterTime", time);
            _neOverrideRequired = true;
            _beatmapCallbacksController.SetField("_startFilterTime", time);
            _beatmapCallbacksController.SetField("_prevSongTime", float.MinValue);
            _beatmapCallbacksController.GetField<Dictionary<float, CallbacksInTime>,
                BeatmapCallbacksController>("_callbacksInTimes").ToList().ForEach(x => x.Value.lastProcessedNode = null);

            OnSongRewind?.Invoke(time);
            if (!flag && resume) _audioTimeSyncController.Resume();
            _beatmapCallbacksUpdater.Resume();
        }
        public void SetSpeedMultiplier(float multiplier, bool resume = true)
        {
            if (multiplier == _audioTimeSyncController.timeScale) return;
            bool flag = _audioTimeSyncController.state == AudioTimeSyncController.State.Paused;
            if (!flag) _audioTimeSyncController.Pause();

            KillAllSounds();
            _audioTimeSyncController.SetField("_timeScale", multiplier);
            _beatmapAudioSource.pitch = multiplier;
            _audioManagerSO.musicPitch = 1f / multiplier;

            OnSongSpeedChanged?.Invoke(multiplier);
            if (!flag && resume) _audioTimeSyncController.Resume();
        }

        private void DespawnAllBeatmapObjects()
        {
            var notes = new List<NoteController>();
            var obstacles = new List<ObstacleController>();

            notes.AddRange(((MemoryPoolContainer<GameNoteController>)_beatmapObjectManager.GetType()
                .GetField("_basicGameNotePoolContainer",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);
            notes.AddRange(((MemoryPoolContainer<GameNoteController>)_beatmapObjectManager.GetType()
                .GetField("_burstSliderHeadGameNotePoolContainer",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);
            notes.AddRange(((MemoryPoolContainer<BurstSliderGameNoteController>)_beatmapObjectManager.GetType()
                .GetField("_burstSliderGameNotePoolContainer",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);
            notes.AddRange(((MemoryPoolContainer<BurstSliderGameNoteController>)_beatmapObjectManager.GetType()
                .GetField("_burstSliderFillPoolContainer",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);
            notes.AddRange(((MemoryPoolContainer<BombNoteController>)_beatmapObjectManager.GetType()
                .GetField("_bombNotePoolContainer",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);
            obstacles.AddRange(((MemoryPoolContainer<ObstacleController>)_beatmapObjectManager.GetType()
                .GetField("_obstaclePoolContainer",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(_beatmapObjectManager)).activeItems);

            notes.ForEach(x => x.Dissolve(0));
            obstacles.ForEach(x => x.Dissolve(0));
        }
        private void KillAllSounds()
        {
            List<NoteCutSoundEffect> notesSounds = new List<NoteCutSoundEffect>();
            notesSounds.AddRange(((MemoryPoolContainer<NoteCutSoundEffect>)_noteCutSoundEffectManager.GetType()
                .GetField("_noteCutSoundEffectPoolContainer",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(_noteCutSoundEffectManager)).activeItems);
            ((BombCutSoundEffect.Pool)_bombCutSoundEffectManager.GetType()
                .GetField("_bombCutSoundEffectPool",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(_bombCutSoundEffectManager)).Clear();

            notesSounds.ForEach(x => x.StopPlayingAndFinish());
            _noteCutSoundEffectManager.SetField("_prevNoteATime", -1f);
            _noteCutSoundEffectManager.SetField("_prevNoteBTime", -1f);
        }

        #region NoodlePatch

        private MethodInfo _neCallbacksControllerUpdateMethod;
        private MethodInfo _prefixMethod;
        private Type _neCallbacksControllerType;
        private Assembly _neAssembly;
        private Harmony _harmony;
        private static FieldInfo _callbacksInTimeField;
        private static FieldInfo _prevSongTimeField;
        private static bool _neOverrideRequired;

        private void PatchNoodle()
        {
            _neAssembly = PluginManager.GetPluginFromId("NoodleExtensions")?.Assembly;
            if (_neAssembly == null) return;

            _neCallbacksControllerType = _neAssembly.GetType("NoodleExtensions.Managers.NoodleObjectsCallbacksManager");
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;

            _neCallbacksControllerUpdateMethod = _neCallbacksControllerType.GetMethod("ManualUpdate", flags);
            if (_neCallbacksControllerUpdateMethod == null) return;

            _prevSongTimeField = _neCallbacksControllerType.GetField("_prevSongtime", flags);
            _callbacksInTimeField = _neCallbacksControllerType.GetField("_callbacksInTime", flags);
            if (_prevSongTimeField == null || _callbacksInTimeField == null) return;

            _prefixMethod = GetType().GetMethod(nameof(NoodleCallbacksControllerPrefix),
                BindingFlags.NonPublic | BindingFlags.Static);

            _harmony = new Harmony("BeatLeader.Replayer.BeatmapTimeController");
            HarmonyUtils.Patch(_harmony, new HarmonyPatchDescriptor(_neCallbacksControllerUpdateMethod, _prefixMethod));
        }
        private static void NoodleCallbacksControllerPrefix(object __instance)
        {
            if (_neOverrideRequired)
            {
                _prevSongTimeField.SetValue(__instance, float.MinValue);
                ((CallbacksInTime)_callbacksInTimeField.GetValue(__instance)).lastProcessedNode = null;
                _neOverrideRequired = false;
            }
        }

        #endregion
    }
}