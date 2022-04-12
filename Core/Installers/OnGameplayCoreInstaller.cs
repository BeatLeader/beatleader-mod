using System.Reflection;
using BeatLeader.UI;
using BeatLeader.UI.ReplayUI;
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
        public override void InstallBindings()
        {
            Plugin.Log.Debug("OnGameplayCoreInstaller");
        }
    }
}