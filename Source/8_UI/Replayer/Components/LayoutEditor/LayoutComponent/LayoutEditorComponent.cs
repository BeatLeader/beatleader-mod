using BeatLeader.UI.Reactive;
using BeatLeader.Utils;
using Reactive;
using UnityEngine;

namespace BeatLeader.Components {
    #region Handler

    internal interface ILayoutComponentTransformsHandler {
        Vector2 OnMove(ILayoutComponent component, Vector2 origin, Vector2 destination);

        Vector2 OnResize(ILayoutComponent component, Vector2 origin, Vector2 destination);
    }

    internal interface ILayoutComponentHandler : ILayoutComponentTransformsHandler {
        RectTransform? AreaTransform { get; }
        Vector2 PointerPosition { get; }

        void OnSelect(ILayoutComponent component);
    }

    #endregion

    #region Component

    internal interface ILayoutComponentWrapperController {
        void SetWrapperActive(bool active);

        void SetWrapperSelected(bool selected);
    }

    internal interface ILayoutComponentController {
        Vector2 ComponentSize { get; }
        Vector2 ComponentPosition { get; }
        Vector2 ComponentAnchor { get; }
        bool ComponentActive { get; set; }
        int ComponentLayer { get; set; }
    }

    internal interface ILayoutComponent {
        ILayoutComponentHandler? ComponentHandler { get; }
        ILayoutComponentWrapperController WrapperController { get; }
        ILayoutComponentController ComponentController { get; }
        string ComponentName { get; }

        void Setup(ILayoutComponentHandler? layoutHandler);
        void RequestRefresh();
    }

    #endregion

    /// <summary>
    /// Reactive component base for LayoutEditor
    /// </summary>
    internal abstract class LayoutEditorComponent : ReactiveComponent,
        ILayoutComponent,
        ILayoutComponentWrapperHandler,
        ILayoutComponentController {
        #region Setup

        public ILayoutComponentHandler? ComponentHandler { get; private set; }
        public ILayoutComponentWrapperController WrapperController => _wrapper;

        ILayoutComponentController ILayoutComponent.ComponentController => this;

        public abstract string ComponentName { get; }
        protected virtual Vector2 MinSize { get; } = Vector2.zero;
        protected virtual Vector2 MaxSize { get; } = new(int.MaxValue, int.MaxValue);

        private LayoutComponentWrapper _wrapper = null!;
        private RectTransform _componentTransform = null!;
        private bool _firstActivation = true;

        public void Setup(ILayoutComponentHandler? handler) {
            ComponentHandler = handler;
            ContentTransform.SetParent(handler?.AreaTransform);
        }

        public void RequestRefresh() {
            RefreshTransforms();
        }

        protected sealed override void Construct(RectTransform rect) {
            var container = rect.gameObject;
            _componentTransform = rect;

            var contentContainer = container.CreateChild("Content").AddComponent<RectTransform>();
            contentContainer.sizeDelta = Vector2.zero;
            contentContainer.anchorMin = Vector2.zero;
            contentContainer.anchorMax = Vector2.one;

            //creating content
            ConstructInternal(contentContainer);

            var wrapperGo = container.CreateChild("Wrapper");
            _wrapper = wrapperGo.AddComponent<LayoutComponentWrapper>();
            _wrapper.Setup(this, ComponentName);
            _wrapper.StateChangedEvent += HandleWrapperStateChanged;
        }

        protected abstract void ConstructInternal(Transform parent);

        #endregion

        #region ComponentController

        public Vector2 ComponentSize {
            get => _componentTransform.rect.size;
            private set => _componentTransform.sizeDelta = value;
        }

        public Vector2 ComponentPosition {
            get => _componentTransform.localPosition;
            private set => _componentTransform.localPosition = value;
        }

        public Vector2 ComponentAnchor { get; } = Vector2.zero;

        public int ComponentLayer {
            get => _componentTransform.GetSiblingIndex();
            set => _componentTransform.SetSiblingIndex(value);
        }

        public bool ComponentActive {
            get => _componentActive;
            set {
                if (!_wrapperActive) {
                    Content.SetActive(value);
                }
                _wrapper.SetComponentActive(value);
                _componentActive = value;
            }
        }

        private bool _componentActive;
        private bool _wrapperActive;

        #endregion

        #region Movement & Scaling

        private Vector2 _componentOriginPos;
        private Vector2 _componentPosOffset;
        private bool _isMoving;

        private Vector2 _lastSelectedCorner;
        private Vector2 _cornerOriginPos;
        private Vector2 _originSize;
        private bool _isScaling;

        protected override void OnUpdate() {
            if (_isMoving) UpdateMovement();
            if (_isScaling) UpdateScaling();
        }

        private void UpdateMovement() {
            var componentDestinationPos = ComponentHandler!.PointerPosition + _componentPosOffset;
            if (componentDestinationPos == _componentOriginPos) return;
            ComponentPosition = ComponentHandler.OnMove(this, _componentOriginPos, componentDestinationPos);
        }

        private void UpdateScaling() {
            var pointerPos = ComponentHandler!.PointerPosition;
            if (pointerPos == _cornerOriginPos) return;
            var delta = pointerPos - _cornerOriginPos;
            var destSize = default(Vector2);
            for (var i = 0; i < 2; i++) {
                if (_lastSelectedCorner[i] is 0) delta[i] *= -1;
                destSize[i] = _originSize[i] + delta[i];
            }
            destSize = destSize.Clamp(MinSize, MaxSize);
            destSize = ComponentHandler.OnResize(this, _originSize, destSize);
            ComponentSize = destSize;
        }

        private void RefreshTransforms() {
            ComponentSize = ComponentHandler!.OnResize(this, ComponentSize, ComponentSize);
            ComponentPosition = ComponentHandler.OnMove(this, ComponentPosition, ComponentPosition);
        }

        #endregion

        #region Tools (Move Into Extension)

        private static void SetPivot(RectTransform rectTransform, Vector2 pivot) {
            var size = rectTransform.rect.size;
            var deltaPivot = rectTransform.pivot - pivot;
            var deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
            rectTransform.pivot = pivot;
            rectTransform.localPosition -= deltaPosition;
        }

        private static Vector2 InverseVector01(Vector2 vec) {
            return new(vec.x is 1 ? 0 : 1, vec.y is 1 ? 0 : 1);
        }

        #endregion

        #region Callbacks

        private void HandleWrapperStateChanged(bool state) {
            if (_firstActivation) {
                ComponentSize = ContentTransform.rect.size;
                _firstActivation = false;
            }
            RefreshTransforms();
            _wrapperActive = state;
            Content.SetActive(ComponentActive || state);
        }

        void ILayoutComponentWrapperHandler.OnComponentPointerClick() {
            ComponentHandler!.OnSelect(this);
        }

        void ILayoutComponentWrapperHandler.OnComponentPointerUp() {
            _isMoving = false;
        }

        void ILayoutComponentWrapperHandler.OnComponentPointerDown() {
            var compPos = (Vector2)_componentTransform.localPosition;
            var pointerPos = ComponentHandler!.PointerPosition;
            _componentPosOffset = compPos - pointerPos;
            _componentOriginPos = compPos;
            _isMoving = true;
        }

        void ILayoutComponentWrapperHandler.OnCornerPointerUp(Vector2 corner) {
            _isScaling = false;
            SetPivot(_componentTransform, new Vector2(0.5f, 0.5f));
        }

        void ILayoutComponentWrapperHandler.OnCornerPointerDown(Vector2 corner) {
            _lastSelectedCorner = corner;
            SetPivot(_componentTransform, InverseVector01(_lastSelectedCorner));
            _cornerOriginPos = ComponentHandler!.PointerPosition;
            _originSize = _componentTransform.rect.size;
            _isScaling = true;
        }

        #endregion
    }
}