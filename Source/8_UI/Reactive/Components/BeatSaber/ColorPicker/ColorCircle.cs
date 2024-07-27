using System.Linq;
using HMUI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BeatLeader.UI.Reactive.Components {
    internal class ColorCircle : ReactiveComponent {
        #region Color
        
        public Color ImmediateColor { get; private set; }
        public Color Color { get; private set; }

        public void SetColor(Color color) {
            _colorPicker.color = color;
            HandleColorChanged(color, ColorChangeUIEventType.PointerUp);
        }
        
        #endregion

        #region Construct

        protected override float? DesiredHeight => 54f;
        protected override float? DesiredWidth => 54f;

        private HSVPanelController _colorPicker = null!;
        private ColorPickerButtonController _pickerButton = null!;

        protected override GameObject Construct() {
            _colorPicker = InstantiateColorPicker();
            _colorPicker.colorDidChangeEvent += HandleColorChanged;
            _pickerButton = _colorPicker.GetComponentInChildren<ColorPickerButtonController>();
            _pickerButton.GetComponent<Touchable>().enabled = false;
            _pickerButton.SetColor(_colorPicker.color);
            return _colorPicker.gameObject;
        }

        protected override void OnInitialize() {
            this.WithSizeDelta(54f, 54f);
        }

        private static HSVPanelController InstantiateColorPicker() {
            var original = Resources.FindObjectsOfTypeAll<HSVPanelController>().First();
            var clone = Object.Instantiate(original);
            return clone;
        }

        #endregion

        #region Callbacks

        private void HandleColorChanged(Color color, ColorChangeUIEventType type) {
            ImmediateColor = color;
            _pickerButton.SetColor(color);
            NotifyPropertyChanged(nameof(ImmediateColor));
            if (type is not ColorChangeUIEventType.PointerUp) return;
            Color = color;
            NotifyPropertyChanged(nameof(Color));
        }

        #endregion
    }
}