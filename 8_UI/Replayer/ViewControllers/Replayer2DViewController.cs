using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Components;
using Zenject;
using UnityEngine;
using BeatLeader.Utils;
using static UnityEngine.UI.CanvasScaler;
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

        private new CanvasGroup _canvasGroup = null!;

        #endregion

        #region Setup

        public override bool IsVisible {
            get => _canvasGroup.alpha == 1;
            set {
                _canvasGroup.alpha = !value && _isUIBuilt ? 1 : 0;
                _enableAfterBuild = !value;
            }
        }

        private bool _enableAfterBuild = true;
        private bool _isUIBuilt;

        public void OpenLayoutEditor() {
            _mainScreenView?.OpenLayoutEditor();
        }

        protected override void OnInit() {
            base.OnInit();
            var canvas = Screen.Canvas;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1;
            canvas.additionalShaderChannels =
                AdditionalCanvasShaderChannels.TexCoord1
                | AdditionalCanvasShaderChannels.TexCoord2;

            var scaler = Screen.CanvasScaler;
            scaler.referenceResolution = new(350, 300);
            scaler.uiScaleMode = ScaleMode.ScaleWithScreenSize;
            scaler.dynamicPixelsPerUnit = 3.44f;
            scaler.referencePixelsPerUnit = 10f;

            gameObject.GetOrAddComponent<GraphicRaycaster>();
            _canvasGroup = Screen.gameObject.GetOrAddComponent<CanvasGroup>();
        }

        protected override void OnPreParse() {
            _canvasGroup.alpha = 0;
            _finishController.ReplayWasExitedEvent += HandleReplayFinish;
            _mainScreenView.LayoutBuiltEvent += HandleUIBuilt;

            _mainScreenView = ReeUIComponentV2.Instantiate<MainScreenView>(transform);
            _mainScreenView.Setup(_pauseController, _finishController, 
                _beatmapTimeController, _playersManager, _launchData, _cameraController, _watermark);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _finishController.ReplayWasExitedEvent -= HandleReplayFinish;
        }

        #endregion

        #region Callbacks

        private void HandleUIBuilt() {
            if (_isUIBuilt) return;
            _isUIBuilt = true;
            if (_enableAfterBuild) {
                CoroutinesHandler.instance.StartCoroutine(UIAnimationCoroutine());
            }
        }

        private void HandleReplayFinish() {
            CoroutinesHandler.instance.StartCoroutine(UIAnimationCoroutine(false));
        }

        #endregion

        #region Animation

        private const float InDuration = 0.5f;
        private const float OutDuration = 0.3f;
        private const float AnimationFrameRate = 60f;

        private IEnumerator UIAnimationCoroutine(bool show = true) {
            yield return new WaitForEndOfFrame();
            IsVisible = !show;
            var duration = show ? InDuration : OutDuration;
            var totalFramesCount = Mathf.FloorToInt(duration * AnimationFrameRate);
            var frameDuration = duration / totalFramesCount;
            var alphaStep = 1f / (show ? totalFramesCount : -totalFramesCount);

            for (int frame = 0; frame < totalFramesCount; frame++) {
                _canvasGroup.alpha += alphaStep;
                yield return new WaitForSeconds(frameDuration);
            }
        }

        #endregion
    }
}
