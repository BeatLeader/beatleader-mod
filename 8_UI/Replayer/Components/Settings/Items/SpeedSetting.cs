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

namespace BeatLeader.Components.Settings
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.SpeedSetting.bsml")]
    internal class SpeedSetting : Setting
    {
        private const float _minAvailableSpeedMultiplier = 0.2f;
        private const float _maxAvailableSpeedMultiplier = 2f;

        [Inject] private readonly PlaybackController _playbackController;
        [Inject] private readonly BeatmapTimeController _timeController;

        [UIComponent("slider-container")] private RectTransform _sliderContainer;
        [UIComponent("handle")] private Image _handle;

        [UIValue("speed-multiplier-text")] private string speedMultiplierText
        {
            get => _speedMultiplierText;
            set
            {
                _speedMultiplierText = value;
                NotifyPropertyChanged(nameof(speedMultiplierText));
            }
        }

        public override int SettingIndex => 3;

        private string _speedMultiplierText;
        private Slider _slider;

        protected override void OnAfterHandling()
        {
            _slider = _sliderContainer.gameObject.AddComponent<Slider>();
            _slider.targetGraphic = _handle;
            _slider.handleRect = _handle.rectTransform;
            _slider.minValue = _minAvailableSpeedMultiplier * 10;
            _slider.maxValue = _maxAvailableSpeedMultiplier * 10;
            _slider.wholeNumbers = true;
            _slider.onValueChanged.AddListener(OnSliderDrag);
            ResetSpeed();
        }
        private void OnSliderDrag(float value)
        {
            float speedMultiplier = value * 0.1f;
            _timeController.SetSpeedMultiplier(speedMultiplier);
            string currentMulColor = speedMultiplier == _playbackController.SongSpeedMultiplier ? "yellow" :
                speedMultiplier == _minAvailableSpeedMultiplier || speedMultiplier == _maxAvailableSpeedMultiplier ? "red" : "#00ffffff" /*cyan*/;
            speedMultiplierText = $"<color={currentMulColor}>{speedMultiplier * 100}%</color> | <color=yellow>{_playbackController.SongSpeedMultiplier * 100}%</color>";
        }
        private void ResetSpeed()
        {
            float value = _playbackController.SongSpeedMultiplier * 10;
            OnSliderDrag(value);
            _slider.value = value;
        }
    }
}
