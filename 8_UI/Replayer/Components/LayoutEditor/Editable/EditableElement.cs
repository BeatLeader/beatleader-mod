using BeatLeader.Models;
using System;
using UnityEngine;

namespace BeatLeader.Components {
    internal abstract class EditableElement : ReeUIComponentV2 {
        public virtual LayoutMapData DefaultLayoutMap { get; protected set; }

        public abstract string Name { get; }
        public bool State {
            get => Content.gameObject.activeSelf;
            set {
                Content.gameObject.SetActive(value);
                NotifyPropertyChanged(nameof(State));
            }
        }
        public int Layer {
            get => Content.GetSiblingIndex();
            set {
                Content.SetSiblingIndex(value);
                NotifyPropertyChanged(nameof(Layer));
            }
        }
        public int TotalLayersCount {
            get => Content.parent.childCount;
        }

        public Vector2 LastWindowCursorPos {
            get => _wrapperWindow.LastCursorPos;
        }
        public Vector2 Position {
            get => Content.localPosition;
            private set => Content.localPosition = value;
        }
        public Vector2 Size {
            get => Content.GetComponent<RectTransform>().rect.size;
        }
        public Vector2 GridAnchor {
            get => _wrapperWindow.anchor;
            set => _wrapperWindow.anchor = value;
        }

        protected virtual RectTransform WrapperContainer {
            get => _contentRect;
        }
        public GlassWrapper Wrapper { get; private set; }

        public event Action<EditableElement> ElementPositionChangedEvent;
        public event Action<EditableElement> ElementSelectedEvent;

        public LayoutMapData tempLayoutMap;

        private ILayoutEditor _editor;
        private GridLayoutWindow _wrapperWindow;
        private RectTransform _contentRect;
        private bool _isInitialized;

        public void Setup(ILayoutEditor editor) {
            if (!IsParsed) return;
            RefreshWrapper();
            _editor = editor;
            _wrapperWindow.gridModel = _editor.LayoutGrid;
            _isInitialized = true;
        }
        public void ApplyMap(LayoutMapData layoutMap) {
            if (!_isInitialized) return;
            DefaultLayoutMap = layoutMap;
            Position = _editor.Map(layoutMap.position, Size, layoutMap.anchor);
            Layer = layoutMap.layer;
            State = layoutMap.enabled;
            SetWrapperPseudoState(layoutMap.enabled);
        }
        public void RefreshWrapper() {
            Wrapper.ComponentName = Name;
        }
        public void SetWrapperPseudoState(bool state) {
            Wrapper.BgFixedOpacity = state ? GlassWrapper.EnabledOpacity : GlassWrapper.DisabledOpacity;
        }

        private void HandleWrapperStateChanged(bool state) {
            _wrapperWindow.enabled = state;
        }
        private void HandleWrapperSelected() {
            ElementSelectedEvent?.Invoke(this);
        }
        private void HandleWindowPositionChanged(Vector2 position) {
            ElementPositionChangedEvent?.Invoke(this);
        }

        protected override void OnInitialize() {
            _contentRect = Content.GetComponent<RectTransform>();
            _wrapperWindow = gameObject.AddComponent<GridLayoutWindow>();
            _wrapperWindow.Target = Content.GetComponent<RectTransform>();
            _wrapperWindow.WindowPositionChangedEvent += HandleWindowPositionChanged;

            Wrapper = new GameObject(Name.Replace(" ", "") + "Wrapper").AddComponent<GlassWrapper>();
            Wrapper.container = _wrapperWindow.Handle = WrapperContainer;
            Wrapper.WrapperStateChangedEvent += HandleWrapperStateChanged;
            Wrapper.WrapperSelectedEvent += HandleWrapperSelected;
            HandleWrapperStateChanged(false);
            RefreshWrapper();
        }
    }
}
