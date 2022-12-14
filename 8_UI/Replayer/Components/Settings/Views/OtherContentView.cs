using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;

namespace BeatLeader.Components {
    internal class OtherContentView : ContentView {
        #region UI Values

        [UIValue("show-head")]
        private bool ShowHead {
            get => _launchData?.ActualSettings.ShowHead ?? false;
            set {
                _launchData.ActualToWriteSettings.ShowHead = value;
                _controllersManager.Head.gameObject.SetActive(value);
            }
        }

        [UIValue("show-left-saber")]
        private bool ShowLeftSaber {
            get => _launchData?.ActualSettings.ShowLeftSaber ?? false;
            set {
                _launchData.ActualToWriteSettings.ShowLeftSaber = value;
                _controllersManager.LeftSaber.gameObject.SetActive(value);
            }
        }

        [UIValue("show-right-saber")]
        private bool ShowRightSaber {
            get => _launchData?.ActualSettings.ShowRightSaber ?? false;
            set {
                _launchData.ActualToWriteSettings.ShowRightSaber = value;
                _controllersManager.RightSaber.gameObject.SetActive(value);
            }
        }

        [UIValue("show-watermark")]
        private bool ShowWatermark {
            get => _replayWatermark?.Enabled ?? false;
            set {
                if (_replayWatermark != null)
                    _replayWatermark.Enabled = value;
            }
        }

        #endregion

        #region Setup

        [UIObject("watermark")] private GameObject _watermark;

        private ReplayerControllersManager _controllersManager;
        private ReplayLaunchData _launchData;
        private ReplayWatermark _replayWatermark;

        public void Setup(
            ReplayerControllersManager controllersManager,
            ReplayLaunchData launchData, 
            ReplayWatermark watermark = null) {
            _controllersManager = controllersManager;
            _launchData = launchData;
            _replayWatermark = watermark;

            NotifyPropertyChanged(nameof(ShowHead));
            NotifyPropertyChanged(nameof(ShowLeftSaber));
            NotifyPropertyChanged(nameof(ShowRightSaber));
            NotifyPropertyChanged(nameof(ShowWatermark));
            _watermark.SetActive(_replayWatermark.CanBeDisabled);
        }

        #endregion
    }
}
