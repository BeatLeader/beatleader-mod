using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.UI.BSML_Addons.Components;
using BeatLeader.Replays;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine.UI;
using UnityEngine;
using Zenject;
using TMPro;

namespace BeatLeader.Components
{
    internal class SpeedController : ReeUIComponentV2WithContainer
    {
        #region Components

        [UIComponent("container")]
        private RectTransform _container;

        [UIComponent("combined-speed-multiplier-text")]
        private TextMeshProUGUI _combinedSpeedMultiplierText;

        [UIComponent("speed-indicator-button")]
        private RectTransform _speedIndicatorButton;

        [UIComponent("background")]
        private Image _background;

        [UIComponent("handle")]
        private Image _handle;

        private Slider _slider;

        #endregion

        #region Configuration

        private const float _minAvailableSpeedMultiplier = 0.2f;
        private const float _maxAvailableSpeedMultiplier = 2f;

        #endregion

        #region Setup

        protected override void OnInitialize()
        {
            _slider = _container.gameObject.AddComponent<Slider>();
            _slider.targetGraphic = _handle;
            _slider.handleRect = _handle.rectTransform;
            _slider.minValue = _minAvailableSpeedMultiplier * 10;
            _slider.maxValue = _maxAvailableSpeedMultiplier * 10;
            _slider.wholeNumbers = true;
            _slider.onValueChanged.AddListener(OnSliderDrag);
            ResetSpeed();
        }

        #endregion

        #region Logic

        [Inject] private readonly PlaybackController _playbackController;

        private void OnSliderDrag(float value)
        {
            float speedMultiplier = value * 0.1f;
            _playbackController.SetSpeedMul(speedMultiplier);
            string currentMulColor = speedMultiplier == _playbackController.songSpeedMultiplier ? "yellow" :
                speedMultiplier == _minAvailableSpeedMultiplier || speedMultiplier == _maxAvailableSpeedMultiplier ? "red" : "#00ffffff" /*cyan*/;
            _combinedSpeedMultiplierText.text = $"<color={currentMulColor}>{speedMultiplier * 100}%</color> | <color=yellow>{_playbackController.songSpeedMultiplier * 100}%</color>";
        }
        [UIAction("speed-indicator-clicked")]
        private void ResetSpeed()
        {
            float value = _playbackController.songSpeedMultiplier * 10;
            OnSliderDrag(value);
            _slider.value = value;
        }

        #endregion
    }
}
