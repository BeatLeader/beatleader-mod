using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using System;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class VectorControlsContentView : ContentView {
        #region Config

        public SliderValuesApplier XSlider { get; private set; }
        public SliderValuesApplier YSlider { get; private set; }
        public SliderValuesApplier ZSlider { get; private set; }
        public int Max {
            set {
                XSlider.max = value;
                YSlider.max = value;
                ZSlider.max = value;
            }
        }
        public int Min {
            set {
                XSlider.min = value;
                YSlider.min = value;
                ZSlider.min = value;
            }
        }
        public int Increment {
            get => 0;
            set {
                XSlider.increment = value;
                YSlider.increment = value;
                ZSlider.increment = value;
            }
        }
        public float Multiplier {
            set {
                XSlider.multiplier = value;
                YSlider.multiplier = value;
                ZSlider.multiplier = value;
            }
        }
        public bool ShowText {
            get => _textContainer.activeSelf;
            set => _textContainer.SetActive(value);
        }
        public int Dimensions {
            get => _dimensions;
            set {
                _dimensions = value;
                _xSliderContainer.SetActive(value >= 1);
                _ySliderContainer.SetActive(value >= 2);
                _zSliderContainer.SetActive(value >= 3);
            }
        }
        public Vector3 MultipliedVector {
            get => new Vector3(XSlider.multipliedValue, YSlider.multipliedValue, ZSlider.multipliedValue);
            set => Vector = new Vector3(value.x / XSlider.multiplier, value.y / YSlider.multiplier, value.z / ZSlider.multiplier);
        }
        public Vector3 Vector {
            get => new Vector3(XSlider.value, YSlider.value, ZSlider.value);
            set {
                X = (int)value.x;
                Y = (int)value.y;
                z = (int)value.z;
            }
        }

        #endregion

        #region Events

        public event Action<Vector3> VectorChangedEvent;

        #endregion

        #region UI Values

        [UIValue("x")]
        private int X {
            get => XSlider != null ? XSlider.value : 0;
            set {
                XSlider.value = value;
                NotifyVectorChanged();
                NotifyPropertyChanged(nameof(X));
            }
        }

        [UIValue("y")]
        private int Y {
            get => YSlider != null ? YSlider.value : 0;
            set {
                YSlider.value = value;
                NotifyVectorChanged();
                NotifyPropertyChanged(nameof(Y));
            }
        }

        [UIValue("z")]
        private int z {
            get => ZSlider != null ? ZSlider.value : 0;
            set {
                ZSlider.value = value;
                NotifyVectorChanged();
                NotifyPropertyChanged(nameof(z));
            }
        }

        [UIValue("max")] private int _max;
        [UIValue("min")] private int _min;
        [UIValue("increment")] private int _increment;

        #endregion

        #region UI Components

        [UIObject("text-container")] private GameObject _textContainer;
        [UIObject("x-slider-container")] private GameObject _xSliderContainer;
        [UIObject("y-slider-container")] private GameObject _ySliderContainer;
        [UIObject("z-slider-container")] private GameObject _zSliderContainer;

        [UIComponent("text")] private TextMeshProUGUI _vectorText;
        [UIComponent("x-slider")] private SliderSetting _xSlider;
        [UIComponent("y-slider")] private SliderSetting _ySlider;
        [UIComponent("z-slider")] private SliderSetting _zSlider;

        #endregion

        #region Setup

        private int _dimensions = 3;

        protected override void OnInitialize() {
            XSlider = new(_xSlider.slider);
            YSlider = new(_ySlider.slider);
            ZSlider = new(_zSlider.slider);
            UpdateVectorText();
        }

        private void UpdateVectorText() {
            _vectorText.text = FormatUtils.FormatLocation(MultipliedVector, dimensions: _dimensions);
        }

        private void NotifyVectorChanged() {
            UpdateVectorText();
            VectorChangedEvent?.Invoke(MultipliedVector);
        }

        #endregion
    }
}
