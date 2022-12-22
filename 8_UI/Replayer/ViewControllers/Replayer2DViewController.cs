using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Components;
using Zenject;
using UnityEngine;
using BeatLeader.Utils;
using UnityEngine.UI;
using System.Collections;
using BeatLeader.Models;
using BeatLeader.Replayer.Camera;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Views.Replayer2DView.bsml")]
    internal class Replayer2DViewController : StandaloneViewController<CanvasScreen> {
        #region Injection

        [Inject] private readonly IReplayPauseController _pauseController = null!;
        [Inject] private readonly IReplayFinishController _finishController = null!;
        [Inject] private readonly IBeatmapTimeController _beatmapTimeController = null!;
        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly ReplayerCameraController _cameraController = null!;
        [Inject] private readonly IReplayWatermark _watermark = null!;

        #endregion

        #region UI Components

        [UIValue("main-view")]
        private MainScreenView _mainScreenView = null!;

        #endregion

        #region Setup

        public override bool IsVisible {
            get => Screen.CanvasGroup.alpha == 1;
            set {
                Screen.CanvasGroup.alpha = value && _isUIBuilt ? 1 : 0;
                _enableAfterBuild = value;
            }
        }

        private bool _enableAfterBuild = true;
        private bool _isUIBuilt;

        public void OpenLayoutEditor() {
            _mainScreenView?.OpenLayoutEditor();
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            Screen.Apply2DTemplate();
            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }

        protected override void OnPreParse() {
            Screen.CanvasGroup.alpha = 0;

            _mainScreenView = ReeUIComponentV2.Instantiate<MainScreenView>(transform);
            _mainScreenView.Setup(_pauseController, _finishController,
                _beatmapTimeController, _playersManager, _launchData, _cameraController, _watermark);

            _finishController.ReplayWasLeftEvent += HandleReplayFinish;
            _mainScreenView.LayoutBuiltEvent += HandleUIBuilt;
        }

        protected override void OnDispose() {
            _finishController.ReplayWasLeftEvent -= HandleReplayFinish;
        }

        #endregion

        #region Callbacks

        private void HandleUIBuilt() {
            if (_isUIBuilt) return;
            _isUIBuilt = true;
            if (!_enableAfterBuild) return;
            StartCoroutine(UIAnimationCoroutine());
        }

        private void HandleReplayFinish() {
            if (!IsVisible) return;
            StartCoroutine(UIAnimationCoroutine(false));
        }

        #endregion

        #region Animation

        private const float InDuration = 0.5f;
        private const float OutDuration = 0.3f;

        private IEnumerator UIAnimationCoroutine(bool show = true) {
            var start = show ? 0 : 1;
            var end = !show ? 0 : 1;
            return BasicCoroutines.AnimateGroupCoroutine(
                Screen.CanvasGroup, start, end, show 
                ? InDuration : OutDuration);
        }

        #endregion
    }
}
