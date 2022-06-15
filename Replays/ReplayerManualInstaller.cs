using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BeatLeader.Utils;
using BeatLeader.Replays;
using BeatLeader.Replays.Managers;
using BeatLeader.Replays.Emulating;
using BeatLeader.Replays.Movement;
using BeatLeader.Replays.Scoring;
using BeatLeader.Replays.Models;
using BeatLeader.Replays.UI;
using SiraUtil.Tools.FPFC;
using VRUIControls;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class ReplayerManualInstaller
    {
        public class InitData
        {
            public readonly bool compatibilityMode;
            public readonly bool noteSyncMode;
            public readonly bool movementLerp;
            public readonly int fieldOfView;

            public InitData(bool noteSyncMode, bool movementLerp, bool compatibilityMode)
            {
                this.noteSyncMode = noteSyncMode;
                this.movementLerp = movementLerp;
                this.compatibilityMode = compatibilityMode;
                this.fieldOfView = 0;
            }
            public InitData(bool noteSyncMode, bool movementLerp, bool compatibilityMode, int fieldOfView)
            {
                this.noteSyncMode = noteSyncMode;
                this.movementLerp = movementLerp;
                this.compatibilityMode = compatibilityMode;
                this.fieldOfView = fieldOfView;
            }
        }

        private readonly List<Type> scoreControllerBindings = new List<Type>()
        {
            typeof(RelativeScoreAndImmediateRankCounter),
            typeof(ScoreUIController),
            typeof(ScoreMissionObjectiveChecker),
            typeof(MultiplierValuesRecorder),
            typeof(PrepareLevelCompletionResults),
            typeof(MultiplayerLocalActiveClient),
            typeof(ScoreMultiplierUIController),
            typeof(NoteCutScoreSpawner),
            typeof(BeatmapObjectExecutionRatingsRecorder),
            typeof(VRsenalScoreLogger)
        };

        public void InstallBindings(BeatLeader.Models.Replay replay, InitData data, DiContainer Container)
        {
            Container.Bind<BeatLeader.Models.Replay>().FromInstance(replay).AsSingle();
            Container.Bind<InitData>().FromInstance(data).AsSingle();
            Container.Bind<SoftLocksController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<IScoringInterlayer>().To<ReplayToBaseScoringInterlayer>().AsSingle().NonLazy();
            Container.BindMemoryPool<SimpleCutScoringElement, SimpleCutScoringElement.Pool>().WithInitialSize(30);
            Container.BindMemoryPool<SimpleNoteCutComparator, SimpleNoteCutComparator.Pool>().WithInitialSize(30)
                .FromComponentInNewPrefab(new GameObject("ComparatorPrefab").AddComponent<SimpleNoteCutComparator>());
            if (data.noteSyncMode)
                Container.Bind<SimpleNoteComparatorsSpawner>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<BeatmapTimeController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<BeatmapEffectsController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            #region ScoreController patch
            ScoreController scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault();
            GameObject gameplayData = scoreController.gameObject;
            gameplayData.SetActive(false);

            GameObject.Destroy(scoreController);
            ReplayerScoreController modifiedScoreController = gameplayData.AddComponent<ReplayerScoreController>();
            Container.Inject(modifiedScoreController);

            Container.Rebind<IScoreController>().FromInstance(modifiedScoreController);
            ZenjectExtension.InjectAllFieldsOfTypeOnFindedGameObjects<IScoreController>(scoreControllerBindings, Container);

            modifiedScoreController.SetEnabled(true);
            gameplayData.SetActive(true);
            #endregion

            Container.Bind<InputManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            if (!data.compatibilityMode)
                Container.Bind<SimpleCutScoreEffectSpawner>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<VRControllersManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            //Container.Bind<SimpleAvatarController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<Replayer>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<PlaybackController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Container.Bind<SceneTweaksManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<ReplayerCameraController.InitData>().FromInstance(new ReplayerCameraController.InitData(data.fieldOfView, "FreeView",

                new StaticCameraPose("LeftView", new Vector3(-3.70f, 2.30f, -1.10f), Quaternion.Euler(new Vector3(0, 60, 0))),
                new StaticCameraPose("RightView", new Vector3(3.70f, 2.30f, -1.10f), Quaternion.Euler(new Vector3(0, -60, 0))),
                new StaticCameraPose("BehindView", new Vector3(0f, 1.9f, -2f), Quaternion.Euler(new Vector3(0, 0, 0))),
                new StaticCameraPose("CenterView", new Vector3(0f, 1.7f, 0f), Quaternion.Euler(new Vector3(0, 0, 0)), InputManager.InputSystemType.FPFC),
                new StaticCameraPose("CenterView", new Vector3(0f, 0f, 0f), Quaternion.Euler(new Vector3(0, 0, 0)), InputManager.InputSystemType.VR),

                new FlyingCameraPose(new Vector2(0.5f, 0.5f), 4, true, "FreeView"),
                new PlayerViewCameraPose(3)

                )).AsSingle();
            Container.Bind<ReplayerCameraController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<UI2DManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<MultiplatformUIManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            if (Container.Resolve<InputManager>().currentInputSystem == InputManager.InputSystemType.FPFC)
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
            Plugin.Log.Notice("Replay system successfully installed!");
        }
        public static void Install(BeatLeader.Models.Replay replay, InitData data, DiContainer Container)
        {
            new ReplayerManualInstaller().InstallBindings(replay, data, Container);
        }
    }
}
