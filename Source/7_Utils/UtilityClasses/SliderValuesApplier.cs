using HMUI;
using System;

namespace BeatLeader
{
    internal class SliderValuesApplier
    {
        public SliderValuesApplier(RangeValuesTextSlider slider)
        {
            _slider = slider;
        }

        public float multiplier = 1;
        private RangeValuesTextSlider _slider;
        private int _increment;

        public int max
        {
            get => (int)_slider.maxValue;
            set => _slider.maxValue = value;
        }
        public int min
        {
            get => (int)_slider.minValue;
            set => _slider.minValue = value;
        }
        public int increment
        {
            get => _increment;
            set => _slider.numberOfSteps = _increment = (int)Math.Round((double)(max - min) / value) + 1;
        }
        public int value
        {
            get => (int)_slider.value;
            set
            {
                _slider.value = value;
            }
        }
        public float multipliedValue
        {
            get => _slider.value * multiplier;
        }
    }
}
