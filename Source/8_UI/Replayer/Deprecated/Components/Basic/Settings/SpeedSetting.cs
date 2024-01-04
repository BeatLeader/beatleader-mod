using BeatSaberMarkupLanguage.Attributes;
using UnityEngine.UI;
using UnityEngine;
using BeatLeader.Models;

namespace BeatLeader.Components {
    internal class SpeedSetting : ReeUIComponentV2 {
        #region Configuration

        private const float MinAvailableSpeedMultiplier = 1f;
        private const float MaxAvailableSpeedMultiplier = 200f;

        #endregion

        #region UI Components

        [UIComponent("slider-container")]
        private readonly RectTransform _sliderContainer = null!;

        [UIComponent("handle")]
        private readonly Image _handle = null!;

        private Slider _slider = null!;

        #endregion

        #region Setup

        private IBeatmapTimeController _beatmapTimeController = null!;
        private bool _isInitialized;

        public void Setup(IBeatmapTimeController beatmapTimeController) {
            _beatmapTimeController = beatmapTimeController;
            _isInitialized = true;
            OnSliderDrag(_slider.value = _beatmapTimeController.SongStartSpeedMultiplier * 100f);
        }

        protected override void OnInitialize() {
            _slider = _sliderContainer.gameObject.AddComponent<Slider>();
            _slider.targetGraphic = _handle;
            _slider.handleRect = _handle.rectTransform;
            _slider.minValue = MinAvailableSpeedMultiplier;
            _slider.maxValue = MaxAvailableSpeedMultiplier;
            _slider.wholeNumbers = true;
            _slider.onValueChanged.AddListener(OnSliderDrag);
        }

        #endregion

        #region SpeedMultiplierText

        [UIValue("speed-multiplier-text")]
        public string SpeedMultiplierText {
            get => _speedMultiplierText;
            private set {
                _speedMultiplierText = value;
                NotifyPropertyChanged(nameof(SpeedMultiplierText));
            }
        }

        private string _speedMultiplierText = null!;

        #endregion

        #region UI Callbacks

        private void OnSliderDrag(float value) {
            if (!_isInitialized 
                || float.IsInfinity(value) 
                || float.IsNaN(value)) return;
            
            value = Mathf.Clamp(value, MinAvailableSpeedMultiplier, MaxAvailableSpeedMultiplier);
            _beatmapTimeController.SetSpeedMultiplier(value / 100f);
            RefreshText(value);
        }

        #endregion

        #region RefreshText

        private void RefreshText(float mul) {
            string currentMulColor =
                mul.Equals(_beatmapTimeController.SongStartSpeedMultiplier) ? "yellow" :
                mul.Equals(MinAvailableSpeedMultiplier) ||
                mul.Equals(MaxAvailableSpeedMultiplier) ? "red" : "#00ffffff" /*cyan*/;

            SpeedMultiplierText = $"<color={currentMulColor}>{mul}%</color>" +
                $" | <color=yellow>{_beatmapTimeController.SongStartSpeedMultiplier * 100f}%</color>";
        }

        #endregion
    }
}
