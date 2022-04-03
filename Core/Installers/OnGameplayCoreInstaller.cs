using System.Reflection;
using BeatLeader.UI;
using BeatLeader.Replays.Models;
using BeatLeader.Replays;
using BeatLeader.Replays.ReplayEnhancers;
using BeatLeader.Utils.Expansions;
using BeatLeader.Utils;
using HarmonyLib;
using JetBrains.Annotations;
using Zenject;
using UnityEngine;

namespace BeatLeader.Installers
{
    [UsedImplicitly]
    public class OnGameplayCoreInstaller : Installer<OnGameplayCoreInstaller>
    {
        private static readonly MethodBase ScoreSaber_playbackEnabled = AccessTools.Method("ScoreSaber.Core.ReplaySystem.HarmonyPatches.PatchHandleHMDUnmounted:Prefix");

        public override void InstallBindings()
        {
            Plugin.Log.Debug("OnGameplayCoreInstaller");

            if (!ReplayUI.isStartedAsReplay)
                InitRecorder();
            else
                InitPlayer();

        }
        private void InitPlayer()
        {
            Debug.LogWarning($"modifiers - {ReplayUI.replayData.info.modifiers}");
            Container.Bind<Replay>().FromInstance(ReplayUI.replayData).AsSingle();
            Container.Bind<ReplayPlayer>().AsSingle();
            MovementPatchHelper.InstallPatch();
            MovementPatchHelper.player.InjectAllFields(Container);
            EventsHandler.MenuSceneLoaded += MovementPatchHelper.UninstallPatch;
        }
        private void InitRecorder()
        {
            #region Gates
            if (ScoreSaber_playbackEnabled != null && (bool)ScoreSaber_playbackEnabled.Invoke(null, null) == false)
            {
                Plugin.Log.Debug("SS replay is running, BL Replay Recorder will not be started.");
                return;
            }
            if (!(MapEnhancer.previewBeatmapLevel.levelID.StartsWith("custom_level_")))
            {
                Plugin.Log.Debug("OST level detected. No recording.");
                return;
            }
            #endregion

            Plugin.Log.Debug("Starting a BL Replay Recorder.");
            Container.BindInterfacesAndSelfTo<ReplayRecorder>().AsSingle();
            Container.BindInterfacesAndSelfTo<TrackingDeviceEnhancer>().AsTransient();
        }
    }
}