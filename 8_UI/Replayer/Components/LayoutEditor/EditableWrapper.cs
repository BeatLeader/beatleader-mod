using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.LayoutEditor.EditableWrapper.bsml")]
    internal class EditableWrapper : IDisposable
    {
        public EditableWrapper(RectTransform rect, string name, bool visible = false, bool locked = false, bool toggleEnabled = false)
        {
            _targetRect = rect;
            _name = name;
            _locked = locked;
            _enabled = toggleEnabled;
            SetEnabled(visible);
        }

        [UIValue("height")] private float height => _targetRect.sizeDelta.y;
        [UIValue("width")] private float width => _targetRect.sizeDelta.x;

        public event Action<bool> OnToggleStateChanged;

        [UIComponent("toggle")] private RectTransform _toggleContainer;
        [UIValue("locked")] private readonly bool _locked;
        [UIValue("name")] private readonly string _name;

        private RectTransform _targetRect;
        private GameObject _container;
        private ToggleWithCallbacks _toggle;
        private CanvasGroup _canvasGroup;
        private bool _enabled;

        public void Dispose()
        {
            Destroy();
        }
        public void Rebuild()
        {
            Destroy();
            Create(_targetRect);
        }
        public void SetEnabled(bool enabled = true)
        {
            SetEnabled(enabled, _enabled);
        }
        public void SetEnabled(bool enabled, bool toggleEnabled)
        {
            if (_container == null) Create(_targetRect);
            _enabled = toggleEnabled;
            if (_toggle != null && !enabled) _toggle.isOn = toggleEnabled;
            _canvasGroup.alpha = toggleEnabled ? 0.95f : 0.75f;
            _container.SetActive(enabled);
        }

        private void Create(RectTransform rect)
        {
            if (_container != null) return;
            GameObject go = new GameObject($"Editable{_name}");

            go.GetOrAddComponent<RectTransform>().sizeDelta = Vector2.zero;
            go.AddComponent<LayoutElement>().ignoreLayout = true;
            _canvasGroup = go.AddComponent<CanvasGroup>();

            BSMLParser.instance.Parse(GetType().ReadViewDefinition(), go, this);

            go.transform.SetParent(rect.parent, false);
            go.transform.localPosition = rect.localPosition;
            _container = go;
        }
        private void Destroy()
        {
            if (_container != null) GameObject.Destroy(_container);
        }

        private void OnToggleValueChanged(bool state)
        {
            OnToggleStateChanged?.Invoke(state);
            _enabled = state;
            _canvasGroup.alpha = state ? 0.95f : 0.75f;
        }
        [UIAction("#post-parse")] private void OnAfterParse()
        {
            if (_locked) return;
            _toggle = _toggleContainer.GetComponentInChildren<ToggleWithCallbacks>();
            _toggle.isOn = _enabled;
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }
}
