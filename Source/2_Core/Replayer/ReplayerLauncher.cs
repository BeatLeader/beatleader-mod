using BeatLeader.Models;
using System;
using BeatLeader.Utils;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using System.Linq;

namespace BeatLeader.Replayer {
    [PublicAPI]
    public class ReplayerLauncher : MonoBehaviour {
        [Inject] private readonly GameScenesManager _gameScenesManager = null!;
        [Inject] private readonly PlayerDataModel _playerDataModel = null!;

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
            var transitionData = data.CreateTransitionData(_playerDataModel);
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

        //private static readonly EnvironmentTypeSO normalEnvironmentType = EnvironmentTypeSO.;
        private static StandardLevelScenesTransitionSetupDataSO? _standardLevelScenesTransitionSetupDataSo = null;

        private void Awake() {
            if (!_standardLevelScenesTransitionSetupDataSo) {
                _standardLevelScenesTransitionSetupDataSo = Resources
                    .FindObjectsOfTypeAll<StandardLevelScenesTransitionSetupDataSO>()
                    .First();
            }
        }

        private StandardLevelScenesTransitionSetupDataSO? CreateTransitionData(ReplayLaunchData launchData) {
            var transitionData = _standardLevelScenesTransitionSetupDataSo;
            var playerData = _playerDataModel.playerData;

            var overrideEnv = launchData.EnvironmentInfo != null;
            var envSettings = playerData.overrideEnvironmentSettings;
            //if (overrideEnv) {
            //    envSettings = new() { overrideEnvironments = true };
            //    envSettings.SetEnvironmentInfoForType(
            //        normalEnvironmentType,
            //        launchData.EnvironmentInfo
            //    );
            //}

            var replay = launchData.MainReplay;
            var practiceSettings = launchData.IsBattleRoyale ? null : replay.ReplayData.PracticeSettings;
            var replayModifiers = replay.ReplayData.GameplayModifiers;

            if (transitionData != null) {
                transitionData.Init(
                    "Solo",
                    launchData.DifficultyBeatmap,
                    launchData.DifficultyBeatmap.level,
                    envSettings,
                    playerData.colorSchemesSettings.GetOverrideColorScheme(),
                    launchData.Settings.IgnoreModifiers ? CreateDisabledModifiers(replayModifiers) : replayModifiers,
                    playerData.playerSpecificSettings.GetPlayerSettingsByReplay(replay),
                    practiceSettings,
                    "Menu",
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
    }
}