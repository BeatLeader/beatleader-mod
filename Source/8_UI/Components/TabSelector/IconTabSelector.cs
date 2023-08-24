using System;
using System.Collections.Generic;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class IconTabSelector : LayoutComponentBase<IconTabSelector> {
        #region Events

        [ExternalProperty, UsedImplicitly]
        public event Action<string>? TabSelectedEvent;

        #endregion

        #region UI Properties

        public class TabParamsDescriptor {
            [ExternalProperty, UsedImplicitly]
            public bool GrowOnHover { get; set; }

            [ExternalProperty, UsedImplicitly]
            public bool ColorizeOnHover { get; set; } = true;

            [ExternalProperty, UsedImplicitly]
            public float HoverLerpMul { get; set; } = 10f;

            [ExternalProperty, UsedImplicitly]
            public Color ActiveColor { get; set; } = AdvancedButton.DefaultHoveredColor;

            [ExternalProperty, UsedImplicitly]
            public Color HoveredColor { get; set; } = Color.grey;

            [ExternalProperty, UsedImplicitly]
            public Color Color { get; set; } = AdvancedButton.DefaultColor;

            [ExternalProperty, UsedImplicitly]
            public RectOffset Pad { get; set; } = new();

            [ExternalProperty, UsedImplicitly]
            public Vector3 HoverScaleSum { get; set; } = new(0.2f, 0.2f, 0.2f);
            
            [ExternalProperty, UsedImplicitly]
            public Vector3 BaseScale { get; set; } = Vector3.one;
        }

        [ExternalProperty(prefix: "tab")]
        public readonly TabParamsDescriptor tabParams = new();

        [ExternalProperty, UsedImplicitly]
        public IList<KeyValuePair<string, Sprite>> Tabs {
            get => _tabs;
            set {
                _tabs = value;
                if (!IsInitialized) return;
                RefreshButtonWrappers();
            }
        }

        private IList<KeyValuePair<string, Sprite>> _tabs = Array.Empty<KeyValuePair<string, Sprite>>();

        #endregion

        #region Buttons

        private class TabSelectorButton : ReeUIComponentV3<TabSelectorButton> {
            #region Events

            public event Action<string, bool>? ToggleEvent;

            #endregion

            #region SetActivated

            public void SetActivated(bool activated) {
                _button.Toggle(activated);
            }

            #endregion

            #region Refresh

            public string Key { get; private set; } = string.Empty;

            public void Refresh(string key, Sprite icon) {
                if (_tabSelector is null) return;
                var desc = _tabSelector.tabParams;
                _button.GrowOnHover = desc.GrowOnHover;
                _button.HoverLerpMul = desc.HoverLerpMul;
                _button.ActiveColor = desc.ActiveColor;
                _button.HoveredColor = desc.HoveredColor;
                _button.Color = desc.Color;
                _button.Pad = desc.Pad;
                _button.BaseScale = desc.BaseScale;
                _button.HoverScaleSum = desc.HoverScaleSum;
                _button.Image.Icon = icon;
                Key = key;
            }

            #endregion

            #region Setup

            private AdvancedButton _button = null!;
            private IconTabSelector? _tabSelector;

            public void Setup(IconTabSelector selector) {
                _tabSelector = selector;
            }

            protected override void OnDispose() {
                Destroy(_button);
            }

            protected override GameObject Construct() {
                _button = AdvancedButton.Instantiate(ContentTransform!);
                _button.ToggleEvent += HandleButtonToggled;
                _button.Sticky = true;
                _button.InheritHeight = true;
                return _button.Content!;
            }

            #endregion

            #region Callbacks

            private void HandleButtonToggled(bool state) {
                ToggleEvent?.Invoke(Key, state);
            }

            #endregion
        }

        private readonly List<TabSelectorButton> _buttons = new();

        private void RefreshButtonWrappers() {
            while (_buttons.Count < _tabs.Count) {
                var wrapper = TabSelectorButton.Instantiate(ContentTransform!);
                wrapper.Setup(this);
                wrapper.ToggleEvent += HandleButtonToggled;
                _buttons.Add(wrapper);
            }
            var index = 0;
            _buttons.ForEach(static x => x.Content!.SetActive(false));
            foreach (var (key, value) in _tabs) {
                var wrapper = _buttons[index];
                wrapper.Refresh(key, value);
                wrapper.Content!.SetActive(true);
                index++;
            }
        }

        private void RefreshButtonStates(string keepActiveKey) {
            foreach (var btn in _buttons) {
                btn.SetActivated(btn.Key == keepActiveKey);
            }
        }

        #endregion

        #region Setup

        protected override LayoutGroupType LayoutGroup => LayoutGroupType.Horizontal;

        public void SelectTab(string tabName) {
            ValidateAndThrow();
            RefreshButtonStates(tabName);
        }

        protected override void OnPropertySet() {
            RefreshButtonWrappers();
        }

        protected override void OnInitialize() {
            RefreshButtonWrappers();
        }

        protected override void OnDispose() {
            _buttons.ForEach(static x => Destroy(x));
            _buttons.Clear();
        }

        #endregion

        #region Callbacks

        private void HandleButtonToggled(string key, bool state) {
            if (!state) return;
            RefreshButtonStates(key);
            TabSelectedEvent?.Invoke(key);
        }

        #endregion
    }
}