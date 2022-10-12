using BeatLeader.Replayer;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine.UI;
using UnityEngine;
using Zenject;
using BeatLeader.Models;

namespace BeatLeader.Components.Settings
{
    internal class SpeedSetting : ReeUIComponentV2WithContainer
    {
        #region Configuration

        private const float minAvailableSpeedMultiplier = 0.2f;
        private const float maxAvailableSpeedMultiplier = 2f;

        #endregion

        #region Injection

        [Inject] private readonly IBeatmapTimeController _beatmapTimeController;
        [Inject] private readonly SongSpeedData _songSpeedData;

        #endregion

        #region SpeedMultiplierText

        [UIValue("speed-multiplier-text")]
        private string _SpeedMultiplierText
        {
            get => _speedMultiplierText;
            set
            {
                _speedMultiplierText = value;
                NotifyPropertyChanged(nameof(_SpeedMultiplierText));
            }
        }

        #endregion

        #region Components

        [UIComponent("slider-container")]
        private RectTransform _sliderContainer;

        [UIComponent("handle")]
        private Image _handle;

        private string _speedMultiplierText;
        private Slider _slider;

        #endregion

        #region Logic

        protected override void OnInitialize()
        {
            _slider = _sliderContainer.gameObject.AddComponent<Slider>();
            _slider.targetGraphic = _handle;
            _slider.handleRect = _handle.rectTransform;
            _slider.minValue = minAvailableSpeedMultiplier * 10;
            _slider.maxValue = maxAvailableSpeedMultiplier * 10;
            _slider.wholeNumbers = true;
            _slider.onValueChanged.AddListener(OnSliderDrag);
            ResetSpeed();
        }
        private void OnSliderDrag(float value)
        {
            float speedMultiplier = value * 0.1f;
            if (speedMultiplier > maxAvailableSpeedMultiplier
                || speedMultiplier < minAvailableSpeedMultiplier) return;

            _beatmapTimeController.SetSpeedMultiplier(speedMultiplier);
            string currentMulColor =
                speedMultiplier.Equals(_songSpeedData.speedMul) ? "yellow" :
                speedMultiplier.Equals(minAvailableSpeedMultiplier) ||
                speedMultiplier.Equals(maxAvailableSpeedMultiplier) ? "red" : "#00ffffff" /*cyan*/;

            _SpeedMultiplierText = $"<color={currentMulColor}>{speedMultiplier * 100}%</color>" +
                $" | <color=yellow>{_songSpeedData.speedMul * 100}%</color>";
        }
        private void ResetSpeed()
        {
            OnSliderDrag(_slider.value = _songSpeedData.speedMul * 10);
        }

        #endregion
    }
}
