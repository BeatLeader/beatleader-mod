using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Replayer;
using UnityEngine;
using HMUI;
using IPA.Utilities;

namespace BeatLeader.ViewControllers
{
    internal class ReplayButton : ReeUIComponentV2
    {
        #region UI Event Handlers

        [UIAction("replay-button-clicked")]
        private void HandleReplayButtonClicked()
        {
            ReplayerMenuLoader.NotifyPlayLastButtonWasPressed();
        }

        #endregion
    }
}
