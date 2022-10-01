using BeatLeader.Replayer;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine.UI;
using UnityEngine;
using Zenject;

namespace BeatLeader.Components.Settings
{
    internal class SpeedSetting : ReeUIComponentV2WithContainer
    {
        private const float minAvailableSpeedMultiplier = 0.2f;
        private const float maxAvailableSpeedMultiplier = 2f;

        [Inject] private readonly BeatmapTimeController _beatmapTimeController;
        [Inject] private readonly GameplayModifiers _gameplayModifiers;

        #region BSML

        [UIComponent("slider-container")] 
        private RectTransform _sliderContainer;

        [UIComponent("handle")]
        private Image _handle;

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

        private string _speedMultiplierText;
        private Slider _slider;

        #endregion

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
                speedMultiplier.Equals(_gameplayModifiers.songSpeedMul) ? "yellow" :
                speedMultiplier.Equals(minAvailableSpeedMultiplier) || 
                speedMultiplier.Equals(maxAvailableSpeedMultiplier) ? "red" : "#00ffffff" /*cyan*/;

            _SpeedMultiplierText = $"<color={currentMulColor}>{speedMultiplier * 100}%</color>" +
                $" | <color=yellow>{_gameplayModifiers.songSpeedMul * 100}%</color>";
        }
        private void ResetSpeed()
        {
            OnSliderDrag(_slider.value = _gameplayModifiers.songSpeedMul * 10);
        }
    }
}
