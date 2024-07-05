using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using UnityEngine;

namespace BeatLeader.Components {
    internal interface ILayoutEditor : ILayoutComponentHandler {
        IReadOnlyCollection<ILayoutComponent> LayoutComponents { get; }
        ILayoutComponentTransformsHandler? AdditionalComponentHandler { get; set; }
        bool PartialDisplayModeActive { get; set; }

        event Action<bool>? StateChangedEvent;
        event Action<ILayoutComponent?>? ComponentSelectedEvent;

        void SetEditorActive(bool active, bool saveCurrentState = false);
        void RefreshComponents();
        void Setup(LayoutEditorSettings settings);
    }

    internal class LayoutEditor : ReactiveComponent, ILayoutEditor {
        #region Events

        public bool PartialDisplayModeActive {
            get => _partialDisplayActive;
            set {
                _partialDisplayActive = value;
                SetPartialDisplayModeActive(value);
            }
        }

        public event Action<bool>? StateChangedEvent;
        public event Action<ILayoutComponent?>? ComponentSelectedEvent;

        #endregion

        #region Setup

        public RectTransform AreaTransform => ContentTransform;
        public IReadOnlyCollection<ILayoutComponent> LayoutComponents => _components;
        private Vector2 AreaSize => AreaTransform.rect.size;

        public ILayoutComponentTransformsHandler? AdditionalComponentHandler { get; set; }

        private readonly HashSet<ILayoutComponent> _components = new();
        private LayoutEditorSettings? _settings;
        private bool _provideCachedPosition;
        private bool _saveCurrentState;
        private bool _editorActive;
        private bool _partialDisplayActive;
        private bool _lastPartialDisplayActive;

        private void SetPartialDisplayModeActive(bool active) {
            if (_editorActive) return;
            foreach (var component in _components) {
                if (!_layoutDatas.TryGetValue(component, out var data)) continue;
                component.ComponentController.ComponentActive = !active || data.active;
            }
        }

        public void SetEditorActive(bool active, bool saveCurrentState = true) {
            _editorActive = active;
            if (active) {
                _lastPartialDisplayActive = _partialDisplayActive;
                SetPartialDisplayModeActive(false);
            }
            //loading
            _provideCachedPosition = true;
            _saveCurrentState = !active && saveCurrentState;
            foreach (var component in _components) {
                component.WrapperController.SetWrapperActive(active);
            }
            _provideCachedPosition = false;
            //applying partial display back if was enabled
            if (!active) {
                SetPartialDisplayModeActive(_lastPartialDisplayActive);
            }
            StateChangedEvent?.Invoke(active);
            ComponentSelectedEvent?.Invoke(null);
        }

        public void RefreshComponents() {
            _provideCachedPosition = true;
            foreach (var component in _components) {
                component.RequestRefresh();
            }
            _provideCachedPosition = false;
            SetPartialDisplayModeActive(false);
        }

        public void Setup(LayoutEditorSettings settings) {
            _settings = settings;
        }

        protected override void Construct(RectTransform rect) { }

        protected override void OnDestroy() {
            if (_settings is null) return;
            _settings.ComponentDatas = _layoutDatas.ToDictionary(
                static pair => pair.Key.ComponentName,
                static pair => pair.Value
            );
        }

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
                    AreaTransform,
                    Input.mousePosition,
                    Canvas?.worldCamera,
                    out var pos
                );
                return pos;
            }
        }

        Vector2 ILayoutComponentTransformsHandler.OnMove(
            ILayoutComponent component, Vector2 origin, Vector2 destination
        ) {
            if (_provideCachedPosition && !_saveCurrentState) {
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
            if (_provideCachedPosition && _saveCurrentState) {
                //saving data
                ModifyLayoutData(
                    component,
                    p => {
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
            if (_provideCachedPosition && !_saveCurrentState) {
                //applying size
                destination = AcquireLayoutData(component).size;
            }
            //modifying size
            destination = AdditionalComponentHandler?.OnResize(component, origin, destination) ?? destination;
            destination = ApplyBorders(destination, component.ComponentController.ComponentSize);
            if (_provideCachedPosition && _saveCurrentState) {
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