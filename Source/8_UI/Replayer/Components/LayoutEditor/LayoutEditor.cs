using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal interface ILayoutEditor : ILayoutComponentHandler {
        IReadOnlyCollection<ILayoutComponent> LayoutComponents { get; }
        ILayoutComponentTransformsHandler? AdditionalComponentHandler { get; set; }

        event Action<bool>? StateChangedEvent;
        event Action<ILayoutComponent?>? ComponentSelectedEvent;

        void SetEditorActive(bool active, bool saveCurrentState = false);
        void Setup(LayoutEditorSettings settings);
    }

    internal class LayoutEditor : LayoutComponentBase<LayoutEditor>, ILayoutEditor {
        #region Events

        public event Action<bool>? StateChangedEvent;
        public event Action<ILayoutComponent?>? ComponentSelectedEvent;

        #endregion

        #region Setup

        public RectTransform? AreaTransform => ContentTransform as RectTransform;
        public IReadOnlyCollection<ILayoutComponent> LayoutComponents => _components;
        private Vector2 AreaSize => AreaTransform!.rect.size;

        public ILayoutComponentTransformsHandler? AdditionalComponentHandler { get; set; }

        private readonly HashSet<ILayoutComponent> _components = new();
        private LayoutEditorSettings? _settings;
        private bool _isChangingEditorState;
        private bool _saveCurrentState;

        public void SetEditorActive(bool active, bool saveCurrentState = true) {
            ValidateAndThrow();
            _isChangingEditorState = true;
            _saveCurrentState = !active && saveCurrentState;
            foreach (var component in _components) {
                component.WrapperController.SetWrapperActive(active);
            }
            _isChangingEditorState = false;
            StateChangedEvent?.Invoke(active);
            ComponentSelectedEvent?.Invoke(null);
        }

        public void Setup(LayoutEditorSettings settings) {
            _settings = settings;
        }

        protected override void OnInitialize() {
            LayoutGroup.childControlHeight = false;
            LayoutGroup.childControlWidth = false;
            LayoutGroup.childForceExpandHeight = false;
            LayoutGroup.childForceExpandWidth = false;
            Content!.GetComponent<LayoutGroup>().enabled = false;
        }

        protected override void OnDispose() {
            var settings = new LayoutEditorSettings {
                ComponentDatas = _layoutDatas.ToDictionary(
                    static pair => pair.Key.ComponentName,
                    static pair => pair.Value
                )
            };
            ConfigFileData.Instance.ReplayerSettings.LayoutEditorSettings = settings;
        }

        protected override bool OnValidation() => AreaTransform is not null;

        #endregion

        #region Handling Tools

        private static void AddDefaultOrLoad<T>(
            IDictionary<string, T>? configDict,
            IDictionary<ILayoutComponent, T> localDict,
            ILayoutComponent component,
            Func<T> activator
        ) {
            if (!(configDict?.TryGetValue(component.ComponentName, out var data) ?? false)) data = activator();
            localDict[component] = data;
        }

        #endregion

        #region LayoutData Handling

        private readonly Dictionary<ILayoutComponent, LayoutData> _layoutDatas = new();

        private void AddDefaultOrLoadLayoutData(ILayoutComponent component) {
            AddDefaultOrLoad(
                _settings?.ComponentDatas,
                _layoutDatas,
                component,
                () => new() {
                    active = true
                }
            );
        }

        private void ModifyLayoutData(ILayoutComponent component, Action<LayoutData> predicate) {
            var data = _layoutDatas[component];
            predicate(data);
            _layoutDatas[component] = data;
        }

        private LayoutData AcquireLayoutData(ILayoutComponent component) {
            if (!_layoutDatas.ContainsKey(component)) AddDefaultOrLoadLayoutData(component);
            return _layoutDatas[component];
        }

        #endregion

        #region Component Handling

        public void AddComponent(ILayoutComponent component) {
            ValidateAndThrow();
            if (!_components.Add(component)) return;
            component.Setup(this);
        }

        public void RemoveComponent(ILayoutComponent component) {
            ValidateAndThrow();
            component.Setup(null);
            _components.Remove(component);
        }

        #endregion

        #region Movement Handling

        Vector2 ILayoutComponentHandler.PointerPosition {
            get {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    AreaTransform, Input.mousePosition, null, out var pos
                );
                return pos;
            }
        }

        Vector2 ILayoutComponentTransformsHandler.OnMove(
            ILayoutComponent component, Vector2 origin, Vector2 destination
        ) {
            if (_isChangingEditorState && !_saveCurrentState) {
                //applying position
                var layoutData = AcquireLayoutData(component);
                destination = layoutData.position;
                //applying other properties
                var controller = component.ComponentController;
                controller.ComponentLayer = layoutData.layer;
                controller.ComponentActive = layoutData.active;
            }
            //modifying position
            destination = AdditionalComponentHandler?.OnMove(component, origin, destination) ?? destination;
            destination = ApplyBorders(destination, component.ComponentController.ComponentSize);
            if (_isChangingEditorState && _saveCurrentState) {
                //saving data
                ModifyLayoutData(
                    component, p => {
                        var controller = component.ComponentController;
                        p.layer = controller.ComponentLayer;
                        p.active = controller.ComponentActive;
                        p.position = destination;
                    }
                );
            }
            return destination;
        }

        Vector2 ILayoutComponentTransformsHandler.OnResize(
            ILayoutComponent component, Vector2 origin, Vector2 destination
        ) {
            if (_isChangingEditorState && !_saveCurrentState) {
                //applying size
                destination = AcquireLayoutData(component).size;
            }
            //modifying size
            destination = AdditionalComponentHandler?.OnResize(component, origin, destination) ?? destination;
            destination = ApplyBorders(destination, component.ComponentController.ComponentSize);
            if (_isChangingEditorState && _saveCurrentState) {
                //saving data
                ModifyLayoutData(component, p => p.size = destination);
            }
            return destination;
        }

        void ILayoutComponentHandler.OnSelect(ILayoutComponent component) {
            ComponentSelectedEvent?.Invoke(component);
        }

        private Vector2 ApplyBorders(Vector2 pos, Vector2 size) {
            return LayoutTool.ApplyBorders(pos, size, AreaSize);
        }

        #endregion
    }
}