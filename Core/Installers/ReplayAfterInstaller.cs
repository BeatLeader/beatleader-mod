using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Replays.Models;
using BeatLeader.Replays;
using BeatLeader.UI;
using BeatLeader.Utils;
using BeatLeader.Replays.Emulators;
using BeatLeader.Utils.Expansions;
using Zenject;
using UnityEngine;

namespace BeatLeader.Installers
{
    internal class ReplayAfterInstaller : Installer<ReplayAfterInstaller>
    {
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
            typeof(ReplayPlayer)
        };

        public override void InstallBindings()
        {
            if (!ReplayMenuUI.isStartedAsReplay) return;
            // InitPlayer();
            Container.BindMemoryPool<ReplayCutScoringElement, ReplayCutScoringElement.Pool>().WithInitialSize(30);
            var scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault();
            var gameplayData = scoreController.gameObject;
            gameplayData.SetActive(false);

            GameObject.Destroy(scoreController);
            var modifiedScoreController = gameplayData.AddComponent<ReplayScoreController>().InjectAllFields(Container);

            Container.Rebind<IScoreController>().FromInstance(modifiedScoreController);
            ZenjectExpansion.InjectAllFieldsOfTypeOnFindedGameObjects<IScoreController>(scoreControllerBindings, Container);

            modifiedScoreController.SetEnabled(true);
            gameplayData.SetActive(true);
        }
    }
}
