using System;
using System.Linq;
using System.Reflection;
using BeatLeader.Utils;
using BeatLeader.Replays.Poses;
using BeatLeader.Replays.Managers;
using BeatLeader.Replays.Emulating;
using BeatLeader.Replays.Movement;
using BeatLeader.Replays.Scoring;
using BeatLeader.ViewControllers;
using BeatLeader.Models;
using SiraUtil.Tools.FPFC;
using UnityEngine;
using Zenject;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Pose = UnityEngine.Pose;

namespace BeatLeader.Replays
{
    public class ReplayerManualInstaller
    {
        public class InitData
        {
            public readonly bool movementLerp;
            public readonly int fieldOfView;

            public InitData(bool movementLerp)
            {
                this.movementLerp = movementLerp;
                this.fieldOfView = 0;
            }
            public InitData(bool movementLerp, int fieldOfView)
            {
                this.movementLerp = movementLerp;
                this.fieldOfView = fieldOfView;
            }
        }

        public void InstallBindings(Replay replay, Score score, InitData data, DiContainer Container)
        {
            Container.Bind<Replay>().FromInstance(replay).AsSingle();
            Container.Bind<Score>().FromInstance(score).AsSingle();
            Container.Bind<InitData>().FromInstance(data).AsSingle();
            Container.Bind<SoftLocksController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<IScoringInterlayer>().To<ReplayToBaseScoringInterlayer>().AsSingle().NonLazy();
            Container.BindMemoryPool<SimpleNoteCutComparator, SimpleNoteCutComparator.Pool>().WithInitialSize(30)
                .FromComponentInNewPrefab(new GameObject("ComparatorPrefab").AddComponent<SimpleNoteCutComparator>());
            Container.Bind<SimpleNoteComparatorsSpawner>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<BeatmapTimeController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            #region ScoreController patch
            ScoreController scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault();
            GameObject gameplayData = scoreController.gameObject;
            gameplayData.SetActive(false);

            GameObject.Destroy(scoreController);
            ReplayerScoreController modifiedScoreController = gameplayData.AddComponent<ReplayerScoreController>();
            Container.Inject(modifiedScoreController);

            Container.Rebind<IScoreController>().FromInstance(modifiedScoreController);
            Container.Bind<IReplayerScoreController>().To<ReplayerScoreController>().FromInstance(modifiedScoreController).AsSingle().NonLazy();
            Container.Reinject<IScoreController>();

            modifiedScoreController.SetEnabled(true);
            gameplayData.SetActive(true);
            #endregion

            Container.Bind<InputManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<BeatmapVisualsController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<VRControllersManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<VRControllersMovementEmulator>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<PlaybackController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Container.Bind<SceneTweaksManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<ReplayerCameraController.InitData>().FromInstance(new ReplayerCameraController.InitData(data.fieldOfView,

                new StaticCameraPose("LeftView", new Vector3(-3.70f, 2.30f, -1.10f), Quaternion.Euler(new Vector3(0, 60, 0))),
                new StaticCameraPose("RightView", new Vector3(3.70f, 2.30f, -1.10f), Quaternion.Euler(new Vector3(0, -60, 0))),
                new StaticCameraPose("BehindView", new Vector3(0f, 1.9f, -2f), Quaternion.Euler(Vector3.zero)),
                new StaticCameraPose("CenterView", new Vector3(0f, 1.7f, 0f), Quaternion.Euler(Vector3.zero), InputManager.InputType.FPFC),
                new StaticCameraPose("CenterView", Vector3.zero, Quaternion.Euler(Vector3.zero), InputManager.InputType.VR),

                new FlyingCameraPose(new Vector2(0.5f, 0.5f), new Vector2(0, 1.7f), 4, true, "FreeView"),
                new PlayerViewCameraPose(3)

                )).AsSingle();
            Container.Bind<ReplayerCameraController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<UI2DManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesTo<GameSettingsLoader>().AsSingle().Lazy();

            if (Container.Resolve<InputManager>().IsInFPFC)
            {
                Container.Bind<ReplayerPCViewController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
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
            //else Container.Bind<> vr view
            Plugin.Log.Notice("Replay system successfully installed!");
        }
        public static void Install(Replay replay, Score score, InitData data, DiContainer Container)
        {
            new ReplayerManualInstaller().InstallBindings(replay, score, data, Container);
        }
    }
}
