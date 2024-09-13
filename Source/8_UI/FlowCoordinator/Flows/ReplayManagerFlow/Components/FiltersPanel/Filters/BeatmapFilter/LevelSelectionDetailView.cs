using BeatLeader.UI.Reactive.Components;
using IPA.Utilities;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class LevelSelectionDetailView : ReactiveComponent {
        #region Setup

        private StandardLevelDetailView? _detailView;
        private LevelBar? _levelBar;
        private bool _isInitialized;

        public void Refresh() {
            if (!_isInitialized || _detailView!._beatmapLevel == null) return;
            _levelBar!.Setup(_detailView.beatmapKey);
            _detailView.RefreshContent();
        }

        public void Setup(StandardLevelDetailView levelDetailView) {
            if (_isInitialized) return;
            _detailView = levelDetailView;
            _levelBar = levelDetailView._levelBar;
            _levelBar = Object.Instantiate(_levelBar, _levelBarContainer.ContentTransform, false);
            _isInitialized = true;
        }

        #endregion

        #region Construct

        private Dummy _levelBarContainer = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new Dummy()
                        .AsFlexItem(size: new() { x = "100%", y = 14f })
                        .Bind(ref _levelBarContainer),
                    //
                    new BsPrimaryButton {
                            Skew = UIStyle.Skew,
                            OnClick = () => _detailView?.actionButton.onClick.Invoke()
                        }
                        .WithLabel("SELECT")
                        .AsFlexItem(size: new() { x = 24f, y = 8f })
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.Center,
                alignItems: Align.Center,
                gap: 2f
            ).Use();
        }

        #endregion
    }
}