using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Utils.Expansions;
using UnityEngine;
using IPA.Utilities;
using BeatLeader.Replays;
using BeatLeader.UI;
using Zenject;

namespace BeatLeader.Installers
{
    internal class ReplaySceneSetupDataInstaller : Installer<ReplaySceneSetupDataInstaller>
    {
        public override void InstallBindings()
        {
            if (!ReplayUI.isStartedAsReplay) return;
            var replayData = ReplayUI.replayData;
            var sceneSetupData = Container.Resolve<GameplayCoreSceneSetupData>();
            sceneSetupData.SetField("gameplayModifiers", replayData.GetModifiersFromReplay());
            sceneSetupData.playerSpecificSettings.SetField("_playerHeight", replayData.info.height);
            sceneSetupData.playerSpecificSettings.SetField("_automaticPlayerHeight", false);

            Container.Rebind<GameplayCoreSceneSetupData>().FromInstance(sceneSetupData).AsCached();
            Resources.FindObjectsOfTypeAll<GameplayCoreInstaller>().FirstOrDefault()
                .InjectAllFieldsOfType<GameplayCoreSceneSetupData, GameplayCoreInstaller>(Container);
        }
    }
}
