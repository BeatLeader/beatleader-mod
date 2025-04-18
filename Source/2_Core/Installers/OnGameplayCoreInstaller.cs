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
using BeatLeader.Models.AbstractReplay;
using BeatLeader.UI.Replayer;
using BeatLeader.UI.Replayer.Desktop;
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
            var replaysCount = launchData.Replays.Count;

            Container.Bind<ReplayLaunchData>().FromInstance(launchData).AsSingle();
            Container.BindInterfacesTo<ReplayBeatmapData>().AsSingle();
            Container.Bind<ReplayerExtraObjectsProvider>().FromNewComponentOnNewGameObject().AsSingle();
            Container.Bind<ZenjectMenuResolver>().AsSingle();

            //Playback
            Container.BindInterfacesAndSelfTo<ReplayPauseController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ReplayFinishController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ReplayTimeController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            //Controllers
            Container.Bind<VirtualPlayerGameSabers>().AsSingle();
            Container.BindMemoryPool<VirtualPlayerAvatarBody, VirtualPlayerAvatarBody.Pool>().WithInitialSize(replaysCount - 1);
            Container.BindMemoryPool<VirtualPlayerBattleRoyaleSabers, VirtualPlayerBattleRoyaleSabers.Pool>().WithInitialSize(replaysCount - 1);
            Container.Bind<MenuControllersManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            InitReplayerOverridable(launchData.ReplayerBindings);

            //Players
            Container.BindMemoryPool<VirtualPlayer, VirtualPlayer.Pool>().WithInitialSize(replaysCount);
            Container.BindInterfacesTo<VirtualPlayersManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            //Event Processing
            Container.BindMemoryPool<ReplayBeatmapEventsProcessor, ReplayBeatmapEventsProcessor.Pool>().WithInitialSize(replaysCount);
            Container.BindMemoryPool<ReplayScoreEventsProcessor, ReplayScoreEventsProcessor.Pool>().WithInitialSize(replaysCount);
            Container.BindInterfacesTo<ReplayBeatmapEventsProcessorProxy>().AsSingle();
            Container.Bind<ReplayHeightsProcessor>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            //Notes handling
            Container.Bind<ReplayerScoreProcessor>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<ReplayerNotesCutter>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            //Tweaks and tools
            Container.Bind<BeatmapVisualsController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesTo<ReplayerCameraController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<TweaksHandler>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<HotkeysHandler>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ReplayWatermark>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            //UI
            if (EnvironmentUtils.UsesFPFC) {
                Container.Bind<ReplayerDesktopScreenSystem>().FromNewComponentOnNewGameObject().AsSingle();
                Container.Bind<ReplayerDesktopViewController>().FromNewComponentAsViewController().AsSingle();
                Container.Bind<ReplayerDesktopUIRenderer>().FromNewComponentOnNewGameObject().AsSingle().Lazy();
                Container.Bind<ReplayerDesktopUIBinder>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            } else {
                Container.Bind<ReplayerFloatingViewController>().FromNewComponentAsViewController().AsSingle();
                Container.Bind<ReplayerFloatingUIBinder>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            }

            Plugin.Log.Notice("[Installer] Replays system successfully installed!");
        }

        private void InitReplayerOverridable(ReplayerBindings? bindings) {
            // Comparator
            if (bindings?.ReplayComparator is { } comparator) {
                BindConcrete(comparator);
            } else {
                BindConcrete(ReplayDataUtils.BasicReplayNoteComparator);
            }

            // Body Settings 
            if (bindings?.BodySettingsFactory is { HasValue: true} bodySettings) {
                if (bodySettings.Value != null) {
                    BindConcrete(bodySettings.Value);
                } else {
                    Container.BindInterfacesTo<EmptyAvatarSettingsViewFactory>().AsSingle();
                }
            } else {
                Container.BindInterfacesTo<BasicAvatarSettingsViewFactory>().AsSingle();
            }
            
            // Body
            if (bindings?.BodySpawner is { } bodySpawner) {
                BindConcrete(bodySpawner);
            } else {
                Container.BindInterfacesTo<VirtualPlayerBodySpawner>().FromNewComponentOnNewGameObject().AsSingle();
            }
        }

        private void BindConcrete<T>(T instance) {
            Container.Bind<T>().To(instance!.GetType()).FromInstance(instance).AsSingle();
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