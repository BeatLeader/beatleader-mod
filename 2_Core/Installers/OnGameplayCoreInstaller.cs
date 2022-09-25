using System.Collections.Generic;
using System.Reflection;
using BeatLeader.Replayer;
using BeatLeader.Core.Managers.ReplayEnhancer;
using BeatLeader.Utils;
using HarmonyLib;
using SiraUtil.Submissions;
using Zenject;
using BeatLeader.Replayer.Camera;
using UnityEngine;
using BeatLeader.Replayer.Emulation;
using BeatLeader.Replayer.Movement;
using BeatLeader.Components;
using BeatLeader.ViewControllers;
using SiraUtil.Tools.FPFC;
using System;
using System.ComponentModel;

namespace BeatLeader.Installers
{
    public class OnGameplayCoreInstaller : Installer<OnGameplayCoreInstaller>
    {
        private static readonly List<string> modes = new() { "Solo", "Multiplayer" };

        public override void InstallBindings()
        {
            Plugin.Log.Debug("OnGameplayCoreInstaller");
            if (ReplayerLauncher.IsStartedAsReplay) InitReplayer();
            else InitRecorder();
        }

        private void InitRecorder()
        {
            RecorderUtils.buffer = RecorderUtils.shouldRecord;

            if (RecorderUtils.shouldRecord)
            {
                RecorderUtils.shouldRecord = false;

                #region Gates
                if (ScoreSaber_playbackEnabled != null && (bool)ScoreSaber_playbackEnabled.Invoke(null, null) == false)
                {
                    Plugin.Log.Debug("SS replay is running, BL Replay Recorder will not be started.");
                    return;
                }
                if (!(MapEnhancer.previewBeatmapLevel.levelID.StartsWith(CustomLevelLoader.kCustomLevelPrefixId)))
                {
                    Plugin.Log.Debug("OST level detected. No recording.");
                    return;
                }
                var gameMode = GameMode();
                if (gameMode != null && !modes.Contains(gameMode))
                {
                    Plugin.Log.Debug("Not allowed game mode");
                    return;
                }
                #endregion

                Plugin.Log.Debug("Starting a BL Replay Recorder.");

                Container.BindInterfacesAndSelfTo<ReplayRecorder>().AsSingle();
                Container.BindInterfacesAndSelfTo<TrackingDeviceEnhancer>().AsTransient();
            }
            else
            {
                //Plugin.Log.Info("Unknown flow detected, recording would not be started.");
            }
        }

        #region Replayer

        private static readonly ReplayerCameraController.InitData CameraInitData = new ReplayerCameraController.InitData(

           new StaticCameraPose(0, "LeftView", new Vector3(-3.70f, 2.30f, -1.10f), Quaternion.Euler(new Vector3(0, 60, 0)), InputManager.InputType.FPFC),
           new StaticCameraPose(0, "LeftView", new Vector3(-3.70f, 0, -1.10f), Quaternion.Euler(new Vector3(0, 60, 0)), InputManager.InputType.VR),
           new StaticCameraPose(1, "RightView", new Vector3(3.70f, 2.30f, -1.10f), Quaternion.Euler(new Vector3(0, -60, 0)), InputManager.InputType.FPFC),
           new StaticCameraPose(1, "RightView", new Vector3(3.70f, 0, -1.10f), Quaternion.Euler(new Vector3(0, -60, 0)), InputManager.InputType.VR),
           new StaticCameraPose(2, "BehindView", new Vector3(0f, 1.9f, -2f), Quaternion.identity, InputManager.InputType.FPFC),
           new StaticCameraPose(2, "BehindView", new Vector3(0, 0, -2), Quaternion.identity, InputManager.InputType.VR),
           new StaticCameraPose(3, "CenterView", new Vector3(0f, 1.7f, 0f), Quaternion.identity, InputManager.InputType.FPFC),
           new StaticCameraPose(3, "CenterView", Vector3.zero, Quaternion.identity, InputManager.InputType.VR),

           new FlyingCameraPose(new Vector2(0.5f, 0.5f), new Vector2(0, 1.7f), 4, true, "FreeView"),
           new PlayerViewCameraPose(3)

        );

        private void InitReplayer()
        {
            DisableScoreSubmission();

            Container.Bind<Models.ReplayLaunchData>().FromInstance(ReplayerLauncher.LaunchData).AsSingle().Lazy();
            Container.Bind<BeatmapTimeController>().FromNewComponentOnNewGameObject().AsSingle().Lazy();

            Container.BindInterfacesAndSelfTo<ReplayEventsProcessor>().AsSingle().NonLazy();
            Container.Bind<ReplayerScoreProcessor>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<ReplayerNotesCutter>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Container.Bind<BeatmapVisualsController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<VRControllersProvider>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<VRControllersMovementEmulator>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<PlaybackController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Container.Bind<ReplayerCameraController.InitData>().FromInstance(CameraInitData).AsSingle().Lazy();
            Container.Bind<ReplayerCameraController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesTo<SceneTweaksManager>().AsSingle().NonLazy();
            Container.BindInterfacesTo<SettingsLoader>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<HotkeysManager>().AsSingle().NonLazy();
            Container.Bind<ReplayWatermark>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            if (InputManager.IsInFPFC)
            {
                Container.Bind<UI2DManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                Container.Bind<ReplayerPCViewController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                PatchSiraFreeView();
            }
            else Container.Bind<ReplayerVRViewController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Plugin.Log.Notice("[Installer] Replay system successfully installed!");
        }
        private void DisableScoreSubmission()
        {
            var submission = Container.Resolve<Submission>();
            submission.DisableScoreSubmission("BeatLeaderReplayer", "Playback");
        }
        private void PatchSiraFreeView()
        {
            try
            {
                Container.Resolve<IFPFCSettings>().Enabled = false;
                Assembly assembly = typeof(IFPFCSettings).Assembly;

                Type smoothCameraListenerType = assembly.GetType("SiraUtil.Tools.FPFC.SmoothCameraListener");
                Type FPFCToggleType = assembly.GetType("SiraUtil.Tools.FPFC.FPFCToggle");
                Type simpleCameraControllerType = assembly.GetType("SiraUtil.Tools.FPFC.SimpleCameraController");

                Container.Unbind<IFPFCSettings>();
                Container.UnbindInterfacesTo(smoothCameraListenerType);
                Container.UnbindInterfacesTo(FPFCToggleType);
                //GameObject.Destroy((UnityEngine.Object)Container.TryResolve(simpleCameraControllerType));
            }
            catch (Exception ex)
            {
                Plugin.Log.Critical($"An unhandled exception occurred during attemping to remove Sira's FPFC system! {ex}");
            }
        }

        #endregion

        #region Game mode stuff

        private string GameMode()
        {
            string singleGM = GetStandardDataSO()?.gameMode;
            string multiGM = GetMultiDataSO()?.gameMode;

            foreach (string mode in new[] { singleGM, multiGM })
            {
                if (mode != null && mode.Length > 0)
                {
                    return mode;
                }
            }

            return null;
        }

        private StandardLevelScenesTransitionSetupDataSO GetStandardDataSO()
        {
            try
            {
                return Container.TryResolve<StandardLevelScenesTransitionSetupDataSO>();
            }
            catch
            {
                return null;
            }
        }

        private MultiplayerLevelScenesTransitionSetupDataSO GetMultiDataSO()
        {
            try
            {
                return Container.TryResolve<MultiplayerLevelScenesTransitionSetupDataSO>();
            }
            catch
            {
                return null;
            }
        }

        #endregion

        // TODO: remove this after verifying of an "allowed flows" strategy
        private static readonly MethodBase ScoreSaber_playbackEnabled = AccessTools.Method("ScoreSaber.Core.ReplaySystem.HarmonyPatches.PatchHandleHMDUnmounted:Prefix");
    }
}