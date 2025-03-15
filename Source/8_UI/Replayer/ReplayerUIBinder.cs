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

        [Inject] private readonly ReplayerExtraObjectsProvider _extraObjects = null!;
        [Inject] private readonly Replayer2DViewController _screenViewController = null!;
        [Inject] private readonly ReplayerVRViewController _vrViewController = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly IReplayPauseController _pauseController = null!;

        #endregion

        #region UI Visibility

        public bool AutoHideUI {
            get => _autoHideUI;
            set {
                _autoHideUI = value;
                RefreshUIVisibility();
            }
        }

        private bool _autoHideUI;

        private void RefreshUIVisibility() {
            if (InputUtils.IsInFPFC || ViewController == null || !_autoHideUI) return;
            var enabled = _pauseController.IsPaused;
            ViewController.IsVisible = enabled;
            UIVisibilityChangedEvent?.Invoke(enabled);
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
                ViewController.Container.transform
                    .SetParent(_extraObjects.ReplayerCore, false);
            }
            AutoHideUI = _launchData.Settings.AutoHideUI;
        }

        private void OnDisable() {
            ViewController.OnClose();
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
