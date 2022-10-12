using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Components;
using Zenject;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage.Components;
using BeatLeader.Models;
using JetBrains.Annotations;
using System;
using BeatLeader.Replayer;

namespace BeatLeader.ViewControllers
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Results.ResultsScreenUI.bsml")]
    internal class ResultsScreenUI : ReeUIComponentV2
    {
        #region Callbacks

        [UIAction("replay-on-click"), UsedImplicitly]
        private void ReplayOnClick()
        {
            ReplayerMenuLoader.NotifyPlayLastButtonWasPressed();
        }

        #endregion
    }
}
