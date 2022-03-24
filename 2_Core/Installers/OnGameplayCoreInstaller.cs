using System.Reflection;
using BeatLeader.Core.Managers.ReplayEnhancer;
using HarmonyLib;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.Installers
{
    [UsedImplicitly]
    public class OnGameplayCoreInstaller : Installer<OnGameplayCoreInstaller>
    {
        public override void InstallBindings()
        {
            Plugin.Log.Debug("OnGameplayCoreInstaller");

            InitRecorder();
        }

        private void InitRecorder()
        {
            #region Gates
            if (ScoreSaber_playbackEnabled != null && (bool)ScoreSaber_playbackEnabled.Invoke(null, null) == false)
            {
                Plugin.Log.Debug("SS replay is running, BL Replay Recorder will not be started.");
                return;
            }
            if (!(MapEnhancer.previewBeatmapLevel.levelID.StartsWith(CustomLevelLoader.kCustomLevelPrefixId)))
            {
                Plugin.Log.Debug("OST level detected. No recording.");
                return;
            }
            #endregion

            Plugin.Log.Debug("Starting a BL Replay Recorder.");
            Container.BindInterfacesAndSelfTo<ReplayRecorder>().AsSingle();
            Container.BindInterfacesAndSelfTo<TrackingDeviceEnhancer>().AsTransient();
        }

        private static readonly MethodBase ScoreSaber_playbackEnabled = AccessTools.Method("ScoreSaber.Core.ReplaySystem.HarmonyPatches.PatchHandleHMDUnmounted:Prefix");
    }
}