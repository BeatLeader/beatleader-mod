using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.LayoutEditor.EditableWrapper.bsml")]
    internal class EditableWrapper : IDisposable, INotifyPropertyChanged
    {
        public EditableWrapper(RectTransform rect, string name, bool visible = false, bool locked = false, bool toggleEnabled = false)
        {
            _targetRect = rect;
            _name = name;
            _locked = locked;
            _enabled = toggleEnabled;
            SetEditableEnabled(visible);
        }

        [UIValue("height")] private float _Height => _targetRect.sizeDelta.y;
        [UIValue("width")] private float _Width => _targetRect.sizeDelta.x;

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
        public void Refresh()
        {
            NotifyPropertyChanged(nameof(_Height));
            NotifyPropertyChanged(nameof(_Width));
            _container.transform.localPosition = _targetRect.localPosition;
        }
        public void SetEnabled(bool enabled)
        {
            if (_locked) return;

            Create(_targetRect);
            _toggle.isOn = enabled;
            NotifyToggleValueChanged(enabled);
        }
        public void SetEditableEnabled(bool enabled = true)
        {
            SetEditableEnabled(enabled, _enabled);
        }
        public void SetEditableEnabled(bool enabled, bool toggleEnabled)
        {
            Create(_targetRect);
            _enabled = toggleEnabled;
            if (_toggle != null && !enabled) _toggle.isOn = toggleEnabled;
            _canvasGroup.alpha = toggleEnabled ? 0.95f : 0.75f;
            _container.SetActive(enabled);  
            Refresh();
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

            if (_locked) return;
            _toggle = _toggleContainer.GetComponentInChildren<ToggleWithCallbacks>();
            _toggle.isOn = _enabled;
            _toggle.onValueChanged.AddListener(NotifyToggleValueChanged);
        }
        private void Destroy()
        {
            if (_container != null) GameObject.Destroy(_container);
        }

        private void NotifyToggleValueChanged(bool state)
        {
            OnToggleStateChanged?.Invoke(state);
            _enabled = state;
            _canvasGroup.alpha = state ? 0.95f : 0.75f;
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
