using System;
using BeatLeader.Models.AbstractReplay;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class QuickReplayOverview : ReeUIComponentV3<QuickReplayOverview> {
        #region UI Components

        [UIComponent("finish-status-image"), UsedImplicitly]
        private ImageView _finishStatusImage = null!;

        [UIComponent("speed-value-image"), UsedImplicitly]
        private ImageView _speedValueImage = null!;

        #endregion

        #region SetReplay

        public void SetReplay(IReplay replay) {
            var data = replay.ReplayData;
            RefreshFinishStatusImage(data.FinishType);
            RefreshSpeedValueImage(data.GameplayModifiers.songSpeedMul);
        }

        #endregion

        #region Finish Status

        private void RefreshFinishStatusImage(ReplayFinishType finishType) {
            switch (finishType) {
                case ReplayFinishType.Failed:
                    _finishStatusImage.color = Color.red;
                    _finishStatusImage.sprite = BundleLoader.SkullIcon;
                    break;
                case ReplayFinishType.Cleared:
                    _finishStatusImage.color = Color.green;
                    _finishStatusImage.sprite = BundleLoader.CheckIcon;
                    break;
                case ReplayFinishType.Incomplete:
                    _finishStatusImage.color = Color.yellow;
                    _finishStatusImage.sprite = BundleLoader.QuestionIcon;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(finishType), finishType, null);
            }
        }

        #endregion

        #region Speed Value

        private void RefreshSpeedValueImage(float speedMultiplier) {
            switch (speedMultiplier) {
                case < 1:
                    _speedValueImage.color = Color.yellow;
                    _speedValueImage.sprite = BundleLoader.SlowSpeedIcon;
                    break;
                case > 1:
                    _speedValueImage.color = Color.cyan;
                    _speedValueImage.sprite = BundleLoader.FastSpeedIcon;
                    break;
                case 1:
                    _speedValueImage.sprite = BundleLoader.MediumSpeedIcon;
                    _speedValueImage.color = Color.white;
                    break;
            }
        }

        #endregion
    }
}