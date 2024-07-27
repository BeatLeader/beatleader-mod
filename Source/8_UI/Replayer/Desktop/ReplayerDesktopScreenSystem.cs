using System.Collections;
using BeatLeader.Utils;
using UnityEngine;
using UnityEngine.UI;
using Screen = HMUI.Screen;

namespace BeatLeader.UI.Replayer.Desktop {
    internal class ReplayerDesktopScreenSystem : MonoBehaviour {
        #region Camera

        public void SetRenderCamera(Camera camera) {
            _screenCanvas.worldCamera = camera;
            _screenCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            _screenCanvas.sortingOrder = 1000;
        }

        #endregion

        #region Setup

        public float ScaleFactor {
            get => _screenCanvas.scaleFactor;
            set => _screenCanvas.scaleFactor = value;
        }

        public Screen Screen { get; private set; } = null!;

        private Canvas _screenCanvas = null!;
        private CanvasGroup _canvasGroup = null!;

        private void Awake() {
            CreateViewController();
            _canvasGroup.alpha = 0;
        }

        private void Start() {
            StartCoroutine(FadeAnimationCoroutine(0f, 1f));
        }

        private void CreateViewController() {
            var go = gameObject.CreateChild("Screen");
            
            _screenCanvas = go.AddComponent<Canvas>();
            _screenCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _screenCanvas.scaleFactor = 4.5f;
            _screenCanvas.planeDistance = 0.1f;
            _canvasGroup = go.AddComponent<CanvasGroup>();
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 3.44f;
            scaler.referencePixelsPerUnit = 10;
            go.AddComponent<GraphicRaycaster>();
            
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            
            Screen = go.AddComponent<Screen>();
        }

        #endregion

        #region Animation

        [SerializeField] private float animationTime = 0.4f;
        [SerializeField] private float animationFramerate = 120f;
        
        private IEnumerator FadeAnimationCoroutine(float startValue, float endValue) {
            var totalFrames = animationTime * animationFramerate;
            var timePerFrame = animationTime / animationFramerate;
            var delta = endValue - startValue;
            var sum = delta / totalFrames;

            for (var i = 0; i < totalFrames; i++) {
                yield return new WaitForSeconds(timePerFrame);
                _canvasGroup.alpha += sum;
            }
            
            _canvasGroup.alpha = endValue;
        }

        #endregion
    }
}