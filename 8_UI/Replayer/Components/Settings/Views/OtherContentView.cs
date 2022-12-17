using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.Replayer.Emulation;
using BeatSaberMarkupLanguage.Attributes;
using IPA.Config.Data;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace BeatLeader.Components {
    internal class OtherContentView : ContentView {
        #region Visibility

        [UIValue("show-left-saber")]
        private bool ShowLeftSaber {
            get => _launchData?.Settings.ShowLeftSaber ?? false;
            set {
                _showLeftSaber = value;
                _launchData.Settings.ShowLeftSaber = value;
                RefreshVisibility();
            }
        }

        [UIValue("show-right-saber")]
        private bool ShowRightSaber {
            get => _launchData?.Settings.ShowRightSaber ?? false;
            set {
                _showRightSaber = value;
                _launchData.Settings.ShowRightSaber = value;
                RefreshVisibility();
            }
        }

        [UIValue("show-head")]
        private bool ShowHead {
            get => _launchData?.Settings.ShowHead ?? false;
            set {
                _showHead = value;
                _launchData.Settings.ShowHead = value;
                RefreshVisibility();
            }
        }

        [UIValue("show-watermark")]
        private bool ShowWatermark {
            get => _replayWatermark?.Enabled ?? false;
            set {
                _showWatermark = value;
                RefreshVisibility();
            }
        }

        private IVRControllersProvider _controllersProvider;

        private bool _showLeftSaber;
        private bool _showRightSaber;
        private bool _showHead;
        private bool _showWatermark;

        private void RefreshVisibility() {
            _controllersProvider.LeftSaber.gameObject.SetActive(_showLeftSaber);
            _controllersProvider.RightSaber.gameObject.SetActive(_showRightSaber);
            _controllersProvider.Head.gameObject.SetActive(_showHead);
            if (_replayWatermark != null) {
                _replayWatermark.Enabled = _showWatermark;
            }
        }

        #endregion

        #region Setup

        [UIObject("watermark-toggle")]
        private readonly GameObject _watermarkToggleObject;

        private ReplayLaunchData _launchData;
        private IVirtualPlayersManager _playersManager;
        private IReplayWatermark _replayWatermark;

        public void Init(ReplayLaunchData launchData, IVirtualPlayersManager playersManager, IReplayWatermark watermark = null) {
            if (_playersManager != null)
                _playersManager.PriorityPlayerWasChangedEvent -= HandlePriorityPlayerChanged;

            _playersManager = playersManager;
            _launchData = launchData;
            _replayWatermark = watermark;

            _playersManager.PriorityPlayerWasChangedEvent += HandlePriorityPlayerChanged;
            _watermarkToggleObject.SetActive(_replayWatermark?.CanBeDisabled ?? false);

            HandlePriorityPlayerChanged(_playersManager.PriorityPlayer);
            RefreshToggles();
        }

        protected override void OnDispose() {
            _playersManager.PriorityPlayerWasChangedEvent -= HandlePriorityPlayerChanged;
        }

        private void RefreshToggles() {
            NotifyPropertyChanged(nameof(ShowHead));
            NotifyPropertyChanged(nameof(ShowLeftSaber));
            NotifyPropertyChanged(nameof(ShowRightSaber));
            NotifyPropertyChanged(nameof(ShowWatermark));
        }

        #endregion

        #region Callbacks

        private void HandlePriorityPlayerChanged(VirtualPlayer player) {
            _controllersProvider = player.ControllersProvider;
            RefreshVisibility();
        }

        #endregion
    }
}
