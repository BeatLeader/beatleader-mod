using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BeatLeader.Utils;
using BeatLeader.Models;
using BeatLeader.Replays;
using BeatLeader.Replays.Managers;
using BeatLeader.Replays.MapEmitating;
using BeatLeader.Replays.Movement;
using BeatLeader.Replays.Scoring;
using VRUIControls;
using IPA.Utilities;
using Zenject;
using UnityEngine;

namespace BeatLeader.Replays
{
    public class ReplayManualInstaller
    {
        public class InitData
        {
            public readonly bool compatibilityMode;
            public readonly bool noteSyncMode;
            public readonly bool movementLerp;
            public readonly bool overrideCamera; //works only in fpfc
            public readonly bool forceRefreshCamera; //allows to fix problems with camera re-enabling caused by RUE or another thing
            public readonly int fieldOfView; 
            public readonly int smoothness; //0-10, 0 - disabled

            public InitData(bool noteSyncMode, bool movementLerp, bool compatibilityMode)
            {
                this.noteSyncMode = noteSyncMode;
                this.movementLerp = movementLerp;
                this.compatibilityMode = compatibilityMode;
                this.overrideCamera = false;
                this.fieldOfView = 0;
                this.smoothness = 0;
                this.forceRefreshCamera = false;
            }
            public InitData(bool noteSyncMode, bool movementLerp, bool compatibilityMode, bool overrideCamera, int fieldOfView, int smoothness, bool forceRefreshCamera)
            {
                this.noteSyncMode = noteSyncMode;
                this.movementLerp = movementLerp;
                this.compatibilityMode = compatibilityMode;
                this.overrideCamera = overrideCamera;
                this.fieldOfView = fieldOfView;
                this.smoothness = smoothness;
                this.forceRefreshCamera = forceRefreshCamera;
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
            typeof(VRsenalScoreLogger),
        };

        public void InstallBindings(Replay replay, InitData data, DiContainer Container)
        {
            Container.Bind<Replay>().FromInstance(replay).AsSingle();
            Container.Bind<InitData>().FromInstance(data).AsSingle();
            Container.BindMemoryPool<SimpleCutScoringElement, SimpleCutScoringElement.Pool>().WithInitialSize(30);
            Container.BindMemoryPool<SimpleScoringInterlayer, SimpleScoringInterlayer.Pool>().WithInitialSize(30);
            Container.BindMemoryPool<SimpleNoteCutComparator, SimpleNoteCutComparator.Pool>().WithInitialSize(50)
                .FromComponentInNewPrefab(new GameObject("ComparatorPrefab")
                .AddComponent<SimpleNoteCutComparator>());
            if (data.noteSyncMode)
                Container.Bind<SimpleNoteComparatorsSpawner>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            #region ScoreController patch
            var scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault();
            var gameplayData = scoreController.gameObject;
            gameplayData.SetActive(false);

            GameObject.Destroy(scoreController);
            var modifiedScoreController = gameplayData.AddComponent<ReplayScoreController>().InjectAllFields(Container);

            Container.Rebind<IScoreController>().FromInstance(modifiedScoreController);
            ZenjectExtension.InjectAllFieldsOfTypeOnFindedGameObjects<IScoreController>(scoreControllerBindings, Container);

            modifiedScoreController.SetEnabled(true);
            gameplayData.SetActive(true);
            #endregion

            Container.Bind<InputManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<MenuSabersManager>().FromNewComponentOnNewGameObject().AsSingle();
            if (!data.compatibilityMode)
                Container.Bind<SimpleCutScoreEffectSpawner>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<SimpleTimeController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<BodyManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            //Container.Bind<SimpleAvatarController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<Replayer>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<PlaybackController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Container.Bind<SceneTweaksManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<PlayerCameraController.InitData>().FromInstance(
                new PlayerCameraController.InitData(data.smoothness, data.fieldOfView, 
                data.forceRefreshCamera, data.overrideCamera)).AsSingle();
            Container.Bind<PlayerCameraController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<MultiplatformUIManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            if (Container.Resolve<InputManager>().currentInputSystem == InputManager.InputSystemType.FPFC)
            {
                Container.Bind<PlaybackNonVRViewController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            }
            else
            {
                Container.Bind<PlaybackVRViewController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            }
        }
        public static void Install(Replay replay, InitData data, DiContainer Container)
        {
            new ReplayManualInstaller().InstallBindings(replay, data, Container);
        }
    }
}
