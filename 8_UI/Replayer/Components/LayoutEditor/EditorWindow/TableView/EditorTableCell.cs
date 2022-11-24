using BeatLeader.UI.BSML_Addons.Components;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System.ComponentModel;
using UnityEngine;

namespace BeatLeader.Components {
    internal class EditorTableCell : ReeUIComponentV2 {
        private static readonly Color defaultColor = new(0.3f, 0.3f, 0.3f);
        private static readonly Color selectedColor = new(0, 170, 230);

        [UIValue("name")]
        public string Name => Element?.Name ?? string.Empty;

        [UIValue("layer")]
        public int Layer => Element?.Layer ?? 0;

        public EditableElement Element { get; private set; }
        public TableCell Cell { get; private set; }

        #region Setup

        [UIComponent("bg-image")]
        private readonly BetterImage _bgImage;

        [UIValue("visibility-toggle")]
        private ToggleButton _visibilityToggle;

        public void Setup(EditableElement element) {
            if (Element != null)
                Element.PropertyChanged -= HandleElementPropertyChanged;
            Element = element;
            Element.PropertyChanged += HandleElementPropertyChanged;
            RefreshVisibilityToggle();
            RefreshTexts();
        }

        protected override void OnInstantiate() {
            _visibilityToggle = Instantiate<ToggleButton>(transform);
        }

        protected override void OnInitialize() {
            Content.SetParent(transform);
            Cell = gameObject.AddComponent<TableCell>();
            Cell.selectionDidChangeEvent += HandleCellSelectionChanged;
            Cell.reuseIdentifier = "EditorTableCell";
            _visibilityToggle.EnabledSprite = BundleLoader.EyeIcon;
            _visibilityToggle.DisabledSprite = BundleLoader.TransparentPixel;
            _visibilityToggle.OnToggle += HandleToggleStateChanged;
        }

        private void RefreshVisibilityToggle() {
            if (Element == null) return;
            _visibilityToggle.Toggle(Element.DefaultLayoutMap.enabled);
        }

        public void RefreshTexts() {
            NotifyPropertyChanged(nameof(Name));
            NotifyPropertyChanged(nameof(Layer));
        }

        #endregion

        #region Callbacks

        private void HandleCellSelectionChanged(SelectableCell cell, SelectableCell.TransitionType transition, object obj) {
            _bgImage.Image.color = cell.selected ? selectedColor : defaultColor;
        }

        private void HandleToggleStateChanged(bool state) {
            Element.tempLayoutMap.enabled = state;
            Element.SetWrapperPseudoState(state);
        }

        private void HandleElementPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(EditableElement.State):
                    RefreshVisibilityToggle();
                    break;
                case nameof(EditableElement.Layer):
                    RefreshTexts();
                    break;
            }
        }

        #endregion
    }
}
