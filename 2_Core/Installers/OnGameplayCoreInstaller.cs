using System.Reflection;
using BeatLeader.Core.Managers.ReplayEnhancer;
using BeatLeader.Utils;
using HarmonyLib;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.Installers {
    [UsedImplicitly]
    public class OnGameplayCoreInstaller : Installer<OnGameplayCoreInstaller> {
        public override void InstallBindings() {
            Plugin.Log.Debug("OnGameplayCoreInstaller");

            InitRecorder();
        }

        private void InitRecorder() {
            RecorderUtils.buffer = RecorderUtils.shouldRecord;

            if (RecorderUtils.shouldRecord) {
                RecorderUtils.shouldRecord = false;

                #region Gates
                if (ScoreSaber_playbackEnabled != null && (bool)ScoreSaber_playbackEnabled.Invoke(null, null) == false) {
                    Plugin.Log.Debug("SS replay is running, BL Replay Recorder will not be started.");
                    return;
                }
                if (!(MapEnhancer.previewBeatmapLevel.levelID.StartsWith(CustomLevelLoader.kCustomLevelPrefixId))) {
                    Plugin.Log.Debug("OST level detected. No recording.");
                    return;
                }
                if (GetSetupDataSO()?.gameMode != "Solo") {
                    Plugin.Log.Debug("Not a \"Solo\" game mode");
                    return;
                }
                #endregion

                Plugin.Log.Debug("Starting a BL Replay Recorder.");

                Container.BindInterfacesAndSelfTo<ReplayRecorder>().AsSingle();
                Container.BindInterfacesAndSelfTo<TrackingDeviceEnhancer>().AsTransient();
            } else {
                //Plugin.Log.Info("Unknown flow detected, recording would not be started.");
            }
        }

        private StandardLevelScenesTransitionSetupDataSO GetSetupDataSO() {
            try {
                return Container.TryResolve<StandardLevelScenesTransitionSetupDataSO>();
            } catch {
                return null;
            }
        }

        // TODO: remove this after verifying of an "allowed flows" strategy
        private static readonly MethodBase ScoreSaber_playbackEnabled = AccessTools.Method("ScoreSaber.Core.ReplaySystem.HarmonyPatches.PatchHandleHMDUnmounted:Prefix");
    }
}