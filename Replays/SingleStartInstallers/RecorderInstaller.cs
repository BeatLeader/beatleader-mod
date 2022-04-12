using System.Reflection;
using BeatLeader.Replays.ReplayEnhancers;
using HarmonyLib;
using Zenject;

namespace BeatLeader.Replays.SingleStartInstallers
{
    internal class RecorderInstaller : Installer<RecorderInstaller>
    {
        private static readonly MethodBase ScoreSaber_playbackEnabled = AccessTools.Method("ScoreSaber.Core.ReplaySystem.HarmonyPatches.PatchHandleHMDUnmounted:Prefix");

        public override void InstallBindings()
        {
            if (ScoreSaber_playbackEnabled != null && (bool)ScoreSaber_playbackEnabled.Invoke(null, null) == false)
            {
                Plugin.Log.Notice("SS replay is running, BL Replay Recorder can not be started!");
                return;
            }
            if (!(MapEnhancer.previewBeatmapLevel.levelID.StartsWith("custom_level_")))
            {
                Plugin.Log.Notice("OST level detected. Skipping recording!");
                return;
            }
            Plugin.Log.Debug("Starting a BL Replay Recorder");
            Container.Bind<ReplayRecorder>().AsSingle();
            Container.Bind<TrackingDeviceEnhancer>().AsTransient();
        }
    }
}
