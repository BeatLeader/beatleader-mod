using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class SkillTriangle : ReeUIComponentV2 {
        #region Components

        [UIComponent("image-component"), UsedImplicitly]
        private ImageView _triangleImage;

        [UIComponent("text-component-a"), UsedImplicitly]
        private TextMeshProUGUI _textComponentA;

        [UIComponent("text-component-b"), UsedImplicitly]
        private TextMeshProUGUI _textComponentB;

        [UIComponent("text-component-c"), UsedImplicitly]
        private TextMeshProUGUI _textComponentC;

        #endregion

        #region Initialize / Dispose

        protected override void OnInitialize() {
            InitializeMaterial();
            InitializeLabels();
        }

        #endregion

        #region SetValues

        private const float MaxRating = 15.0f;
        private const string StarSymbol = "<size=70%>★</size>";

        public void SetValues(float techRating, float accRating, float passRating) {
            _textComponentA.text = FormatLabel("Tech", techRating);
            _textComponentB.text = FormatLabel("Acc", accRating);
            _textComponentC.text = FormatLabel("Pass", passRating);

            _normalizedValues.x = Mathf.Clamp01(techRating / MaxRating);
            _normalizedValues.y = Mathf.Clamp01(accRating / MaxRating);
            _normalizedValues.z = Mathf.Clamp01(passRating / MaxRating);
            UpdateMaterialProperties();
        }

        #endregion

        #region Labels

        private static string FormatLabel(string label, float value) {
            return $"{label}: {FormatUtils.FormatStars(value)}";
        }

        private void InitializeLabels() {
            _textComponentA.transform.localPosition = new Vector3(-12.0f, 12f, 0.0f);
            _textComponentB.transform.localPosition = new Vector3(12.0f, 12f, 0.0f);
            _textComponentC.transform.localPosition = new Vector3(0.0f, -12f, 0.0f);
        }

        #endregion

        #region Material

        private static readonly int normalizedValuesPropertyId = Shader.PropertyToID("_Normalized");

        private Material _materialInstance;
        private Vector4 _normalizedValues = Vector4.zero;

        private void InitializeMaterial() {
            _materialInstance = Material.Instantiate(BundleLoader.SkillTriangleMaterial);
            _triangleImage.material = _materialInstance;
            UpdateMaterialProperties();
        }

        private void UpdateMaterialProperties() {
            _materialInstance.SetVector(normalizedValuesPropertyId, _normalizedValues);
        }

        #endregion
    }
}