using BeatLeader.Models;
using System;
using System.Linq;
using BeatLeader.Utils;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using BeatSaber.PerformancePresets;
using BeatSaber.GameSettings;

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
        [InjectOptional] private readonly BeatmapLevelsEntitlementModel _beatmapLevelsEntitlementModel = null!;
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
            if (transitionData == null) {
                Plugin.Log.Error("[Launcher] Cannot create transition data");
                return false;
            }
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
        private static GraphicSettingsHandler? _graphicSettingsHandler;

        private void Awake() {
            if (!_standardLevelScenesTransitionSetupDataSo) {
                _standardLevelScenesTransitionSetupDataSo = Resources
                    .FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>()
                    .First();
            }

            if (_graphicSettingsHandler == null) {
                _graphicSettingsHandler = Resources
                    .FindObjectsOfTypeAll<MainSystemInit>()
                    .First()._graphicSettingsHandler;
            }
        }

        private StandardLevelScenesTransitionSetupDataSO? CreateTransitionData(ReplayLaunchData launchData) {
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
            var practiceSettings = launchData.IsBattleRoyale ? null : replay.ReplayData.PracticeSettings;
            var replayModifiers = replay.ReplayData.GameplayModifiers;

            if (transitionData != null) {
                transitionData.Init(
                    "Solo",
                    launchData.BeatmapKey!.Value,
                    launchData.BeatmapLevel,
                    envSettings,
                    playerData.colorSchemesSettings.GetOverrideColorScheme(),
                    null,
                    launchData.Settings.IgnoreModifiers ? CreateDisabledModifiers(replayModifiers) : replayModifiers,
                    playerData.playerSpecificSettings.GetPlayerSettingsByReplay(replay),
                    practiceSettings,
                    _environmentsListModel,
                    _audioClipAsyncLoader,
                    _beatmapDataLoader,
                    _graphicSettingsHandler.instance.customPreset,
                    "Menu",
                    _beatmapLevelsModel,
                    _beatmapLevelsEntitlementModel,
                    false,
                    false,
                    null
                );
            }

            return transitionData;
        }

        private static GameplayModifiers CreateDisabledModifiers(GameplayModifiers replayModifiers) {
            return new GameplayModifiers().CopyWith(
                instaFail: replayModifiers.instaFail,
                energyType: replayModifiers.energyType,
                noFailOn0Energy: replayModifiers.noFailOn0Energy
            );
        }

        #endregion

        #region Callbacks

        private static void HandleLevelFinish(
            StandardLevelScenesTransitionSetupDataSO transitionData,
            LevelCompletionResults completionResults
        ) {
            transitionData.didFinishEvent -= HandleLevelFinish;
            LaunchData?.FinishReplay(transitionData);
            ReplayWasFinishedEvent?.Invoke(LaunchData);

            LaunchData = null;
            IsStartedAsReplay = false;
        }

        #endregion
    }
}