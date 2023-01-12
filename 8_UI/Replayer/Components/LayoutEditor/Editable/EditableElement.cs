using BeatLeader.Models;
using BeatLeader.Utils;
using System;
using UnityEngine;

namespace BeatLeader.Components {
    internal abstract class EditableElement : ReeUIComponentV2 {
        public static readonly Color IdlingColor = new(0, 1, 1);
        public static readonly Color SelectedColor = new(1, 1, 0);
        public static readonly Color TextColor = Color.black;

        public virtual RectTransform Root => _container;
        public abstract string Name { get; }
        public virtual LayoutMap LayoutMap { get; }

        public LayoutMap TempLayoutMap;
        public int Layer {
            get => Root.GetSiblingIndex();
            set {
                TempLayoutMap.layer = value;
                Root.SetSiblingIndex(value);
                NotifyPropertyChanged(nameof(Layer));
            }
        }
        public bool State {
            get => TempLayoutMap.enabled;
            set {
                TempLayoutMap.enabled = value;
                NotifyPropertyChanged(nameof(State));
            }
        }

        public bool WrapperPseudoState {
            set {
                var alpha = value ? 0.8f : 0.6f;
                _idlingColor.SetAlpha(alpha);
                _selectedColor.SetAlpha(alpha);
                WrapperSelectionState = _selectionState;
                NotifyPropertyChanged(nameof(WrapperPseudoState));
            }
        }
        public bool WrapperSelectionState {
            get => _selectionState;
            set {
                _wrapper.Color = (_selectionState = value) ? _selectedColor : _idlingColor;
                NotifyPropertyChanged(nameof(WrapperSelectionState));
            }
        }
        public bool WrapperState {
            get => _wrapper.IsEnabled;
            set {
                _wrapper.SetEnabled(value);
                NotifyPropertyChanged(nameof(WrapperState));
            }
        }

        public event Action<EditableElement>? ElementWasSelectedEvent;
        public event Action<Vector2>? ElementDraggingEvent;
        public event Action? ElementWasGrabbedEvent;
        public event Action? ElementWasReleasedEvent;

        private RectTransform _container = null!;
        private GlassWrapper _wrapper = null!;
        private LayoutHandle _handle = null!;
        private Color _idlingColor = IdlingColor;
        private Color _selectedColor = SelectedColor;
        private bool _selectionState;

        #region Setup

        protected override void OnInstantiate() {
            _wrapper = new GameObject("Wrapper").AddComponent<GlassWrapper>();
            _wrapper.WrapperWasSelectedEvent += HandleWrapperWasSelected;
            _handle = _wrapper.gameObject.AddComponent<LayoutHandle>();
            _handle.HandleWasGrabbedEvent += HandleHandleWasGrabbedEvent;
            _handle.HandleWasReleasedEvent += HandleHandleWasReleasedEvent;
            _handle.HandleDraggingEvent += HandleHandleDraggingEvent;
        }

        protected override void OnInitialize() {
            _wrapper.container = _container = Content
                .gameObject.GetOrAddComponent<RectTransform>();
            _wrapper.ComponentName = Name;
        }

        protected override void OnDispose() {
            _wrapper?.gameObject.TryDestroy();
        }

        #endregion

        #region Callbacks

        private void HandleWrapperWasSelected() {
            ElementWasSelectedEvent?.Invoke(this);
        }

        private void HandleHandleWasGrabbedEvent() {
            ElementWasGrabbedEvent?.Invoke();
        }

        private void HandleHandleWasReleasedEvent() {
            ElementWasReleasedEvent?.Invoke();
        }

        private void HandleHandleDraggingEvent() {
            var parent = _container.parent.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Input.mousePosition, null, out var pos);
            ElementDraggingEvent?.Invoke(pos);
        }

        #endregion
    }
}
