using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Components;
using Zenject;
using BeatLeader.Utils;
using UnityEngine.UI;
using System.Collections;
using BeatLeader.Models;
using BeatLeader.Replayer;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Views.Replayer2DView.bsml")]
    internal class Replayer2DViewController : StandaloneViewController<CanvasScreen> {
        #region Injection

        [Inject] private readonly IReplayPauseController _pauseController = null!;
        [Inject] private readonly IReplayFinishController _finishController = null!;
        [Inject] private readonly IReplayTimeController _timeController = null!;
        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly ReplayerCameraController _cameraController = null!;
        [Inject] private readonly IReplayWatermark _watermark = null!;

        #endregion

        #region UI Components

        [UIComponent("replayer-panel"), UsedImplicitly]
        private ReplayerUIPanel _replayerUIPanel = null!;

        #endregion

        #region Setup

        public override bool IsVisible {
            get => Screen.CanvasGroup.alpha == 1;
            set {
                Screen.CanvasGroup.alpha = value ? 1 : 0;
            }
        }
        
        protected override void OnInitialize() {
            base.OnInitialize();
            Screen.Apply2DTemplate();
            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            _replayerUIPanel.Setup(
                _pauseController, _finishController,
                _timeController, _playersManager,
                _cameraController.ViewableCamera!,
                _launchData, _watermark
            );
            StartCoroutine(UIAnimationCoroutine());
        }

        protected override void OnPreParse() {
            Screen.CanvasGroup.alpha = 0;
            _finishController.ReplayWasLeftEvent += HandleReplayFinish;
        }

        protected override void OnDispose() {
            _finishController.ReplayWasLeftEvent -= HandleReplayFinish;
        }

        #endregion

        #region Callbacks

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