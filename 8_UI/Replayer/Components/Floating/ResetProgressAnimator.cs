using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class ResetProgressAnimator : MonoBehaviour {
        #region Configuration

        private const int animationFrameRate = 120;

        #endregion

        #region Events

        public event Action<bool> RevealWasFinishedEvent;
        public event Action RevealWasStartedEvent;

        #endregion

        #region Setup

        private Image _image;
        private bool _wasCancelled;
        private bool _animating;

        public void SetImage(Image image) {
            _image = image;
        }

        #endregion

        #region Cancel & Start

        public void StartAnimation(float duration) {
            if (_animating) return;
            StartCoroutine(RevealAnimationCoroutine(duration));
        }
        public void CancelAnimation() {
            if (!_animating) return;
            _wasCancelled = true;
        }

        #endregion

        #region Animation

        private IEnumerator RevealAnimationCoroutine(float duration) {
            _animating = true;
            _image.fillAmount = 0;
            _image.color = GetColorWithModifiedOpacity(_image.color, 1);

            float framesCount = Mathf.FloorToInt(duration * animationFrameRate);
            float frameDuration = duration / framesCount;
            float step = 1 / framesCount;

            RevealWasStartedEvent?.Invoke();

            for (int frame = 0; frame < framesCount; frame++) {
                if (_wasCancelled) break;
                _image.fillAmount += step;
                yield return new WaitForSeconds(frameDuration);
            }

            RevealWasFinishedEvent?.Invoke(!_wasCancelled);
            StartCoroutine(DissolveAnimationCoroutine(duration * 0.2f, _wasCancelled));
        }
        private IEnumerator DissolveAnimationCoroutine(float duration, bool flowBack) {
            float framesCount = Mathf.FloorToInt(animationFrameRate * duration);
            float frameDuration = duration / framesCount;
            float fadeStep = 1 / framesCount;
            float fillStep = _image.fillAmount / framesCount;

            for (int frame = 0; frame < framesCount; frame++) {
                _image.color = GetColorWithModifiedOpacity(_image.color, _image.color.a - fadeStep);
                if (flowBack) _image.fillAmount -= fillStep;
                yield return new WaitForSeconds(frameDuration);
            }

            _wasCancelled = false;
            _animating = false;
        }

        #endregion

        #region Modify color

        private Color GetColorWithModifiedOpacity(Color color, float opacity) {
            color.a = opacity;
            return color;
        }

        #endregion
    }
}
