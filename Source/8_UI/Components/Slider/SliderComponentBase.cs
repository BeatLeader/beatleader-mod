using System;
using BeatLeader.Utils;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal abstract class SliderComponentBase<T> : TextSettingComponentBase<T> where T : ReeUIComponentV3<T> {
        #region Events

        [ExternalProperty, UsedImplicitly]
        public event Action<float>? ValueChangedEvent;

        #endregion

        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public float Value {
            get => _value;
            set {
                _value = value;
                RecalculateValue();
            }
        }

        [ExternalProperty, UsedImplicitly]
        public float ValueStep {
            get => _valueStep;
            set {
                _originalValueStep = value;
                SetValueStep(value);
                RecalculateValue();
            }
        }

        [ExternalProperty, UsedImplicitly]
        public Range ValueRange {
            get => _valueRange;
            set {
                _valueRange = value!;
                RecalculateValue();
            }
        }

        public sealed override bool Interactable {
            get => _interactable;
            set {
                _interactable = value;
                _sliderImage.color = _sliderImage.color.ColorWithAlpha(value ? 1f : 0.25f);
            }
        }

        private float _valueStep;
        private float _originalValueStep;
        private float _value;
        private Range _valueRange = new(0f, 1f);

        #endregion

        #region Actual Value
        
        private void SetValueStep(float value) {
            _valueStep = Mathf.Clamp(value, 0, ValueRange.Amplitude);
        }

        private void RecalculateValue() {
            SetValueStep(_originalValueStep);
            _value = ClampAndCeil(_value, ValueRange, ValueStep);
            var value = MathUtils.Map(
                _value,
                _valueRange.Start,
                _valueRange.End,
                0,
                1
            );
            _normalizedValue = value;
            _targetNormalizedValue = value;
            NotifyValueChanged();
        }

        private void NotifyValueChanged() {
            ValueChangedEvent?.Invoke(_value);
        }

        private static float ClampAndCeil(float value, Range range, float step) {
            value = MathUtils.RoundStepped(value, step, range.Start);
            value = Mathf.Clamp(value, range.Start, range.End);
            return value;
        }

        #endregion

        #region UI Value

        private float _targetNormalizedValue;
        private float _normalizedValue;
        private bool _set;

        private void UpdateNormalizedValue() {
            if (Mathf.Abs(_targetNormalizedValue - _normalizedValue) < 1E-05f) {
                if (_set) return;
                _normalizedValue = _targetNormalizedValue;
                SetMaterialDirty();
                _set = true;
            } else {
                SetMaterialDirty();
                _normalizedValue = Mathf.Lerp(_normalizedValue, _targetNormalizedValue, Time.deltaTime * 10f);
                _set = false;
            }
        }

        private void UpdateTargetNormalizedValue(Vector2 localPoint) {
            var rect = SettingTransform.rect;
            var amplitude = rect.width / 2f;
            //converting from rect to value range
            _targetNormalizedValue = MathUtils.Map(
                localPoint.x,
                -amplitude,
                amplitude,
                ValueRange.Start,
                ValueRange.End
            );
            //clamping value
            _targetNormalizedValue = Mathf.Clamp(
                _targetNormalizedValue,
                ValueRange.Start,
                ValueRange.End
            );
            //rounding value
            _targetNormalizedValue = MathUtils.RoundStepped(
                _targetNormalizedValue,
                ValueStep,
                ValueRange.Start
            );
            //converting to 01 for shader
            _targetNormalizedValue = MathUtils.Map(
                _targetNormalizedValue,
                ValueRange.Start,
                ValueRange.End,
                0,
                1
            );
            _value = _valueRange.GetValueClamped(_targetNormalizedValue);
            NotifyValueChanged();
        }

        #endregion

        #region ValueText

        private void UpdateValueText() {
            _valueText.text = $"{_value:F2}";
            var widthDiv = SettingTransform.rect.width / 2;
            var pos = MathUtils.Map(_normalizedValue, 0, 1, -widthDiv, widthDiv);
            pos += -Mathf.Sign(pos) * (_valueTextTransform.rect.width / 2f);
            _valueTextTransform.localPosition = new(pos, 0f);
        }

        #endregion

        #region Material

        private static readonly int normalizedValuePropertyId = Shader.PropertyToID("_NormalizedValue");
        private static readonly int colorPropertyId = Shader.PropertyToID("_Color");
        private static readonly int focusPropertyId = Shader.PropertyToID("_Focus");
        private static readonly int scalePropertyId = Shader.PropertyToID("_Scale");

        private static readonly Color trackColor = new(0.06f, 0.06f, 0.06f, 0f);

        //directly affects the aspect so only change if you know what you do
        private Vector2 _anchorSize = new(30, 5);
        private Vector2 _deltaSize;

        private Material _sliderMaterial = null!;
        private bool _materialIsDirty;

        private void SetMaterialDirty() {
            _materialIsDirty = true;
        }

        private void UpdateMaterialIfDirty() {
            if (!_materialIsDirty) return;
            _sliderMaterial.SetFloat(normalizedValuePropertyId, _normalizedValue);
            _sliderMaterial.SetColor(colorPropertyId, trackColor);
            _sliderMaterial.SetFloat(focusPropertyId, _focus);
            _sliderMaterial.SetVector(scalePropertyId, _deltaSize);
        }

        protected override void OnRectDimensionsChange() {
            _deltaSize = ContentTransform.rect.size / _anchorSize;
            SetMaterialDirty();
        }

        #endregion

        #region Setup

        protected override void OnInitialize() {
            _curvedCanvasSettings = Content.GetComponentInParent<CurvedCanvasSettings>();
            Value = 0;
            Height = 6;
            InheritWidth = true;
        }

        protected override void OnDispose() {
            Destroy(_sliderMaterial);
        }

        private void LateUpdate() {
            UpdateNormalizedValue();
            UpdateValueText();
            UpdateMaterialIfDirty();
        }

        #endregion

        #region Construct

        private ImageView _sliderImage = null!;
        
        private RectTransform _valueTextTransform = null!;
        private TMP_Text _valueText = null!;

        private PointerEventsHandler _pointerEventsHandler = null!;
        private SmoothHoverController _smoothHoverController = null!;
        private float _focus;

        protected override LayoutGroupType LayoutGroupDirection => LayoutGroupType.Horizontal;

        private RectTransform CreateSlider(Transform parent) {
            var sliderGo = parent.gameObject.CreateChild("Slider");
            var sliderRect = sliderGo.AddComponent<RectTransform>();

            _pointerEventsHandler = sliderGo.AddComponent<PointerEventsHandler>();
            _pointerEventsHandler.PointerUpdatedEvent += HandlePointerUpdated;

            _smoothHoverController = sliderGo.AddComponent<SmoothHoverController>();
            _smoothHoverController.AddStateListener(HandleHoverStateChanged, false);

            _sliderMaterial = Instantiate(GetMaterialPrefab());
            _sliderImage = sliderGo.AddComponent<AdvancedImageView>();
            _sliderImage.sprite = BundleLoader.WhiteBG;
            _sliderImage.material = _sliderMaterial;

            return sliderRect;
        }

        private void CreateValueText(Transform parent) {
            var valueTextGo = parent.gameObject.CreateChild("ValueTextContainer");
            _valueTextTransform = valueTextGo.AddComponent<RectTransform>();
            var layoutElement = valueTextGo.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
            layoutElement.preferredHeight = 3.7f;

            var group = valueTextGo.AddComponent<VerticalLayoutGroup>();
            group.padding = new(1, 1, 0, 0);

            var sizeFitter = valueTextGo.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var img = valueTextGo.AddComponent<AdvancedImageView>();
            img.sprite = BundleLoader.WhiteBG;
            img.material = GameResources.UIFogBackgroundMaterial;
            img.color = Color.black.ColorWithAlpha(0.93f);
            img.raycastTarget = false;
            img.pixelsPerUnitMultiplier = 13f;

            var textGo = valueTextGo.CreateChild("Text");
            _valueText = textGo.AddComponent<TextMeshProUGUI>();
            _valueText.alignment = TextAlignmentOptions.Midline;
            _valueText.fontSize = 3.6f;

            var textSizeFitter = textGo.AddComponent<ContentSizeFitter>();
            textSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            textGo.AddComponent<LayoutElement>();
        }

        protected sealed override RectTransform ConstructSetting(Transform parent) {
            var sliderRect = CreateSlider(parent);
            CreateValueText(sliderRect);
            return sliderRect;
        }

        protected abstract Material GetMaterialPrefab();

        #endregion

        #region Pointer Handling

        private CurvedCanvasSettings? _curvedCanvasSettings;
        private bool _interactable;
        
        private void UpdateLocalPoint(PointerEventData eventData) {
            if (!_pointerEventsHandler.IsPressed || !_interactable) return;
            var vector = eventData.TranslateToLocalPoint(SettingTransform, Canvas!, _curvedCanvasSettings);

            if (float.IsNaN(vector.x) || float.IsNaN(vector.y)) return;
            UpdateTargetNormalizedValue(vector);
        }

        #endregion

        #region Callbacks

        private void HandleHoverStateChanged(bool hovered, float progress) {
            _focus = progress;
            SetMaterialDirty();
        }

        private void HandlePointerUpdated(PointerEventsHandler handler, PointerEventData data) {
            _smoothHoverController.ForceKeepHovered = handler.IsFocused;
            UpdateLocalPoint(data);
        }

        #endregion
    }
}