using UnityEngine;
using Zenject;
using BeatLeader.Models;
using BeatLeader.Replayer;

namespace BeatLeader.UI.Replayer {
    internal abstract class ReplayerUIBinder : MonoBehaviour {
        #region Injection

        [Inject] private readonly ReplayerExtraObjectsProvider _extraObjects = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly IReplayPauseController _pauseController = null!;

        #endregion

        #region UI Visibility

        private void RefreshUIVisibility() {
            if (!_launchData.Settings.AutoHideUI) return;
            SetUIEnabled(_pauseController.IsPaused);
        }

        protected abstract void SetUIEnabled(bool uiEnabled);

        #endregion

        #region Setup

        private void Awake() {
            _pauseController.PauseStateChangedEvent += HandlePauseStateChanged;
            transform.SetParent(_extraObjects.ReplayerCore, false);
            SetupUI();
        }

        private void OnDestroy() {
            _pauseController.PauseStateChangedEvent -= HandlePauseStateChanged;
        }

        protected abstract void SetupUI();

        #endregion

        #region Callbacks

        private void HandlePauseStateChanged(bool state) {
            RefreshUIVisibility();
        }

        #endregion
    }
}