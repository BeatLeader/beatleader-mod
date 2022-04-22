using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BeatLeader.Utils;
using BeatLeader.Models;
using BeatLeader.Replays;
using BeatLeader.Replays.Tools;
using BeatLeader.Replays.Movement;
using BeatLeader.Replays.Scoring;
using BeatLeader.Core.Managers.ReplayEnhancer;
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
            public readonly bool generateInRealTime;
            public readonly bool generateAsync;
            public readonly bool movementLerp;

            public InitData(bool compatibilityMode, bool generateInRealTime, bool generateAsync, bool noteSyncMode, bool movementLerp)
            {
                this.compatibilityMode = compatibilityMode;
                this.generateInRealTime = generateInRealTime;
                this.generateAsync = generateAsync;
                this.noteSyncMode = noteSyncMode;
                this.movementLerp = movementLerp;
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

        public void ManualInstall(Replay replay, InitData data, DiContainer Container)
        {
            Container.Bind<Replay>().FromInstance(replay).AsSingle();
            Container.Bind<InitData>().FromInstance(data).AsSingle();
            Container.BindMemoryPool<SimpleCutScoringElement, SimpleCutScoringElement.Pool>().WithInitialSize(30);
            Container.BindMemoryPool<SimpleScoringInterlayer, SimpleScoringInterlayer.Pool>().WithInitialSize(30);

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

            Container.Bind<MenuSabersManager>().FromNewComponentOnNewGameObject().AsSingle();
            Container.Bind<SimpleNoteComparatorsSpawner>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            if (!data.compatibilityMode) Container.Bind<SimpleCutScoreEffectSpawner>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<MovementManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<SimpleAvatarController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<Replayer>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<PlaybackController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Container.BindFactory<SimpleNoteCutComparator, SimpleNoteCutComparator.Factory>()
                .FromComponentInNewPrefab(new GameObject("ComparatorPrefab")
                .AddComponent<SimpleNoteCutComparator>());
        }
    }
}
