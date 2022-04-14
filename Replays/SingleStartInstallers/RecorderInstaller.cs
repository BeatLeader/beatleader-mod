using System.Linq;
using System.Reflection;
using BeatLeader.Utils;
using BeatLeader.Replays.Models;
using BeatLeader.Replays.ReplayEnhancers;
using HarmonyLib;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Installers
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
            Container.Bind<TrackingDeviceEnhancer>().AsTransient();
            Container.Bind<ReplayDataProvider>().AsSingle().NonLazy();
            var go = new GameObject("ReplayRecorder");
            go.AddComponent<ReplayRecorder>();
            Container.Bind<ReplayRecorder>().FromComponentOn(go).AsSingle().NonLazy();
            
            Zenjector.beforeGameplayCoreInstalled -= Install;
        }
    }
}
