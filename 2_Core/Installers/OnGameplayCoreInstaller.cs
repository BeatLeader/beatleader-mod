using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BeatLeader.Utils;
using BeatLeader.Replays;
using BeatLeader.Replays.Emulators;
using BeatLeader.Core.Managers.ReplayEnhancer;
using HarmonyLib;
using JetBrains.Annotations;
using Zenject;
using UnityEngine;

namespace BeatLeader.Installers 
{
    public class OnGameplayCoreInstaller : Installer<OnGameplayCoreInstaller> 
    {
        private static readonly MethodBase ScoreSaber_playbackEnabled = AccessTools.Method("ScoreSaber.Core.ReplaySystem.HarmonyPatches.PatchHandleHMDUnmounted:Prefix");
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
            Plugin.Log.Debug("OnGameplayCoreInstaller");

            InitPlayer();
            InitRecorder();
        }
        private void InitRecorder() 
        {
            if (ReplayMenuUI.asReplay) return;
            #region Gates
            if (ScoreSaber_playbackEnabled != null && (bool)ScoreSaber_playbackEnabled.Invoke(null, null) == false) {
                Plugin.Log.Warn("SS replay is running, BL Replay Recorder will not be started!");
                return;
            }
            if (!(MapEnhancer.previewBeatmapLevel.levelID.StartsWith(CustomLevelLoader.kCustomLevelPrefixId))) {
                Plugin.Log.Notice("OST level detected! Recording unavailable!");
                return;
            }
            #endregion

            Plugin.Log.Debug("Starting a BL Replay Recorder.");

            Container.BindInterfacesAndSelfTo<ReplayRecorder>().AsSingle();
            Container.BindInterfacesAndSelfTo<TrackingDeviceEnhancer>().AsTransient();
        }
        private void InitPlayer()
        {
            if (!ReplayMenuUI.asReplay) return;

            Container.Bind<SimpleFlyingScoreSpawner>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindMemoryPool<ReplayCutScoringElement, ReplayCutScoringElement.Pool>().WithInitialSize(30);

            //var scoreSpawner = Resources.FindObjectsOfTypeAll<NoteCutScoreSpawner>().FirstOrDefault();
            //if (scoreSpawner != null)
            //{
            //    GameObject.Destroy(scoreSpawner);
            //}

            var scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault();
            var gameplayData = scoreController.gameObject;
            gameplayData.SetActive(false);

            GameObject.Destroy(scoreController);
            var modifiedScoreController = gameplayData.AddComponent<ReplayScoreController>().InjectAllFields(Container);

            Container.Rebind<IScoreController>().FromInstance(modifiedScoreController);
            ZenjectExpansion.InjectAllFieldsOfTypeOnFindedGameObjects<IScoreController>(scoreControllerBindings, Container);
            Container.Bind<ReplayPlayer>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            modifiedScoreController.SetEnabled(true);
            gameplayData.SetActive(true);
        }
    }
}