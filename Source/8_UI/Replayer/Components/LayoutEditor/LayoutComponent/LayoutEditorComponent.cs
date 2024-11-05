using BeatLeader.Utils;
using Reactive;
using UnityEngine;

namespace BeatLeader.Components {
    /// <summary>
    /// Reactive component base for LayoutEditor.
    /// </summary>
    internal abstract class LayoutEditorComponent : ReactiveComponent, ILayoutComponent, ILayoutComponentWrapperHandler {
        #region Setup

        private LayoutComponentWrapper _wrapper = null!;
        private RectTransform _componentTransform = null!;
        private ILayoutComponentHandler? _handler;

        public void Setup(ILayoutComponentHandler? editor) {
            _handler = editor;
            ContentTransform.SetParent(editor?.AreaTransform);
        }

        #endregion

        #region Abstraction

        public abstract string ComponentName { get; }
        protected virtual Vector2 MinSize { get; } = Vector2.zero;
        protected virtual Vector2 MaxSize { get; } = new(int.MaxValue, int.MaxValue);

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
        }

        protected abstract void ConstructInternal(Transform parent);

        #endregion

        #region LayoutData

        public ref LayoutData LayoutData => ref _layoutData;

        private LayoutData _layoutData;
        private bool _wrapperActive;

        public void ApplyLayoutData(bool notify) {
            // Applying values
            _wrapper.SetComponentActive(_layoutData.visible);
            _componentTransform.sizeDelta = _layoutData.size;
            _componentTransform.localPosition = _layoutData.position;
            _componentTransform.SetSiblingIndex(_layoutData.layer);
            // Applying state immediately only if outside the Edit mode
            if (!_wrapperActive) {
                Content.SetActive(_layoutData.visible);
            }
            if (notify) {
                _handler!.OnLayoutDataUpdate(this);
            }
        }

        public void LoadLayoutData() {
            _layoutData.layer = ContentTransform.GetSiblingIndex();
        }

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
            var pos = _handler!.PointerPosition + _componentPosOffset;
            if (pos == _componentOriginPos) return;

            pos = _handler.OnMove(this, pos);
            _componentTransform.localPosition = pos;
            _layoutData.position = pos;
        }

        private void UpdateScaling() {
            var pointerPos = _handler!.PointerPosition;
            if (pointerPos == _cornerOriginPos) return;
            var delta = pointerPos - _cornerOriginPos;
            Vector2 destSize = default;
            for (var i = 0; i < 2; i++) {
                if (_lastSelectedCorner[i] is 0) {
                    delta[i] *= -1;
                }
                destSize[i] = _originSize[i] + delta[i];
            }
            destSize = destSize.Clamp(MinSize, MaxSize);
            destSize = _handler.OnResize(this, destSize);
            _componentTransform.sizeDelta = destSize;
            _layoutData.size = destSize;
        }

        #endregion

        #region Tools

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

        public void OnEditorModeChanged(LayoutEditorMode mode) {
            _wrapperActive = mode is LayoutEditorMode.Edit;
            _wrapper.SetWrapperActive(_wrapperActive);
            Content.SetActive(
                mode is LayoutEditorMode.ViewAll or LayoutEditorMode.Edit ||
                (mode is LayoutEditorMode.View && _layoutData.visible)
            );
        }

        public void OnSelectedComponentChanged(ILayoutComponent? component) {
            _wrapper.SetWrapperSelected(component == this);
        }

        void ILayoutComponentWrapperHandler.OnComponentPointerClick() {
            _handler!.OnSelect(this);
        }

        void ILayoutComponentWrapperHandler.OnComponentPointerUp() {
            _isMoving = false;
            _layoutData.position = (Vector2)_componentTransform.localPosition;
        }

        void ILayoutComponentWrapperHandler.OnComponentPointerDown() {
            var compPos = (Vector2)_componentTransform.localPosition;
            var pointerPos = _handler!.PointerPosition;
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
            _cornerOriginPos = _handler!.PointerPosition;
            _originSize = _componentTransform.rect.size;
            _isScaling = true;
        }

        #endregion
    }
}