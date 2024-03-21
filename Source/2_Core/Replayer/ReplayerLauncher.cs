using BeatLeader.Models;
using System;
using System.Linq;
using BeatLeader.Utils;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    [PublicAPI]
    public class ReplayerLauncher : MonoBehaviour {
        #region Injection
        
        [Inject] private readonly GameScenesManager _gameScenesManager = null!;
        [Inject] private readonly PlayerDataModel _playerDataModel = null!;
        [Inject] private readonly EnvironmentsListModel _environmentsListModel = null!;
        [Inject] private readonly BeatmapDataLoader _beatmapDataLoader = null!;
        [Inject] private readonly AudioClipAsyncLoader _audioClipAsyncLoader = null!;
        [InjectOptional] private readonly BeatmapLevelsModel _beatmapLevelsModel = null!;
        
        #endregion

        #region StartReplay

        public static ReplayLaunchData? LaunchData { get; private set; }
        public static bool IsStartedAsReplay { get; private set; }

        public static event Action<ReplayLaunchData>? ReplayWasStartedEvent;
        public static event Action<ReplayLaunchData>? ReplayWasFinishedEvent;

        public static byte[]? GetMainReplayCustomData(string key) {
            var dictionary = LaunchData?.MainReplay?.CustomData;

            if (!IsStartedAsReplay || dictionary == null || !dictionary.ContainsKey(key)) {
                return default;
            }

            return dictionary[key];
        }

        public bool StartReplay(ReplayLaunchData data, Action? afterTransitionCallback = null) {
            if (data.Replays.Count == 0) return false;

            Plugin.Log.Notice("[Launcher] Loading replay data...");
            var transitionData = CreateTransitionData(data);
            transitionData.didFinishEvent += HandleLevelFinish;
            IsStartedAsReplay = true;
            LaunchData = data;

            Plugin.Log.Notice("[Launcher] Starting replay...");
            _gameScenesManager.PushScenes(transitionData, 0.7f, afterTransitionCallback);
            ReplayWasStartedEvent?.Invoke(data);

            return true;
        }

        #endregion

        #region CreateTransitionData

        private static readonly EnvironmentType normalEnvironmentType = EnvironmentType.Normal;

        private static StandardLevelScenesTransitionSetupDataSO? _standardLevelScenesTransitionSetupDataSo;

        private void Awake() {
            if (!_standardLevelScenesTransitionSetupDataSo) {
                _standardLevelScenesTransitionSetupDataSo = Resources
                    .FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>()
                    .First();
            }
        }

        private StandardLevelScenesTransitionSetupDataSO CreateTransitionData(ReplayLaunchData launchData) {
            var transitionData = _standardLevelScenesTransitionSetupDataSo;
            var playerData = _playerDataModel.playerData;

            var overrideEnv = launchData.EnvironmentInfo != null;
            var envSettings = playerData.overrideEnvironmentSettings;
            if (overrideEnv) {
                envSettings = new() { overrideEnvironments = true };
                envSettings.SetEnvironmentInfoForType(
                    normalEnvironmentType,
                    launchData.EnvironmentInfo
                );
            }

            var replay = launchData.MainReplay;
            var practiceSettings = launchData.IsBattleRoyale ? null
                : launchData.MainReplay.ReplayData.PracticeSettings;

            transitionData!.Init(
                "Solo",
                launchData.BeatmapKey!.Value,
                launchData.BeatmapLevel,
                envSettings,
                playerData.colorSchemesSettings.GetOverrideColorScheme(),
                null,
                replay.ReplayData.GameplayModifiers,
                playerData.playerSpecificSettings.GetPlayerSettingsByReplay(replay),
                practiceSettings,
                _environmentsListModel,
                _audioClipAsyncLoader,
                _beatmapDataLoader,
                "Menu",
                _beatmapLevelsModel
            );

            return transitionData;
        }

        #endregion

        #region Callbacks

        private static void HandleLevelFinish(
            StandardLevelScenesTransitionSetupDataSO transitionData,
            LevelCompletionResults completionResults
        ) {
            transitionData.didFinishEvent -= HandleLevelFinish;
            LaunchData!.FinishReplay(transitionData);
            ReplayWasFinishedEvent?.Invoke(LaunchData);

            LaunchData = null;
            IsStartedAsReplay = false;
        }

        #endregion
    }
}