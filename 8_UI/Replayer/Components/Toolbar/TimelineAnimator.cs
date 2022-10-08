using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components
{
    internal class TimelineAnimator : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerUpHandler,
        IPointerDownHandler
    {
        #region Configuration

        private const float _handleExpandSize = 0.8f;
        private const float _backgroundExpandSize = 1f;
        private const float _animationFrameRate = 60f;
        private const float _expandTransitionDuration = 0.15f;
        private const float _shrinkTransitionDuration = 0.1f;

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
            RectTransform fillArea)
        {
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

        private bool _normalStateTrigger = true;
        private bool _pressedStateTrigger;
        private bool _highlightedStateTrigger;

        private void Update()
        {
            if (!_initialized) return;

            _normalStateTrigger = !_pressedStateTrigger 
                && !_highlightedStateTrigger;

            SetState(!_normalStateTrigger);
        }

        public void OnPointerEnter(PointerEventData data)
        {
            _highlightedStateTrigger = true;
        }
        public void OnPointerExit(PointerEventData data)
        {
            _highlightedStateTrigger = false;
        }
        public void OnPointerUp(PointerEventData data)
        {
            _pressedStateTrigger = false;
            HandleReleasedEvent?.Invoke();
        }
        public void OnPointerDown(PointerEventData data)
        {
            _pressedStateTrigger = true;
            HandlePressedEvent?.Invoke();
        }

        #endregion

        #region Animation

        private bool _highlighted;
        private bool _inProcess;

        private bool SetState(bool highlighted = true)
        {
            if (_inProcess || highlighted == _highlighted) return false;
            _inProcess = true;
            StartCoroutine(AnimationCoroutine(
                highlighted ? _expandTransitionDuration : _shrinkTransitionDuration,
                highlighted));
            return true;
        }
        private IEnumerator AnimationCoroutine(float duration, bool highlight)
        {
            float totalFramesCount = Mathf.FloorToInt(duration * _animationFrameRate);
            float frameDuration = duration / totalFramesCount;
            float sizeStepBG = _backgroundExpandSize / totalFramesCount;
            float sizeStepHandle = _handleExpandSize / totalFramesCount;

            for (int frame = 0; frame < totalFramesCount; frame++)
            {
                Vector2 nextSizeBG = new Vector2(_background.sizeDelta.x,
                    highlight ? _background.sizeDelta.y + sizeStepBG : _background.sizeDelta.y - sizeStepBG);

                Vector2 nextSizeFillArea = new Vector2(_fillArea.sizeDelta.x,
                    highlight ? _fillArea.sizeDelta.y + sizeStepBG : _fillArea.sizeDelta.y - sizeStepBG);

                Vector2 nextSizeHandle = new Vector2(
                    highlight ? _handle.localScale.x + sizeStepHandle : _handle.localScale.x - sizeStepHandle,
                    highlight ? _handle.localScale.y + sizeStepHandle : _handle.localScale.y - sizeStepHandle);

                var sizeStepBGDiv2 = sizeStepBG / 2;
                Vector2 nextPosMarksArea = new Vector2(_marksAreaContainer.localPosition.x,
                    highlight ? _marksAreaContainer.localPosition.y - sizeStepBGDiv2 :
                    _marksAreaContainer.localPosition.y + sizeStepBGDiv2);

                _marksAreaContainer.localPosition = nextPosMarksArea;
                _background.sizeDelta = nextSizeBG;
                _fillArea.sizeDelta = nextSizeFillArea;
                _handle.localScale = nextSizeHandle;

                yield return new WaitForSeconds(frameDuration);
            }

            _highlighted = highlight;
            _inProcess = false;
        }

        #endregion
    }
}
