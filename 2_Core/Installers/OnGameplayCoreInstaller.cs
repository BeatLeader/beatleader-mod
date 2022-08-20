using System.Collections.Generic;
using System.Reflection;
using BeatLeader.Replayer;
using BeatLeader.Core.Managers.ReplayEnhancer;
using BeatLeader.Utils;
using HarmonyLib;
using JetBrains.Annotations;
using SiraUtil.Submissions;
using Zenject;

namespace BeatLeader.Installers {
    [UsedImplicitly]
    public class OnGameplayCoreInstaller : Installer<OnGameplayCoreInstaller> {

        private List<string> modes = new() { "Solo", "Multiplayer" };

        public override void InstallBindings() {
            Plugin.Log.Debug("OnGameplayCoreInstaller");
            if (ReplayerMenuLauncher.IsStartedAsReplay)
            {
                DisableScoreSubmission();
                ReplayerManualInstaller.Install(ReplayerMenuLauncher.Replay, ReplayerMenuLauncher.Score, Container);
            }
            else InitRecorder();
        }

        private void InitRecorder() {
            RecorderUtils.buffer = RecorderUtils.shouldRecord;

            if (RecorderUtils.shouldRecord) {
                RecorderUtils.shouldRecord = false;

                #region Gates
                if (ScoreSaber_playbackEnabled != null && (bool)ScoreSaber_playbackEnabled.Invoke(null, null) == false) {
                    Plugin.Log.Debug("SS replay is running, BL Replay Recorder will not be started.");
                    return;
                }
                if (!(MapEnhancer.previewBeatmapLevel.levelID.StartsWith(CustomLevelLoader.kCustomLevelPrefixId))) {
                    Plugin.Log.Debug("OST level detected. No recording.");
                    return;
                }
                var gameMode = GameMode();
                if (gameMode != null && !modes.Contains(gameMode)) {
                    Plugin.Log.Debug("Not allowed game mode");
                    return;
                }
                #endregion

                Plugin.Log.Debug("Starting a BL Replay Recorder.");

                Container.BindInterfacesAndSelfTo<ReplayRecorder>().AsSingle();
                Container.BindInterfacesAndSelfTo<TrackingDeviceEnhancer>().AsTransient();
            } else {
                //Plugin.Log.Info("Unknown flow detected, recording would not be started.");
            }
        }
        private void DisableScoreSubmission()
        {
            var submission = Container.Resolve<Submission>();
            submission.DisableScoreSubmission("BeatLeaderReplayer", "Playback");
        }

        #region Game mode stuff

        private string GameMode() {
            string singleGM = GetStandardDataSO()?.gameMode;
            string multiGM = GetMultiDataSO()?.gameMode;

            foreach (string mode in new[] { singleGM, multiGM }) {
                if (mode != null && mode.Length > 0) {
                    return mode;
                }
            }

            return null;
        }

        private StandardLevelScenesTransitionSetupDataSO GetStandardDataSO() {
            try {
                return Container.TryResolve<StandardLevelScenesTransitionSetupDataSO>();
            } catch {
                return null;
            }
        }

        private MultiplayerLevelScenesTransitionSetupDataSO GetMultiDataSO() {
            try {
                return Container.TryResolve<MultiplayerLevelScenesTransitionSetupDataSO>();
            } catch {
                return null;
            }
        }

        #endregion

        // TODO: remove this after verifying of an "allowed flows" strategy
        private static readonly MethodBase ScoreSaber_playbackEnabled = AccessTools.Method("ScoreSaber.Core.ReplaySystem.HarmonyPatches.PatchHandleHMDUnmounted:Prefix");
    }
}