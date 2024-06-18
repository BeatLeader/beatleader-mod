using BeatLeader.Models;
using BeatLeader.UI.Replayer;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;

namespace BeatLeader.Components {
    internal class OtherContentView : ContentView {
        #region Toggles

        [UIValue("show-watermark")]
        private bool ShowWatermark {
            get => _replayWatermark?.Enabled ?? false;
            set {
                if (_replayWatermark != null) {
                    _replayWatermark.Enabled = value;
                }
            }
        }

        [UIValue("exit-replay-automatically")]
        private bool ExitAutomatically {
            get => _launchData?.Settings.ExitReplayAutomatically ?? false;
            set => _launchData.Settings.ExitReplayAutomatically = value;
        }

        #endregion

        #region UI Components

        [UIValue("body-menu-button")]
        private NavigationButton _bodyMenuButton = null!;

        [UIValue("timeline-menu-button")]
        private NavigationButton _timelineMenuButton = null!;

        private BodyContentView _bodyContentView = null!;
        private TimelineContentView _timelineContentView = null!;

        #endregion

        #region Setup

        [UIObject("watermark-toggle")]
        private readonly GameObject _watermarkToggleObject = null!;

        private IReplayWatermark? _replayWatermark;
        private ReplayLaunchData _launchData = null!;

        public void Setup(
            IVirtualPlayersManager playersManager,
            ReplayLaunchData launchData,
            IReplayWatermark? watermark = null,
            IReplayTimeline? timeline = null) {
            _launchData = launchData;
            _replayWatermark = watermark;
            _bodyContentView.Setup(playersManager, launchData);
            _timelineContentView.Setup(timeline!, launchData);
            _watermarkToggleObject.SetActive(_replayWatermark?.CanBeDisabled ?? false);
            NotifyPropertyChanged(nameof(ShowWatermark));
            NotifyPropertyChanged(nameof(ExitAutomatically));
        }

        protected override void OnInstantiate() {
            _bodyContentView = InstantiateOnSceneRoot<BodyContentView>();
            _timelineContentView = InstantiateOnSceneRoot<TimelineContentView>();
            _bodyContentView.ManualInit(null!);
            _timelineContentView.ManualInit(null!);

            _bodyMenuButton = Instantiate<NavigationButton>(transform);
            _timelineMenuButton = Instantiate<NavigationButton>(transform);
            _bodyMenuButton.Setup(this, _bodyContentView, "Body");
            _timelineMenuButton.Setup(this, _timelineContentView, "Timeline");
        }

        #endregion
    }
}
