using BeatLeader.Models;
using BeatLeader.Replayer.Emulation;
using BeatSaberMarkupLanguage.Attributes;

namespace BeatLeader.Components {
    internal class BodyContentView : ContentView {
        #region Visibility

        [UIValue("show-left-saber")]
        private bool ShowLeftSaber {
            get => _launchData?.Settings.ShowLeftSaber ?? false;
            set {
                _launchData.Settings.ShowLeftSaber = value;
                RefreshVisibility();
            }
        }

        [UIValue("show-right-saber")]
        private bool ShowRightSaber {
            get => _launchData?.Settings.ShowRightSaber ?? false;
            set {
                _launchData.Settings.ShowRightSaber = value;
                RefreshVisibility();
            }
        }

        [UIValue("show-head")]
        private bool ShowHead {
            get => _launchData?.Settings.ShowHead ?? false;
            set {
                _launchData.Settings.ShowHead = value;
                RefreshVisibility();
            }
        }

        private IVRControllersProvider _controllersProvider = null!;

        private void RefreshVisibility() {
            _controllersProvider.LeftSaber.gameObject.SetActive(ShowLeftSaber);
            _controllersProvider.RightSaber.gameObject.SetActive(ShowRightSaber);
            _controllersProvider.Head.gameObject.SetActive(ShowHead);
        }

        #endregion

        #region Setup

        private IVirtualPlayersManager _playersManager = null!;
        private ReplayLaunchData _launchData = null!;

        public void Setup(IVirtualPlayersManager playersManager, ReplayLaunchData launchData) {
            OnDispose();
            _launchData = launchData;
            _playersManager = playersManager;
            _playersManager.PriorityPlayerWasChangedEvent += HandlePriorityPlayerChanged;
            HandlePriorityPlayerChanged(playersManager.PriorityPlayer);
            RefreshToggles();
        }


        protected override void OnDispose() {
            if (_playersManager != null)
                _playersManager.PriorityPlayerWasChangedEvent -= HandlePriorityPlayerChanged;
        }

        private void RefreshToggles() {
            NotifyPropertyChanged(nameof(ShowHead));
            NotifyPropertyChanged(nameof(ShowLeftSaber));
            NotifyPropertyChanged(nameof(ShowRightSaber));
        }

        #endregion

        #region Callbacks

        private void HandlePriorityPlayerChanged(VirtualPlayer player) {
            _controllersProvider = player.ControllersProvider!;
            RefreshVisibility();
        }

        #endregion
    }
}
