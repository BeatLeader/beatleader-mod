using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;

namespace BeatLeader.Components {
    internal class TimelineContentView : ContentView {
        #region Visibility

        private const string TimelineMissName = "MissMark";
        private const string TimelineBombName = "BombMark";
        private const string TimelinePauseName = "PauseMark";

        [UIValue("show-misses")]
        private bool ShowMisses {
            get => _launchData?.Settings.ShowTimelineMisses ?? false;
            set {
                _launchData.Settings.ShowTimelineMisses = value;
                RefreshVisibility();
            }
        }

        [UIValue("show-bombs")]
        private bool ShowBombs {
            get => _launchData?.Settings.ShowTimelineBombs ?? false;
            set {
                _launchData.Settings.ShowTimelineBombs = value;
                RefreshVisibility();
            }
        }

        [UIValue("show-pauses")]
        private bool ShowPauses {
            get => _launchData?.Settings.ShowTimelinePauses ?? false;
            set {
                _launchData.Settings.ShowTimelinePauses = value;
                RefreshVisibility();
            }
        }

        private void RefreshVisibility() {
            _timeline.ShowMarkers(TimelineMissName, ShowMisses);
            _timeline.ShowMarkers(TimelineBombName, ShowBombs);
            _timeline.ShowMarkers(TimelinePauseName, ShowPauses);
        }

        #endregion

        private IReplayTimeline _timeline = null!;
        private ReplayLaunchData _launchData = null!;

        public void Setup(IReplayTimeline timeline, ReplayLaunchData launchData) {
            OnDispose();
            _timeline = timeline;
            _launchData = launchData;
            _timeline.MarkersWasGeneratedEvent += HandleMarkersWasGenerated;
            RefreshToggles();
        }

        protected override void OnDispose() {
            if (_timeline != null)
                _timeline.MarkersWasGeneratedEvent -= HandleMarkersWasGenerated;
        }

        private void RefreshToggles() {
            NotifyPropertyChanged(nameof(ShowMisses));
            NotifyPropertyChanged(nameof(ShowBombs));
            NotifyPropertyChanged(nameof(ShowPauses));
        }

        private void HandleMarkersWasGenerated() {

        }
    }
}
