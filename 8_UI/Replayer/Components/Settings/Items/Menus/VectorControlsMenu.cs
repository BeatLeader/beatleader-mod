using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using System;
using TMPro;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace BeatLeader.Components.Settings
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.VectorControlsMenu.bsml")]
    internal class VectorControlsMenu : Menu
    {
        public SliderValuesApplier xSlider { get; private set; }
        public SliderValuesApplier ySlider { get; private set; }
        public SliderValuesApplier zSlider { get; private set; }
        public int max
        {
            set
            {
                xSlider.max = value;
                ySlider.max = value;
                zSlider.max = value;
            }
        }
        public int min
        {
            set
            {
                xSlider.min = value;
                ySlider.min = value;
                zSlider.min = value;
            }
        }
        public int increment
        {
            get => 0;
            set
            {
                xSlider.increment = value;
                ySlider.increment = value;
                zSlider.increment = value;
            }
        }
        public float multiplier
        {
            set
            {
                xSlider.multiplier = value;
                ySlider.multiplier = value;
                zSlider.multiplier = value;
            }
        }
        public bool showText
        {
            get => _textContainer.activeSelf;
            set => _textContainer.SetActive(value);
        }
        public int dimensions
        {
            get => _dimensions;
            set
            {
                _dimensions = value;
                _xSliderContainer.SetActive(value >= 1);
                _ySliderContainer.SetActive(value >= 2);
                _zSliderContainer.SetActive(value >= 3);
            }
        }
        public Vector3 multipliedVector
        {
            get => new Vector3(xSlider.multipliedValue, ySlider.multipliedValue, zSlider.multipliedValue);
            set => vector = new Vector3(value.x / xSlider.multiplier, value.y / ySlider.multiplier, value.z / zSlider.multiplier);
        }
        public Vector3 vector
        {
            get => new Vector3(xSlider.value, ySlider.value, zSlider.value);
            set
            {
                x = (int)value.x;
                y = (int)value.y;
                z = (int)value.z;
            }
        }

        public event Action<Vector3> VectorChangedEvent;

        #region BSML Stuff

        [UIValue("x")] private int x
        {
            get => xSlider != null ? xSlider.value : 0;
            set
            {
                xSlider.value = value;
                NotifyVectorChanged();
                NotifyPropertyChanged(nameof(x));
            }
        }
        [UIValue("y")] private int y
        {
            get => ySlider != null ? ySlider.value : 0;
            set
            {
                ySlider.value = value;
                NotifyVectorChanged();
                NotifyPropertyChanged(nameof(y));
            }
        }
        [UIValue("z")] private int z
        {
            get => zSlider != null ? zSlider.value : 0;
            set
            {
                zSlider.value = value;
                NotifyVectorChanged();
                NotifyPropertyChanged(nameof(z));
            }
        }

        [UIValue("max")] private int _max;
        [UIValue("min")] private int _min;
        [UIValue("increment")] private int _increment;

        [UIObject("text-container")] private GameObject _textContainer;
        [UIObject("x-slider-container")] private GameObject _xSliderContainer;
        [UIObject("y-slider-container")] private GameObject _ySliderContainer;
        [UIObject("z-slider-container")] private GameObject _zSliderContainer;

        [UIComponent("text")] private TextMeshProUGUI _vectorText;
        [UIComponent("x-slider")] private SliderSetting _xSlider;
        [UIComponent("y-slider")] private SliderSetting _ySlider;
        [UIComponent("z-slider")] private SliderSetting _zSlider;

        #endregion

        private int _dimensions = 3;

        protected override void OnAfterParse()
        {
            xSlider = new(_xSlider.slider);
            ySlider = new(_ySlider.slider);
            zSlider = new(_zSlider.slider);
            UpdateVectorText();
        }
        private void UpdateVectorText()
        {
            double round(float t) => Math.Round(t, 2);
            string line = $"<color=\"green\">X:{round(multipliedVector.x)} ";
            if (_dimensions >= 2) line += $"<color=\"red\">Y:{round(multipliedVector.y)} ";
            if (_dimensions >= 3) line += $"<color=\"blue\">Z:{round(multipliedVector.z)}";
            _vectorText.text = line;
        }
        private void NotifyVectorChanged()
        {
            UpdateVectorText();
            VectorChangedEvent?.Invoke(multipliedVector);
        }
    }
}
