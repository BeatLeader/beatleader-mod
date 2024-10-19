using UnityEngine;

namespace BeatLeader.UI.Rendering {
    [RequireComponent(typeof(RectTransform))]
    internal class BlurPostProcessor : PostProcessModule {
        #region Setup

        public int lodFactor = 1;

        private Material? _applicatorMaterial;
        private string? _applicatorPropertyName;
        private bool _isInitialized;

        private Material _blurMaterial = null!;
        private Material _plotterMaterial = null!;
        private int _propertyId;

        private RenderTexture _renderTexture = null!;
        private RenderTexture _tempRenderTexture = null!;
        private RectTransform _rectTransform = null!;
        private RectTransform _canvasRectTransform = null!;

        private static readonly int mainTexPropertyId = Shader.PropertyToID("_MainTex");
        private static readonly int areaPropertyId = Shader.PropertyToID("_Area");

        public void Init(Material applicatorMaterial, string applicatorPropertyName) {
            _applicatorMaterial = applicatorMaterial;
            _applicatorPropertyName = applicatorPropertyName;
            _propertyId = Shader.PropertyToID(_applicatorPropertyName);
            _isInitialized = true;
        }

        private void Awake() {
            _rectTransform = GetComponent<RectTransform>();
            _blurMaterial = Instantiate(BundleLoader.Materials.blurMaterial);
            _plotterMaterial = Instantiate(BundleLoader.Materials.plotterMaterial);
            _canvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        }

        private void OnDestroy() {
            Destroy(_blurMaterial);
            Destroy(_plotterMaterial);
        }

        private void Start() {
            ReloadRenderTexture();
        }

        private void OnRectTransformDimensionsChange() {
            if (!_rectTransform) return;
            ReloadRenderTexture();
        }

        private void ReloadRenderTexture() {
            if (_renderTexture != null) {
                Destroy(_renderTexture);
                Destroy(_tempRenderTexture);
            }
            var rect = _rectTransform.rect;
            if (rect.width <= 0 || rect.height <= 0) return;
            _renderTexture = new RenderTexture(
                (int)rect.width / lodFactor,
                (int)rect.height / lodFactor,
                0,
                RenderTextureFormat.ARGB32
            );
            _tempRenderTexture = new RenderTexture(_renderTexture);
        }

        #endregion

        #region Render

        public override void Process(RenderTexture texture) {
            if (!_isInitialized) {
                throw new UninitializedComponentException();
            }
            var corners = GetMappedCorners();
            RenderSampledTexture(texture, corners);
            // rendering blur
            _blurMaterial.SetTexture(_propertyId, _tempRenderTexture);
            Graphics.Blit(_tempRenderTexture, _renderTexture, _blurMaterial);
            // applying final texture
            _applicatorMaterial!.SetTexture(_propertyId, _renderTexture);
        }

        private void RenderSampledTexture(RenderTexture texture, Vector4 corners) {
            _plotterMaterial.SetVector(areaPropertyId, corners);
            _plotterMaterial.SetTexture(mainTexPropertyId, texture);
            Graphics.Blit(texture, _tempRenderTexture, _plotterMaterial);
        }

        #endregion

        #region Clipping

        private Vector4 GetMappedCorners() {
            var corners = GetRectCorners();
            var width = _canvasRectTransform.rect.width;
            var height = _canvasRectTransform.rect.height;
            return new Vector4(
                Mathf.Clamp01(corners.x / width),
                Mathf.Clamp01(corners.y / height),
                Mathf.Clamp01(corners.z / width),
                Mathf.Clamp01(corners.w / height)
            );
        }

        private Vector4 GetRectCorners() {
            var pos = (Vector2)_rectTransform.localPosition + new Vector2(Screen.width, Screen.height) * 0.5f;
            var size = _rectTransform.rect.size;
            var pivot = _rectTransform.pivot;
            // Create a rect in screen space
            return new Vector4(
                pos.x - pivot.x * size.x,
                pos.y - pivot.y * size.y,
                pos.x + (1 - pivot.x) * size.x,
                pos.y + (1 - pivot.y) * size.y
            );
        }

        #endregion
    }
}