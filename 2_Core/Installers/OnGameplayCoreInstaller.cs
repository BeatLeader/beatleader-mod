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
using HarmonyLib;
using IPA.Utilities;
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
        };

        public override void InstallBindings()
        {
            if (ReplayMenuUI.asReplay)
            {
                new ReplayManualInstaller().ManualInstall(ReplayMenuUI.replay, 
                    new ReplayManualInstaller.InitData(true, true, false, true), Container);
            }
            else InitRecorder();
        }
        private void InitRecorder()
        {
            #region Gates
            if (ScoreSaber_playbackEnabled != null && (bool)ScoreSaber_playbackEnabled.Invoke(null, null) == false)
            {
                Plugin.Log.Warn("SS replay is running, BL Replay Recorder will not be started!");
                return;
            }
            if (!(MapEnhancer.previewBeatmapLevel.levelID.StartsWith(CustomLevelLoader.kCustomLevelPrefixId)))
            {
                Plugin.Log.Notice("OST level detected! Recording unavailable!");
                return;
            }
            #endregion

            Plugin.Log.Debug("Starting a BL Replay Recorder.");

            Container.BindInterfacesAndSelfTo<ReplayRecorder>().AsSingle();
            Container.BindInterfacesAndSelfTo<TrackingDeviceEnhancer>().AsTransient();
        }
    }
}