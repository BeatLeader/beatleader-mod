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
        [UIValue("voting-button"), UsedImplicitly]
        private VotingButton _votingButton = ReeUIComponentV2.InstantiateOnSceneRoot<VotingButton>(false);

        [UIValue("voting-panel"), UsedImplicitly]
        private VotingPanel _votingPanel = ReeUIComponentV2.InstantiateOnSceneRoot<VotingPanel>(false);

        [UIValue("image-skew")]
        private readonly float _imageSkew = 0.18f;

        #region Callbacks

        [UIAction("replay-button-clicked"), UsedImplicitly]
        private void HandleReplayButtonClicked()
        {
            ReplayerMenuLoader.NotifyPlayLastButtonWasPressed();
        }

        #endregion

        private void Awake()
        {
            _votingButton.SetParent(transform);
            _votingPanel.SetParent(transform);
        }
    }
}
