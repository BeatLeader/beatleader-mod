using System.Collections.Generic;
using System.Reflection;
using BeatLeader.Replayer;
using BeatLeader.Core.Managers.ReplayEnhancer;
using BeatLeader.Utils;
using HarmonyLib;
using SiraUtil.Submissions;
using Zenject;
using BeatLeader.Replayer.Emulation;
using BeatLeader.Components;
using BeatLeader.ViewControllers;
using SiraUtil.Tools.FPFC;
using System;
using BeatLeader.Replayer.Tweaking;
using BeatLeader.Replayer.Binding;
using BeatLeader.UI;
using BeatLeader.Models;
using IPA.Loader;

namespace BeatLeader.Installers {
    public class OnGameplayCoreInstaller : Installer<OnGameplayCoreInstaller> {
        public override void InstallBindings() {
            Plugin.Log.Debug("OnGameplayCoreInstaller");
            if (ReplayerLauncher.IsStartedAsReplay) InitReplayer();
            else InitRecorder();
        }

        #region Recorder

        private static readonly List<string> modes = new() { "Solo", "Multiplayer" };

        private void InitRecorder() {
            RecorderUtils.buffer = RecorderUtils.shouldRecord;

            if (RecorderUtils.shouldRecord) {
                RecorderUtils.shouldRecord = false;

                #region Gates
                if (PluginManager.GetPluginFromId("ScoreSaber") != null && ScoreSaber_playbackEnabled != null && (bool)ScoreSaber_playbackEnabled.Invoke(null, null) == false) {
                    Plugin.Log.Debug("SS replay is running, BL Replay Recorder will not be started!");
                    return;
                }
                var gameMode = GetGameMode();
                if (gameMode != null && !modes.Contains(gameMode)) {
                    Plugin.Log.Debug("Not allowed game mode, BL Replay Recorder will not be started!");
                    return;
                }

                #endregion

                Plugin.Log.Debug("Starting a BL Replay Recorder...");

                Container.BindInterfacesAndSelfTo<ReplayRecorder>().AsSingle();
                Container.BindInterfacesAndSelfTo<TrackingDeviceEnhancer>().AsTransient();
            } else {
                //Plugin.Log.Info("Unknown flow detected, recording would not be started.");
            }
        }

        #endregion

        #region Replayer

        private void InitReplayer() {
            DisableScoreSubmission();

            //Dependencies
            var launchData = ReplayerLauncher.LaunchData!;
            Container.Bind<ReplayLaunchData>().FromInstance(launchData).AsSingle();
            Container.BindInterfacesTo<ReplayBeatmapData>().AsSingle();
            Container.Bind<ReplayerExtraObjectsProvider>().FromNewComponentOnNewGameObject().AsSingle();

            //Playback
            Container.BindInterfacesAndSelfTo<ReplayPauseController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ReplayFinishController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ReplayTimeController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            //VR Controllers
            Container.Bind<OriginalVRControllersProvider>().FromNewComponentOnNewGameObject().AsSingle();
            Container.Bind<MenuControllersManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            if (launchData.VRControllersManager is { } controllersManager) {
                Container.BindInterfacesTo(controllersManager.GetType()).AsSingle();
            } else {
                Container.BindInterfacesTo<VRControllersManager>().AsSingle();
            }
            
            //Event Processing
            Container.BindMemoryPool<ReplayBeatmapEventsProcessor, ReplayBeatmapEventsProcessor.Pool>().WithInitialSize(2);
            Container.BindMemoryPool<ReplayScoreEventsProcessor, ReplayScoreEventsProcessor.Pool>().WithInitialSize(2);
            Container.BindMemoryPool<VirtualPlayer, VirtualPlayer.Pool>().WithInitialSize(2);
            Container.BindInterfacesTo<VirtualPlayersManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Container.BindInterfacesTo<ReplayBeatmapEventsProcessorProxy>().AsSingle();
            Container.Bind<ReplayHeightsProcessor>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            //Notes handling
            Container.Bind<ReplayerScoreProcessor>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<ReplayerNotesCutter>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            //Tweaks and tools
            Container.Bind<BeatmapVisualsController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<ReplayerCameraController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<TweaksHandler>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<HotkeysHandler>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ReplayWatermark>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            //UI
            Container.Bind<Replayer2DViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ReplayerVRViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ReplayerUIBinder>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Plugin.Log.Notice("[Installer] Replays system successfully installed!");
        }

        #endregion

        #region Submission

        private static Ticket _submissionTicket;

        private void DisableScoreSubmission() {
            _submissionTicket = Container.Resolve<Submission>()?.DisableScoreSubmission("BeatLeaderReplayer", "Playback");
            ReplayerLauncher.LaunchData.ReplayWasFinishedEvent += HandleReplayWasFinished;
        }

        private void HandleReplayWasFinished(StandardLevelScenesTransitionSetupDataSO data, Models.ReplayLaunchData launchData) {
            launchData.ReplayWasFinishedEvent -= HandleReplayWasFinished;
            if (_submissionTicket != null) {
                Container.Resolve<Submission>()?.Remove(_submissionTicket);
                _submissionTicket = null;
            }
        }

        #endregion

        #region Game mode stuff

        private string GetGameMode() {
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