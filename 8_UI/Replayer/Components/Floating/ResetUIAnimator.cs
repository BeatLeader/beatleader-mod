using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components
{
    internal class ResetUIAnimator : MonoBehaviour
    {
        public event Action<bool> RevealWasFinishedEvent;

        public Image image;
        public int animationFrameRate = 120;
        private bool _wasCancelled;
        private bool _animating;

        public void StartAnimation(float duration)
        {
            if (_animating) return;
            StartCoroutine(RevealAnimationCoroutine(duration));
        }
        public void CancelAnimation()
        {
            if (!_animating) return;
            _wasCancelled = true;
        }

        private IEnumerator RevealAnimationCoroutine(float duration)
        {
            _animating = true;
            image.fillAmount = 0;
            image.color = GetColorWithModifiedOpacity(image.color, 1);

            float framesCount = Mathf.FloorToInt(duration * animationFrameRate);
            float frameDuration = duration / framesCount;
            float step = 1 / framesCount;

            for (int frame = 0; frame < framesCount; frame++)
            {
                if (_wasCancelled) break;
                image.fillAmount = image.fillAmount + step;
                yield return new WaitForSeconds(frameDuration);
            }

            RevealWasFinishedEvent?.Invoke(!_wasCancelled);
            StartCoroutine(DissolveAnimationCoroutine(duration * 0.2f, _wasCancelled));
        }
        private IEnumerator DissolveAnimationCoroutine(float duration, bool flowBack)
        {
            float framesCount = Mathf.FloorToInt(animationFrameRate * duration);
            float frameDuration = duration / framesCount;
            float fadeStep = 1 / framesCount;
            float fillStep = image.fillAmount / framesCount;

            for (int frame = 0; frame < framesCount; frame++)
            {
                image.color = GetColorWithModifiedOpacity(image.color, image.color.a - fadeStep);
                if (flowBack) image.fillAmount -= fillStep;
                yield return new WaitForSeconds(frameDuration);
            }

            _wasCancelled = false;
            _animating = false;
        }
        private Color GetColorWithModifiedOpacity(Color color, float opacity)
        {
            color.a = opacity;
            return color;
        }
    }
}
