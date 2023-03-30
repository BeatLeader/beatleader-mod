using BeatLeader.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models.Replay;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    public class ReplayerLauncher : MonoBehaviour {
        [Inject] private readonly GameScenesManager _gameScenesManager = null!;
        [Inject] private readonly PlayerDataModel _playerDataModel = null!;

        public static ReplayLaunchData? LaunchData { get; private set; }
        public static bool IsStartedAsReplay { get; private set; }

        public static event Action<ReplayLaunchData>? ReplayWasStartedEvent;
        public static event Action<ReplayLaunchData>? ReplayWasFinishedEvent;

        public bool StartReplay(ReplayLaunchData data) {
            if (data.Replays.Count == 0) return false;

            Plugin.Log.Notice("[Launcher] Loading replay data...");
            try {
                var transitionData = data.CreateTransitionData(_playerDataModel);
                Plugin.Log.Notice("AfterCreate");

                transitionData.didFinishEvent += HandleLevelFinish;
                IsStartedAsReplay = true;
                LaunchData = data;

                Plugin.Log.Notice("PrePush");
                _gameScenesManager.PushScenes(transitionData, 0.7f, null);
                ReplayWasStartedEvent?.Invoke(data);
                Plugin.Log.Notice("[Launcher] Starting replay...");

                return true;
            } catch (Exception ex) {
                Plugin.Log.Error("Exception: \r\n" + ex);
                return false;
            }
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