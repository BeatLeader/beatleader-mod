﻿using BeatLeader.Models;
using BeatLeader.Replayer.Emulation;
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
                _controllersProvider.Head.gameObject.SetActive(value);
            }
        }

        [UIValue("show-left-saber")]
        private bool ShowLeftSaber {
            get => _launchData?.ActualSettings.ShowLeftSaber ?? false;
            set {
                _launchData.ActualToWriteSettings.ShowLeftSaber = value;
                _controllersProvider.LeftSaber.gameObject.SetActive(value);
            }
        }

        [UIValue("show-right-saber")]
        private bool ShowRightSaber {
            get => _launchData?.ActualSettings.ShowRightSaber ?? false;
            set {
                _launchData.ActualToWriteSettings.ShowRightSaber = value;
                _controllersProvider.RightSaber.gameObject.SetActive(value);
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

        private VRControllersAccessor _controllersProvider;
        private ReplayLaunchData _launchData;
        private ReplayWatermark _replayWatermark;

        public void Setup(
            VRControllersAccessor controllersProvider,
            ReplayLaunchData launchData, 
            ReplayWatermark watermark = null) {
            _controllersProvider = controllersProvider;
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