using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Utils;
using BeatLeader.Utils.Expansions;
using UnityEngine;
using IPA.Utilities;
using BeatLeader.Replays.Emulators;
using BeatLeader.Replays;
using BeatLeader.UI;
using Zenject;

namespace BeatLeader.Replays.Installers
{
    internal class PlayerScenePatchInstaller : Installer<PlayerScenePatchInstaller>
    {
        public override void InstallBindings()
        {
            if (!ReplayMenuUI.isStartedAsReplay) return;
            var replayData = ReplayMenuUI.replayData;

            var sceneSetupData = Container.Resolve<GameplayCoreSceneSetupData>();
            sceneSetupData.SetField("gameplayModifiers", replayData.GetModifiersFromReplay());
            sceneSetupData.playerSpecificSettings.SetField("_playerHeight", replayData.info.height);
            sceneSetupData.playerSpecificSettings.SetField("_automaticPlayerHeight", false);

            Container.Rebind<GameplayCoreSceneSetupData>().FromInstance(sceneSetupData).AsCached();
            Resources.FindObjectsOfTypeAll<GameplayCoreInstaller>().FirstOrDefault().InjectAllFieldsOfType<GameplayCoreSceneSetupData, GameplayCoreInstaller>(Container);
        }
    }
}
