using System;
using System.Collections.Generic;
using BeatLeader.Models;
using Reactive;
using UnityEngine;

namespace BeatLeader.Components {
    internal class LayoutEditor : ReactiveComponent, ILayoutEditor, ILayoutComponentHandler {
        #region Impl

        public IReadOnlyCollection<ILayoutComponent> LayoutComponents => _components;
        public RectTransform AreaTransform => ContentTransform;
        private Vector2 AreaSize => AreaTransform.rect.size;

        public LayoutEditorMode PreviousMode { get; private set; }

        public LayoutEditorMode Mode {
            get => _mode;
            set {
                PreviousMode = _mode;
                _mode = value;
                if (value is LayoutEditorMode.Edit) {
                    foreach (var component in _components) {
                        _cachedLayoutData.Add(component, component.LayoutData);
                    }
                } else {
                    _cachedLayoutData.Clear();
                }
                _layoutGrid.enabled = _mode is LayoutEditorMode.Edit;
                foreach (var comp in _components) {
                    comp.OnEditorModeChanged(value);
                }
                ModeChangedEvent?.Invoke(value);
            }
        }

        public event Action<LayoutEditorMode>? ModeChangedEvent;
        public event Action<ILayoutComponent>? ComponentAddedEvent;
        public event Action<ILayoutComponent>? ComponentRemovedEvent;

        private readonly HashSet<ILayoutComponent> _components = new();
        private readonly Dictionary<ILayoutComponent, LayoutData> _cachedLayoutData = new();

        private LayoutGrid _layoutGrid = null!;
        private LayoutEditorSettings? _settings;
        private LayoutEditorMode _mode;

        public void CancelChanges() {
            // Applying changes that was saved before entering the edit mode
            foreach (var (comp, data) in _cachedLayoutData) {
                comp.LayoutData = data;
                comp.ApplyLayoutData();
            }
        }

        public void LoadLayoutFromSettings() {
            if (_settings == null) {
                return;
            }
            var resolution = new Vector2(Screen.width, Screen.height);
            var scaleFactor = Canvas!.scaleFactor;
            _settings.Migrate(resolution, scaleFactor);
            //
            var dict = _settings!.ComponentData;
            foreach (var component in _components) {
                if (dict.TryGetValue(component.ComponentName, out var data)) {
                    component.LayoutData = data;
                    component.ApplyLayoutData();
                }
            }
        }

        public void Setup(LayoutEditorSettings settings) {
            _settings = settings;
        }

        public void AddComponent(ILayoutComponent component) {
            if (!_components.Add(component)) return;
            component.Setup(this);
            component.OnEditorModeChanged(_mode);
            ComponentAddedEvent?.Invoke(component);
        }

        public void RemoveComponent(ILayoutComponent component) {
            component.Setup(null);
            _components.Remove(component);
            ComponentRemovedEvent?.Invoke(component);
        }

        #endregion

        #region Construct & Destroy

        protected override void Construct(RectTransform rect) {
            _layoutGrid = rect.gameObject.AddComponent<LayoutGrid>();
            _layoutGrid.enabled = false;
            _mode = LayoutEditorMode.View;
        }

        protected override void OnDestroy() {
            if (_settings == null) {
                return;
            }
            foreach (var component in _components) {
                var name = component.ComponentName;
                var data = component.LayoutData;
                _settings.ComponentData[name] = data;
            }
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

        public void OnLayoutDataUpdate(ILayoutComponent component) {
            foreach (var comp in _components) {
                if (comp == component) continue;
                comp.LoadLayoutData();
                comp.ApplyLayoutData(false);
            }
        }

        Vector2 ILayoutComponentHandler.OnMove(ILayoutComponent component, Vector2 destination) {
            //modifying position
            destination = _layoutGrid.OnMove(component, destination);
            destination = ApplyBorders(destination, component.LayoutData.size);
            return destination;
        }

        Vector2 ILayoutComponentHandler.OnResize(ILayoutComponent component, Vector2 destination) {
            //modifying size
            destination = _layoutGrid.OnResize(component, destination);
            destination = ApplyBorders(destination, component.LayoutData.size);
            return destination;
        }

        void ILayoutComponentHandler.OnSelect(ILayoutComponent component) {
            foreach (var comp in _components) {
                comp.OnSelectedComponentChanged(component);
            }
        }

        private Vector2 ApplyBorders(Vector2 pos, Vector2 size) {
            return LayoutTool.ApplyBorders(pos, size, AreaSize);
        }

        #endregion
    }
}