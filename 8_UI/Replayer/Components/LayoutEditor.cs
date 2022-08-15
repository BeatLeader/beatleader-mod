using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage;
using BeatLeader.Utils;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using HMUI;

namespace BeatLeader.Components
{
    [SerializeAutomatically]
    internal class LayoutEditor : ReeUIComponentV2WithContainer
    {
        [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.LayoutEditor.EditableElement.bsml")]
        private class EditableElement : IDisposable
        {
            public EditableElement(string name, RectTransform rect, bool enabled = true, bool locked = false)
            {
                this.name = name;
                this.locked = locked;
                ParentRect = rect;
                ParentEnabled = enabled;
                _parentCanvasGroup = rect.gameObject.GetOrAddComponent<CanvasGroup>();
                _parentCanvasGroup.alpha = ParentEnabled ? 1 : 0;
            }

            public GameObject Container { get; private set; }
            public RectTransform ParentRect { get; private set; }
            public bool ParentEnabled { get; private set; }
            public bool Enabled { get; private set; }

            public event Action<string, bool> OnStateChanged;

            #region BSML Stuff

            [UIComponent("toggle")] private readonly RectTransform toggleContainer;

            [UIValue("name")] public readonly string name;
            [UIValue("locked")] public readonly bool locked;

            [UIValue("height")] private float Height => ParentRect.sizeDelta.y;
            [UIValue("width")] private float Width => ParentRect.sizeDelta.x;

            #endregion

            private ToggleWithCallbacks _toggle;
            private CanvasGroup _canvasGroup;
            private CanvasGroup _parentCanvasGroup;
            private bool _tempParentEnabled;

            public void Dispose()
            {
                if (Enabled) return;
                RemoveEditable();
                Destroy(_canvasGroup);
            }
            public void SetEnabled(bool enabled, bool applySettings = true, bool notify = true)
            {
                if (enabled) CreateEditable(ParentRect);
                else RemoveEditable(applySettings);

                if (enabled) OnToggleValueChanged(ParentEnabled);
                else _parentCanvasGroup.alpha = ParentEnabled ? 1 : 0;

                if (notify) OnStateChanged?.Invoke(name, ParentEnabled);
            }
            private void CreateEditable(RectTransform rect)
            {
                if (Container != null) return;
                GameObject go = new GameObject($"Editable{name}");

                go.GetOrAddComponent<RectTransform>().sizeDelta = Vector2.zero;
                go.AddComponent<LayoutElement>().ignoreLayout = true;
                _canvasGroup = go.AddComponent<CanvasGroup>();

                BSMLParser.instance.Parse(Utilities.GetResourceContent(GetType().Assembly,
                    GetType().GetCustomAttribute<ViewDefinitionAttribute>().Definition), go, this);

                go.transform.SetParent(rect.parent, false);
                go.transform.localPosition = rect.localPosition;
                Container = go;
            }
            private void RemoveEditable(bool applySettings = true)
            {
                if (Container != null) Destroy(Container);
                if (applySettings) ParentEnabled = _tempParentEnabled;
            }

            private void OnToggleValueChanged(bool state)
            {
                if (_canvasGroup != null) _canvasGroup.alpha = state ? 0.95f : 0.75f;
                _parentCanvasGroup.alpha = state ? 0.7f : 0.4f;
                _tempParentEnabled = state;
            }
            [UIAction("#post-parse")] private void OnAfterParse()
            {
                OnToggleValueChanged(ParentEnabled);
                if (locked) return;
                _toggle = toggleContainer.GetComponentInChildren<ToggleWithCallbacks>();
                _toggle.isOn = ParentEnabled;
                _toggle.onValueChanged.AddListener(OnToggleValueChanged);
            }
        }

        [SerializeAutomatically] private static Dictionary<string, bool> _objectsConfig = new();

        private Dictionary<string, EditableElement> _editableObjects = new();
        public bool EditMode { get; private set; }

        public void SetEditModeEnabled(bool enabled, bool applySettings = true)
        {
            if (enabled == EditMode) return;
            _editableObjects.ToList().ForEach(x => x.Value.SetEnabled(enabled, applySettings));
            content.gameObject.SetActive(enabled);
            EditMode = enabled;
        }
        public bool TryAddObject(string name, RectTransform rect, bool loadConfig = true, bool enabled = true, bool locked = false)
        {
            bool state = _objectsConfig.ContainsKey(name) && loadConfig ? _objectsConfig[name] : enabled;
            var editable = new EditableElement(name, rect, state, locked);
            editable.OnStateChanged += SaveSettings;
            return _editableObjects.TryAdd(name, editable);
        }
        public bool TryRemoveObject(string name)
        {
            if (EditMode) return false;
            if (_editableObjects.TryGetValue(name, out EditableElement value))
            {
                _editableObjects.Remove(name);
                value.Dispose();
                return true;
            }
            else return false;
        }

        private void SaveSettings(string name, bool value)
        {
            if (!_objectsConfig.TryAdd(name, value)) _objectsConfig[name] = value;
        }

        #region BSML Stuff

        [UIComponent("window")] private RectTransform _window;
        [UIComponent("handle")] private RectTransform _handle;

        protected override void OnInitialize()
        {
            content.gameObject.SetActive(false);
            _window.gameObject.AddComponent<WindowView>().handle = _handle;
            _handle.gameObject.AddComponent<HighlightableView>().Init(Color.cyan, Color.yellow);
        }

        [UIAction("done-button-clicked")] private void OnDoneButtonClicked() => SetEditModeEnabled(false);
        [UIAction("cancel-button-clicked")] private void OnCancelButtonClicked() => SetEditModeEnabled(false, false);

        #endregion
    }
}
