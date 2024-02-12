using BeatLeader.Models;
using System;
using BeatLeader.Utils;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

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
            transitionData.didFinishEvent += HandleLevelFinish;
            IsStartedAsReplay = true;
            LaunchData = data;

            Plugin.Log.Notice("[Launcher] Starting replay...");
            _gameScenesManager.PushScenes(transitionData, 0.7f, afterTransitionCallback);
            ReplayWasStartedEvent?.Invoke(data);

            return true;
        }

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
    }
}