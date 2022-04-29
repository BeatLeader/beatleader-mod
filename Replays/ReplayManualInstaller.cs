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
            if (Container.Resolve<IVRPlatformHelper>().GetType() == typeof(DevicelessVRHelper))
            {
                Container.Bind<PlayerViewController.InitData>()
                    .FromInstance(new PlayerViewController.InitData(data.smoothness, data.fieldOfView, data.forceRefreshCamera)).AsSingle();
                Container.Bind<PlayerViewController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                Resources.FindObjectsOfTypeAll<VRLaserPointer>().First().gameObject.SetActive(false);
                Resources.FindObjectsOfTypeAll<SaberBurnMarkArea>().First().gameObject.SetActive(false); //fpfc burn marks does not disappearing base game bug, bg, please, fix it
            }
            else Container.Bind<ReplayPlaybackUI>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            //Container.Bind<NonVRUIManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy(); //fpfc form

            #region ScoreController patch
            var scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault();
            var gameplayData = scoreController.gameObject;
            gameplayData.SetActive(false);

            GameObject.Destroy(scoreController);
            var modifiedScoreController = gameplayData.AddComponent<ReplayScoreController>().InjectAllFields(Container);

            Container.Rebind<IScoreController>().FromInstance(modifiedScoreController);
            ZenjectExpansion.InjectAllFieldsOfTypeOnFindedGameObjects<IScoreController>(scoreControllerBindings, Container);

            modifiedScoreController.SetEnabled(true);
            gameplayData.SetActive(true);
            #endregion

            Container.Bind<PauseMenuSabersManager>().FromNewComponentOnNewGameObject().AsSingle();
            if (!data.compatibilityMode) 
                Container.Bind<SimpleCutScoreEffectSpawner>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<MovementManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<SimpleAvatarController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<Replayer>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<PlaybackController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
        }
        public static void Install(Replay replay, InitData data, DiContainer Container)
        {
            new ReplayManualInstaller().InstallBindings(replay, data, Container);
        }
    }
}
