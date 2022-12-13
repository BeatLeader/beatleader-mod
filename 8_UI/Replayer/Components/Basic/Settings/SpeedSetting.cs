using BeatSaberMarkupLanguage.Attributes;
using UnityEngine.UI;
using UnityEngine;
using BeatLeader.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace BeatLeader.Components {
    internal class SpeedSetting : ReeUIComponentV2 {
        #region Configuration

        private const float MinAvailableSpeedMultiplier = 0.2f;
        private const float MaxAvailableSpeedMultiplier = 2f;

        #endregion

        #region UI Components

        [UIComponent("slider-container")]
        private RectTransform _sliderContainer;

        [UIComponent("handle")]
        private Image _handle;

        private Slider _slider;

        #endregion

        #region Setup

        private IBeatmapTimeController _beatmapTimeController;
        private SongSpeedData _songSpeedData;
        private bool _isInitialized;

        public void Setup(
            IBeatmapTimeController beatmapTimeController,
            SongSpeedData songSpeedData) {
            _beatmapTimeController = beatmapTimeController;
            _songSpeedData = songSpeedData;
            _isInitialized = true;
            OnSliderDrag(_slider.value = _songSpeedData.speedMul * 10);
        }

        protected override void OnInitialize() {
            _slider = _sliderContainer.gameObject.AddComponent<Slider>();
            _slider.targetGraphic = _handle;
            _slider.handleRect = _handle.rectTransform;
            _slider.minValue = MinAvailableSpeedMultiplier * 10;
            _slider.maxValue = MaxAvailableSpeedMultiplier * 10;
            _slider.wholeNumbers = true;
            _slider.onValueChanged.AddListener(OnSliderDrag);
        }

        #endregion

        #region SpeedMultiplierText

        [UIValue("speed-multiplier-text")]
        private string SpeedMultiplierText {
            get => _speedMultiplierText;
            set {
                _speedMultiplierText = value;
                NotifyPropertyChanged(nameof(SpeedMultiplierText));
            }
        }

        private string _speedMultiplierText;

        #endregion

        #region UI Callbacks

        private void OnSliderDrag(float value) {
            if (!_isInitialized 
                || float.IsInfinity(value) 
                || float.IsNaN(value)) return;

            var speedMultiplier = value * 0.1f;
            speedMultiplier = Mathf.Clamp(speedMultiplier, 
                MinAvailableSpeedMultiplier, MaxAvailableSpeedMultiplier);

            _beatmapTimeController.SetSpeedMultiplier(speedMultiplier);
            RefreshText(speedMultiplier);
        }

        #endregion

        #region RefreshText

        private void RefreshText(float mul) {
            string currentMulColor =
                mul.Equals(_songSpeedData.speedMul) ? "yellow" :
                mul.Equals(MinAvailableSpeedMultiplier) ||
                mul.Equals(MaxAvailableSpeedMultiplier) ? "red" : "#00ffffff" /*cyan*/;

            SpeedMultiplierText = $"<color={currentMulColor}>{mul * 100}%</color>" +
                $" | <color=yellow>{_songSpeedData.speedMul * 100}%</color>";
        }

        #endregion
    }
}
