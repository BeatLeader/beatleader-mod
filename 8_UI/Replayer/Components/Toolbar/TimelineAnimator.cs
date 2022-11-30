using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    internal class TimelineAnimator : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerUpHandler,
        IPointerDownHandler {
        #region Configuration

        private const float HandleExpandSize = 0.8f;
        private const float BackgroundExpandSize = 1f;
        private const float AnimationFrameRate = 60f;
        private const float ExpandTransitionDuration = 0.15f;
        private const float ShrinkTransitionDuration = 0.1f;

        #endregion

        #region Setup

        private RectTransform _background;
        private RectTransform _handle;
        private RectTransform _marksAreaContainer;
        private RectTransform _fillArea;
        private bool _initialized;

        public void Setup(
            RectTransform background,
            RectTransform handle,
            RectTransform marksAreaContainer,
            RectTransform fillArea) {
            _background = background;
            _handle = handle;
            _marksAreaContainer = marksAreaContainer;
            _fillArea = fillArea;
            _initialized = true;
        }

        #endregion

        #region Events

        public event Action HandleReleasedEvent;
        public event Action HandlePressedEvent;

        #endregion

        #region Triggers

        private bool _highlightedStateTrigger;

        private void Update() {
            if (!_initialized) return;
            StartAnimation(_highlightedStateTrigger);
        }

        public void OnPointerEnter(PointerEventData data) {
            _highlightedStateTrigger = true;
        }
        public void OnPointerExit(PointerEventData data) {
            _highlightedStateTrigger = false;
        }
        public void OnPointerUp(PointerEventData data) {
            HandleReleasedEvent?.Invoke();
        }
        public void OnPointerDown(PointerEventData data) {
            HandlePressedEvent?.Invoke();
        }

        #endregion

        #region Animation

        private bool _highlighted;
        private bool _isAnimating;

        private bool StartAnimation(bool highlighted = true) {
            if (_isAnimating || highlighted == _highlighted) return false;
            _isAnimating = true;
            var duration = highlighted ? ExpandTransitionDuration : ShrinkTransitionDuration;
            StartCoroutine(AnimationCoroutine(duration, highlighted));
            return true;
        }

        private IEnumerator AnimationCoroutine(float duration, bool highlight) {
            var totalFramesCount = Mathf.FloorToInt(duration * AnimationFrameRate);
            var frameDuration = duration / totalFramesCount;
            var sizeStepBG = BackgroundExpandSize / totalFramesCount;
            var sizeStepHandle = HandleExpandSize / totalFramesCount;
            var nextSizeBG = _background.sizeDelta;
            var nextSizeFillArea = _fillArea.sizeDelta;
            var nextPosMarksArea = _marksAreaContainer.localPosition;

            sizeStepBG = highlight ? sizeStepBG : -sizeStepBG;
            sizeStepHandle = highlight ? sizeStepHandle : -sizeStepHandle;

            var vecStepHandle = new Vector3(sizeStepHandle, sizeStepHandle);
            var sizeStepBGDiv2 = sizeStepBG / 2;

            for (int frame = 0; frame < totalFramesCount; frame++) {
                nextSizeBG.y += sizeStepBG;
                nextSizeFillArea.y += sizeStepBG;
                nextPosMarksArea.y -= sizeStepBGDiv2;

                _marksAreaContainer.localPosition = nextPosMarksArea;
                _background.sizeDelta = nextSizeBG;
                _fillArea.sizeDelta = nextSizeFillArea;
                _handle.localScale += vecStepHandle;

                yield return new WaitForSeconds(frameDuration);
            }

            _highlighted = highlight;
            _isAnimating = false;
        }

        #endregion
    }
}
