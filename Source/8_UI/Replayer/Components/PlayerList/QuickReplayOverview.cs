using System;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class QuickReplayOverview : ReactiveComponent {
        #region Construct
        
        private Image _finishStatusImage = null!;
        private Image _speedValueImage = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new Image()
                        .WithRectExpand()
                        .Bind(ref _finishStatusImage)
                        .InBlurBackground(color: Color.black.ColorWithAlpha(0.93f))
                        .AsFlexItem(size: 5f),
                    //
                    new Image()
                        .WithRectExpand()
                        .Bind(ref _speedValueImage)
                        .InBlurBackground(color: Color.black.ColorWithAlpha(0.93f))
                        .AsFlexItem(size: 5f),
                }
            }.AsFlexGroup(gap: 1f).Use();
        }

        protected override void OnInitialize() {
            RefreshFinishStatusImage(ReplayFinishType.Incomplete);
            RefreshSpeedValueImage(1f);
        }

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
                    _finishStatusImage.Color = Color.red;
                    _finishStatusImage.Sprite = BundleLoader.SkullIcon;
                    break;
                case ReplayFinishType.Cleared:
                    _finishStatusImage.Color = Color.green;
                    _finishStatusImage.Sprite = BundleLoader.CheckIcon;
                    break;
                case ReplayFinishType.Incomplete:
                    _finishStatusImage.Color = Color.yellow;
                    _finishStatusImage.Sprite = BundleLoader.QuestionIcon;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(finishType), finishType, null);
            }
        }

        #endregion

        #region Speed Value

        private void RefreshSpeedValueImage(float speedMultiplier) {
            switch (speedMultiplier) {
                case < 1:
                    _speedValueImage.Color = Color.yellow;
                    _speedValueImage.Sprite = BundleLoader.SlowSpeedIcon;
                    break;
                case > 1:
                    _speedValueImage.Color = Color.cyan;
                    _speedValueImage.Sprite = BundleLoader.FastSpeedIcon;
                    break;
                case 1:
                    _speedValueImage.Sprite = BundleLoader.MediumSpeedIcon;
                    _speedValueImage.Color = Color.white;
                    break;
            }
        }

        #endregion
    }
}