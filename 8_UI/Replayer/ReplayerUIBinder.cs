using BeatLeader.ViewControllers;
using UnityEngine;
using Zenject;
using BeatLeader.Components;
using BeatLeader.Utils;
using System;
using BeatLeader.Models;
using BeatLeader.Replayer;

namespace BeatLeader.UI {
    internal class ReplayerUIBinder : MonoBehaviour {
        #region Injection

        [Inject] private readonly MenuControllersManager _menuControllersManager;
        [Inject] private readonly Replayer2DViewController _screenViewController;
        [Inject] private readonly ReplayerVRViewController _vrViewController;
        [Inject] private readonly ReplayLaunchData _launchData;
        [Inject] private readonly IReplayPauseController _pauseController;

        #endregion

        #region UI Visibility

        public bool AutoHideUI {
            get => _autoHideUI;
            set {
                _autoHideUI = value;
                RefreshUIVisibility();
            }
        }

        public bool UIEnabled { get; private set; }

        private bool _autoHideUI;

        private void RefreshUIVisibility() {
            if (ViewController == null || !_autoHideUI) return;
            UIEnabled = _pauseController.IsPaused;
            ViewController.IsVisible = UIEnabled;
            UIVisibilityChangedEvent?.Invoke(UIEnabled);
        }

        #endregion

        #region Events

        public event Action<bool>? UIVisibilityChangedEvent;

        #endregion

        #region Setup

        public IStandaloneViewController ViewController { get; private set; } = null!;

        private void Start() {
            _pauseController.PauseStateChangedEvent += HandlePauseStateChanged;
            ViewController = InputUtils.IsInFPFC ? _screenViewController : _vrViewController;
            ViewController.Init();
            if (!InputUtils.IsInFPFC) {
                ViewController.Container.transform.SetParent(_menuControllersManager.HandsContainer, false);
            }
            AutoHideUI = _launchData.Settings.AutoHideUI;
        }

        private void OnDestroy() {
            _pauseController.PauseStateChangedEvent -= HandlePauseStateChanged;
        }

        #endregion

        #region Callbacks

        private void HandlePauseStateChanged(bool state) {
            RefreshUIVisibility();
        }

        #endregion
    }
}
