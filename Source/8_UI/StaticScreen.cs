using BeatSaberMarkupLanguage;
using UnityEngine;
using VRUIControls;

namespace BeatLeader.Components {
    internal class StaticScreen : MonoBehaviour {
        private CanvasGroup _canvasGroup = null!;
        private float _targetAlpha;
        private bool _set;
        
        public void Present() {
            _targetAlpha = 1f;
            _set = false;
            gameObject.SetActive(true);
        }

        public void Dismiss() {
            _targetAlpha = 0f;
            _set = false;
        }

        private void Update() {
            if (_set) {
                return;
            }
            var val =  Mathf.Lerp(_canvasGroup.alpha, _targetAlpha, Time.deltaTime * 8f);
            if (Mathf.Abs(_targetAlpha - val) < 0.001f) {
                val = _targetAlpha;
                _set = true;
                gameObject.SetActive(Mathf.Approximately(val, 1f));
            }
            _canvasGroup.alpha = val;
        }

        private void Awake() {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.referencePixelsPerUnit = 10f;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;
            canvas.sortingOrder = 5;
            //
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            // Raycaster
            var raycaster = gameObject.AddComponent<VRGraphicRaycaster>();
            BeatSaberUI.DiContainer.Inject(raycaster);
            //
            transform.localScale = Vector3.one * 0.02f;
        }
    }
}