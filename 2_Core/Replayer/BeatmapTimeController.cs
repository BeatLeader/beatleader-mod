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

        [Inject] private readonly NoteCutSoundEffect.Pool _noteCutSoundPool;
        [Inject] private readonly BombCutSoundEffect.Pool _bombCutSoundPool;

        public event Action<float> OnSongSpeedChanged;
        public event Action<float> OnSongRewind;

        private List<IBeatmapObjectController> _spawnedBeatmapObjectControllers = new();
        private BombCutSoundEffectManager _bombCutSoundEffectManager;
        private AudioManagerSO _audioManagerSO;
        private AudioSource _beatmapAudioSource;

        private void Start()
        {
            _bombCutSoundEffectManager = Resources.FindObjectsOfTypeAll<BombCutSoundEffectManager>().First();
            _audioManagerSO = Resources.FindObjectsOfTypeAll<AudioManagerSO>().First();
            _beatmapAudioSource = _audioTimeSyncController.GetField<AudioSource, AudioTimeSyncController>("_audioSource");
            ResolveFields();
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

            DespawnAllNoteControllerSounds();
            DespawnAllBeatmapObjects();

            _audioTimeSyncController.SetField("_prevAudioSamplePos", -1);
            _audioTimeSyncController.SeekTo(time / _audioTimeSyncController.timeScale);
            _beatmapCallbacksControllerInitData.SetField("startFilterTime", time);
            _neOverrideRequired = true;
            _beatmapCallbacksController.SetField("_startFilterTime", time);
            _beatmapCallbacksController.SetField("_prevSongTime", float.MinValue);
            _beatmapCallbacksController.GetField<Dictionary<float, CallbacksInTime>,
                BeatmapCallbacksController>("_callbacksInTimes").ToList().ForEach(x => x.Value.lastProcessedNode = null);

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
            _noteCutSoundPool.Clear();
            _bombCutSoundPool.Clear();
            _noteCutSoundEffectManager.SetField("_prevNoteATime", -1f);
            _noteCutSoundEffectManager.SetField("_prevNoteBTime", -1f);
        }
        private void ResolveFields()
        {
            _spawnedBeatmapObjectControllers = _beatmapObjectManager
                .GetField<List<IBeatmapObjectController>, BeatmapObjectManager>("_allBeatmapObjects");
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