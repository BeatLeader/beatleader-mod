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
    internal class LayoutEditor : ReeUIComponentV2WithContainer
    {
        [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.LayoutEditor.EditableElement.bsml")]
        private class EditableElement : IDisposable
        {
            public EditableElement(string name, RectTransform rect, bool enabled = true, bool locked = false)
            {
                this.name = name;
                this.locked = locked;
                parentRect = rect;
                parentEnabled = enabled;
                _parentCanvasGroup = rect.gameObject.GetOrAddComponent<CanvasGroup>();
                TryLoadSettings();
            }

            private ToggleWithCallbacks _toggle;
            private CanvasGroup _canvasGroup;
            private CanvasGroup _parentCanvasGroup;
            private bool _tempParentEnabled;

            #region BSML Info

            [UIComponent("toggle")] private readonly RectTransform toggleContainer;

            [UIValue("name")] public readonly string name;
            [UIValue("locked")] public readonly bool locked;

            [UIValue("height")] private float _height => parentRect.sizeDelta.y;
            [UIValue("width")] private float _width => parentRect.sizeDelta.x;

            #endregion

            public GameObject container { get; private set; }
            public RectTransform parentRect { get; private set; }
            public bool parentEnabled { get; private set; }
            public bool enabled { get; private set; }

            public void Dispose()
            {
                if (enabled) return;
                RemoveEditable();
                Destroy(_canvasGroup);
            }
            public void SetEnabled(bool enabled, bool applySettings = true)
            {
                if (enabled) CreateEditable(parentRect);
                else RemoveEditable(applySettings);
                _parentCanvasGroup.alpha = parentEnabled ? 1 : 0;
            }
            private void CreateEditable(RectTransform rect)
            {
                if (container != null) return;
                GameObject go = new GameObject($"Editable{name}");

                go.GetOrAddComponent<RectTransform>().sizeDelta = Vector2.zero;
                go.AddComponent<LayoutElement>().ignoreLayout = true;
                _canvasGroup = go.AddComponent<CanvasGroup>();

                BSMLParser.instance.Parse(Utilities.GetResourceContent(GetType().Assembly,
                    GetType().GetCustomAttribute<ViewDefinitionAttribute>().Definition), go, this);

                go.transform.SetParent(rect.parent, false);
                go.transform.localPosition = rect.localPosition;
                container = go;
            }
            private void RemoveEditable(bool applySettings = true)
            {
                if (container == null) return;
                Destroy(container);
                if (applySettings) parentEnabled = _tempParentEnabled;
            }
            private void TryLoadSettings()
            {
                try
                {
                    if (ReplayerConfig.instance.UIComponents.TryGetValue(name, out bool value))
                    {
                        _parentCanvasGroup.alpha = value ? 1f : 0f;
                        parentEnabled = value;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Can not load layout configuration for {name} because of unhandled exception! {ex}");
                }
            }

            private void OnToggleValueChanged(bool state)
            {
                if (_canvasGroup != null) _canvasGroup.alpha = state ? 0.95f : 0.75f;
                _parentCanvasGroup.alpha = state ? 0.7f : 0.4f;
                _tempParentEnabled = state;
            }
            [UIAction("#post-parse")] private void OnAfterParse()
            {
                OnToggleValueChanged(parentEnabled);
                if (!locked)
                {
                    _toggle = toggleContainer.GetComponentInChildren<ToggleWithCallbacks>();
                    _toggle.isOn = parentEnabled;
                    _toggle.onValueChanged.AddListener(OnToggleValueChanged);
                }
            }
        }

        private Dictionary<string, EditableElement> _editableObjects = new();
        public bool editMode { get; private set; }

        public void SetEditModeEnabled(bool enabled, bool applySettings = true)
        {
            if (enabled == editMode) return;
            _editableObjects.ToList().ForEach(x => x.Value.SetEnabled(enabled, applySettings));
            content.gameObject.SetActive(enabled);
            editMode = enabled;
            SaveSettings();
        }
        public void AddObject(string name, RectTransform rect, bool enabled = true, bool locked = false)
        {
            try
            {
                _editableObjects.Add(name, new EditableElement(name, rect, enabled, locked));
            }
            catch (Exception _) {
                throw new Exception("Object with the same name is already added!");
            }
        }
        public void RemoveObject(string name)
        {
            if (editMode) return;
            if (_editableObjects.TryGetValue(name, out EditableElement value))
            {
                _editableObjects.Remove(name);
                value.Dispose();
            }
            else throw new Exception($"Can not find object named \"{name}\"");
        }
        private void SaveSettings()
        {
            try
            {
                ReplayerConfig.instance.UIComponents.Clear();
                _editableObjects.ToList().ForEach(x => ReplayerConfig.instance.UIComponents.Add(x.Key, x.Value.parentEnabled));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Can not save layout configuration because of unhandled exception! {ex}");
            }
        }

        #region BSML Stuff

        [UIComponent("window")] private RectTransform _window;
        [UIComponent("handle")] private RectTransform _handle;

        protected override void OnInitialize()
        {
            content.gameObject.SetActive(false);
            _window.gameObject.AddComponent<UIWindow>().handle = _handle;
            _handle.gameObject.AddComponent<UIHighlightable>().Init(Color.cyan, Color.yellow);
        }

        [UIAction("done-button-clicked")] private void OnDoneButtonClicked() => SetEditModeEnabled(false);
        [UIAction("cancel-button-clicked")] private void OnCancelButtonClicked() => SetEditModeEnabled(false, false);

        #endregion
    }
}
