using BeatLeader.Replayer;
using BeatLeader.Replayer.Poses;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

namespace BeatLeader.Components.Settings
{
    [SerializeAutomatically]
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Settings.Items.Vector3MatrixMenu.bsml")]
    internal class Vector3MatrixMenu : Menu
    {
        [UIValue("max")] public int max
        {
            get => _max;
            set
            {
                _xSlider.slider.maxValue = value;
                _ySlider.slider.maxValue = value;
                _zSlider.slider.maxValue = value;
                _max = value;
            }
        }
        [UIValue("min")] public int min
        {
            get => _min;
            set
            {
                _xSlider.slider.minValue = value;
                _ySlider.slider.minValue = value;
                _zSlider.slider.minValue = value;
                _min = value;
            }
        }
        [UIValue("increment")] public int increment
        {
            get => _increment;
            set
            {
                var val = (int)Math.Round((double)(max - min) / value) + 1;
                _xSlider.slider.numberOfSteps = val;
                _ySlider.slider.numberOfSteps = val;
                _zSlider.slider.numberOfSteps = val;
                _increment = value;
            }
        }
        public bool showText
        {
            get => _textContainer.activeSelf;
            set => _textContainer.SetActive(value);
        }
        public Vector3 vector
        {
            get => _vector;
            set
            {
                _vector = value;
                NotifyVectorChanged();
                NotifyPropertyChanged(nameof(x));
                NotifyPropertyChanged(nameof(y));
                NotifyPropertyChanged(nameof(z));
            }
        }

        public event Action<Vector3> OnVectorChanged;

        public float multiplier = 1;

        [UIValue("x")] private int x
        {
            get => Mathf.CeilToInt(vector.x / multiplier);
            set
            {
                _vector.x = value * multiplier;
                NotifyVectorChanged();
                NotifyPropertyChanged(nameof(x));
            }
        }
        [UIValue("y")] private int y
        {
            get => Mathf.CeilToInt(vector.y / multiplier);
            set
            {
                _vector.y = value * multiplier;
                NotifyVectorChanged();
                NotifyPropertyChanged(nameof(y));
            }
        }
        [UIValue("z")] private int z
        {
            get => Mathf.CeilToInt(vector.z / multiplier);
            set
            {
                _vector.z = value * multiplier;
                NotifyVectorChanged();
                NotifyPropertyChanged(nameof(z));
            }
        }

        [UIObject("text-container")] private GameObject _textContainer;
        [UIComponent("text")] private TextMeshProUGUI _vectorText;
        [UIComponent("x-slider")] private SliderSetting _xSlider;
        [UIComponent("y-slider")] private SliderSetting _ySlider;
        [UIComponent("z-slider")] private SliderSetting _zSlider;

        private int _increment;
        private int _max;
        private int _min;
        private Vector3 _vector;

        protected override void OnAfterParse()
        {
            UpdateVectorText();
        }
        private void UpdateVectorText()
        {
            double round(float t) => Math.Round(t, 2);
            _vectorText.text = $"<color=\"green\">X:{round(_vector.x)} <color=\"red\">Y:{round(_vector.y)} <color=\"blue\">Z:{round(_vector.z)}";
        }
        private void NotifyVectorChanged()
        {
            UpdateVectorText();   
            OnVectorChanged?.Invoke(_vector);
        }
    }
}
