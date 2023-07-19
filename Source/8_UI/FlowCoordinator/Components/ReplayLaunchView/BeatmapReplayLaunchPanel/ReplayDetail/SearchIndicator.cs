using System.Collections;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class SearchIndicator : ReeUIComponentV2 {
        #region UI Components

        [UIComponent("image"), UsedImplicitly]
        private RectTransform _imageRect = null!;

        #endregion

        #region Init

        public float ImageSize {
            set => _imageRect.sizeDelta = new(value, value);
        }

        public float radius = 1f;
        public float speed = 1f;

        private float _angle;

        protected override void OnRootStateChange(bool active) {
            if (!active) StopAllCoroutines();
            else StartCoroutine(AnimationCoroutine());
        }

        protected override void OnInitialize() {
            ImageSize = 6f;
        }

        #endregion

        #region Animation

        private bool _shouldBeStopped;

        private IEnumerator AnimationCoroutine() {
            while (true) {
                if (_shouldBeStopped) {
                    _shouldBeStopped = false;
                    yield break;
                }
                _imageRect.localPosition = new Vector3(
                    Mathf.Cos(_angle),
                    Mathf.Sin(_angle)
                ) * radius;
                _imageRect.localEulerAngles = new Vector3(
                    0, 0,
                    MathUtils.Map(Mathf.Sin(_angle), -1, 1, -20, 20)
                );
                _angle += speed * Time.deltaTime;
                yield return null;
            }
        }

        #endregion
    }
}