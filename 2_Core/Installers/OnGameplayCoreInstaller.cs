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
    [UsedImplicitly]
    public class OnGameplayCoreInstaller : Installer<OnGameplayCoreInstaller> 
    {
        public override void InstallBindings() 
        {
            Plugin.Log.Debug("OnGameplayCoreInstaller");

            InitPlayer();
            InitRecorder();
        }
        private void InitRecorder() 
        {
            if (ReplaySystemHelper.asReplay) return;
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
            if (!ReplaySystemHelper.asReplay) return;
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

            var player = new GameObject("ReplayPlayer").AddComponent<ReplayPlayer>().InjectAllFields(Container);
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
            typeof(ReplayPlayer)
        };
        private static readonly MethodBase ScoreSaber_playbackEnabled = AccessTools.Method("ScoreSaber.Core.ReplaySystem.HarmonyPatches.PatchHandleHMDUnmounted:Prefix");
    }
}