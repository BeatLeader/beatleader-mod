using System;
using System.Linq;
using System.Reflection;
using BeatLeader.Utils;
using BeatLeader.Replayer.Poses;
using BeatLeader.Replayer.Managers;
using BeatLeader.Replayer.Emulating;
using BeatLeader.Replayer.Movement;
using BeatLeader.Replayer.Scoring;
using BeatLeader.ViewControllers;
using BeatLeader.Models;
using SiraUtil.Tools.FPFC;
using UnityEngine;
using Zenject;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Pose = UnityEngine.Pose;

namespace BeatLeader.Replayer
{
    public class ReplayerManualInstaller
    {
        public static void Install(Replay replay, Score score, DiContainer Container)
        {
            new ReplayerManualInstaller().InstallBindings(replay, score, Container);
        }

        public void InstallBindings(Replay replay, Score score, DiContainer Container)
        {
            Container.Bind<Replay>().FromInstance(replay).AsSingle();
            Container.Bind<Score>().FromInstance(score).AsSingle();
            Container.Bind<SoftLocksController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<IScoringInterlayer>().To<ReplayToBaseScoringInterlayer>().AsSingle().NonLazy();
            Container.BindMemoryPool<SimpleNoteCutComparator, SimpleNoteCutComparator.Pool>().WithInitialSize(30)
               .FromComponentInNewPrefab(new GameObject("ComparatorPrefab").AddComponent<SimpleNoteCutComparator>());
            Container.Bind<SimpleNoteComparatorsSpawner>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<BeatmapTimeController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            //scorecontroller patch
            Container.Bind<IReplayerScoreController>().To<ReplayerScoreController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Rebind<IScoreController>().To<IReplayerScoreController>().FromResolve().AsSingle().NonLazy();
            Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault().TryDestroy();

            Container.Bind<InputManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<BeatmapVisualsController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<VRControllersManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<VRControllersMovementEmulator>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<PlaybackController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Container.Bind<SceneTweaksManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<ReplayerCameraController.InitData>().FromInstance(new ReplayerCameraController.InitData(

                new StaticCameraPose(0, "LeftView", new Vector3(-3.70f, 2.30f, -1.10f), Quaternion.Euler(new Vector3(0, 60, 0))),
                new StaticCameraPose(1, "RightView", new Vector3(3.70f, 2.30f, -1.10f), Quaternion.Euler(new Vector3(0, -60, 0))),
                new StaticCameraPose(2, "BehindView", new Vector3(0f, 1.9f, -2f), Quaternion.Euler(Vector3.zero)),
                new StaticCameraPose(3, "CenterView", new Vector3(0f, 1.7f, 0f), Quaternion.Euler(Vector3.zero), InputManager.InputType.FPFC),
                new StaticCameraPose(3, "CenterView", Vector3.zero, Quaternion.Euler(Vector3.zero), InputManager.InputType.VR),

                new FlyingCameraPose(new Vector2(0.5f, 0.5f), new Vector2(0, 1.7f), 4, true, "FreeView"),
                new PlayerViewCameraPose(3)

                )).AsSingle();
            Container.Bind<ReplayerCameraController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<UI2DManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesTo<GameSettingsLoader>().AsSingle().Lazy();

            InstallUI(Container, !Container.Resolve<InputManager>().IsInFPFC);
            Plugin.Log.Notice("Replay system successfully installed!");
        }
        private static void InstallUI(DiContainer container, bool vr)
        {
            if (vr)
                container.Bind<ReplayerVRViewController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            else
            {
                container.Bind<Camera2Patcher>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                container.Bind<ReplayerPCViewController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                PatchSiraFreeView(container);
            }
        }
        private static void PatchSiraFreeView(DiContainer container)
        {
            try
            {
                container.Resolve<IFPFCSettings>().Enabled = false;
                Assembly assembly = typeof(IFPFCSettings).Assembly;

                Type smoothCameraListenerType = assembly.GetType("SiraUtil.Tools.FPFC.SmoothCameraListener");
                Type FPFCToggleType = assembly.GetType("SiraUtil.Tools.FPFC.FPFCToggle");
                Type simpleCameraControllerType = assembly.GetType("SiraUtil.Tools.FPFC.SimpleCameraController");

                container.Unbind<IFPFCSettings>();
                container.UnbindInterfacesTo(smoothCameraListenerType);
                container.UnbindInterfacesTo(FPFCToggleType);
                //GameObject.Destroy((UnityEngine.Object)Container.TryResolve(simpleCameraControllerType));
            }
            catch (Exception ex)
            {
                Plugin.Log.Critical($"An unhandled exception occurred during attemping to remove Sira's FPFC system! {ex}");
            }
        }
    }
}
