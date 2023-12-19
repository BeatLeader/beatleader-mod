using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal enum Axis {
        X = 0,
        Y = 1
    }

    internal enum FlexDirection {
        Row = 0,
        Column,
        RowReverse,
        ColumnReverse
    }

    internal enum JustifyContent {
        FlexStart = 0,
        FlexEnd,
        Center,
        SpaceBetween,
        SpaceAround,
        SpaceEvenly
    }

    internal enum AlignItems {
        Center = 0,
        FlexStart,
        FlexEnd,
        Stretch
    }

    //forked and modified from https://github.com/JiangJie/flexbox4unity/blob/main/src/FlexContainer.cs
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    internal sealed class FlexContainer : UIBehaviour, ICanvasElement, ILayoutElement {
        #region Serialized Fields

        [SerializeField]
        private FlexDirection flexDirection;
        
        [SerializeField]
        private JustifyContent justifyContent;

        [SerializeField]
        private AlignItems alignItems;
        
        [SerializeField]
        private Vector2 gap;
        
        [SerializeField]
        private bool autoWidth;
        
        [SerializeField]
        private bool autoHeight;

        #endregion

        #region UI Properties

        public FlexDirection FlexDirection {
            get => flexDirection;
            set {
                flexDirection = value;
                SetDirty();
            }
        }

        public JustifyContent JustifyContent {
            get => justifyContent;
            set {
                justifyContent = value;
                SetDirty();
            }
        }

        public AlignItems AlignItems {
            get => alignItems;
            set {
                alignItems = value;
                SetDirty();
            }
        }

        public Vector2 Gap {
            get => gap;
            set {
                gap = value;
                SetDirty();
            }
        }

        public bool AutoWidth {
            get => autoWidth;
            set {
                autoWidth = value;
                SetDirty();
            }
        }

        public bool AutoHeight {
            get => autoHeight;
            set {
                autoHeight = value;
                SetDirty();
            }
        }

        #endregion
        
        #region ILayoutElement

        float ILayoutElement.preferredWidth {
            get {
                if (autoWidth) CalculateLayout();
                return ContainerRect.rect.size.x;
            }
        }

        float ILayoutElement.preferredHeight {
            get {
                if (autoHeight) CalculateLayout();
                return ContainerRect.rect.size.y;
            }
        }

        float ILayoutElement.minWidth => 0;

        float ILayoutElement.flexibleWidth => 0;

        float ILayoutElement.minHeight => 0;

        float ILayoutElement.flexibleHeight => 0;

        int ILayoutElement.layoutPriority => 0;

        #endregion

        #region ICanvasElement

        void ICanvasElement.Rebuild(CanvasUpdate executing) {
            switch (executing) {
                case CanvasUpdate.PostLayout:
                    if (_isDirty) CalculateLayout();
                    break;
            }
        }

        void ICanvasElement.LayoutComplete() {
            _isDirty = false;
        }

        void ICanvasElement.GraphicUpdateComplete() { }

        void ILayoutElement.CalculateLayoutInputHorizontal() { }

        void ILayoutElement.CalculateLayoutInputVertical() { }

        #endregion

        #region Calculation

        private RectTransform ContainerRect {
            get {
                if (_rect == null) {
                    _rect = GetComponent<RectTransform>();
                }
                return _rect;
            }
        }

        private readonly List<(RectTransform rect, FlexItem flex)> _children = new();
        private bool _isDirty;

        private DrivenRectTransformTracker _tracker;
        private RectTransform? _rect;
        
        private readonly List<int> _calculationBuffer = new();
        private readonly List<int> _growableItemBuffer = new();
        private readonly List<int> _shrinkableItemBuffer = new();
        private readonly List<float> _itemMainSizeBuffer = new();
        private readonly List<float> _itemCrossSizeBuffer = new();

        private void CalculateLayout() {
            RefreshChildren();
            if (_children.Count == 0) return;

            var mainAxis = (int)(flexDirection is FlexDirection.Row or FlexDirection.RowReverse ? Axis.X : Axis.Y);
            var crossAxis = 1 - mainAxis;
            var xIsMainAxis = mainAxis == (int)Axis.X;
            var mainPositionScale = xIsMainAxis ? 1 : -1;

            #region Init

            _calculationBuffer.Clear();
            _growableItemBuffer.Clear();
            _shrinkableItemBuffer.Clear();
            _itemMainSizeBuffer.Clear();
            _itemCrossSizeBuffer.Clear();
            
            var itemMainSizeList = _itemMainSizeBuffer;

            var gapMainSize = gap[mainAxis] * (_children.Count - 1);

            for (var index = 0; index < _children.Count; index++) {
                var item = _children[index];
                var flex = item.flex;
                var rect = item.rect;

                var basicCrossSize = flex.FlexBasis[crossAxis];
                var crossSize = basicCrossSize >= 0 ? basicCrossSize : LayoutUtility.GetPreferredSize(rect, crossAxis);

                if (flex.AlignSelf == AlignSelf.Stretch || (flex.AlignSelf == AlignSelf.Auto && alignItems == AlignItems.Stretch)) {
                    crossSize = ContainerRect.rect.size[crossAxis];
                }
                crossSize = GetActualSizeByAxis(index, crossAxis, crossSize);

                rect.sizeDelta = CreateVector2AccordingMainAxis(rect.rect.size[mainAxis], crossSize);
                _itemCrossSizeBuffer.Add(crossSize);

                var basicMainSize = flex.FlexBasis[mainAxis];
                var mainSize = basicMainSize >= 0 ? basicMainSize : LayoutUtility.GetPreferredSize(rect, mainAxis);
                mainSize = GetActualSizeByAxis(index, mainAxis, mainSize);
                itemMainSizeList.Add(mainSize);

                if (flex.FlexGrow > 0) {
                    _growableItemBuffer.Add(index);
                }
                if (flex.FlexShrink > 0) {
                    _shrinkableItemBuffer.Add(index);
                }
            }

            var restSpace = GetRestSpace();

            #endregion

            #region Item

            if (autoWidth || autoHeight) {
                var autoMain = xIsMainAxis ? autoWidth : autoHeight;
                var autoCross = xIsMainAxis ? autoHeight : autoWidth;

                var x = ContainerRect.rect.size[mainAxis] - (autoMain ? restSpace : 0);
                var y = autoCross ? _itemCrossSizeBuffer.Max() : ContainerRect.rect.size[crossAxis];
                ContainerRect.sizeDelta = CreateVector2AccordingMainAxis(x, y);

                if (autoMain) restSpace = 0;
            }
            
            switch (restSpace) {
                case > 0:
                    var totalGrow = Math.Max(_growableItemBuffer.Sum(i => _children[i].flex.FlexGrow), 1f);
                    while (_growableItemBuffer.Count > 0 && restSpace > 0) {
                        var hasMoreSpace = false;

                        for (var i = 0; i < _growableItemBuffer.Count; i++) {
                            var index = _growableItemBuffer[i];
                            var flex = _children[index].flex;
                            var maxMainSize = flex.MaxSize[mainAxis];

                            var mainSize = itemMainSizeList[index];
                            mainSize += flex.FlexGrow / totalGrow * restSpace;

                            if (maxMainSize >= 0) {
                                if (mainSize >= maxMainSize) {
                                    _calculationBuffer.Add(index);
                                }
                                if (mainSize > maxMainSize) {
                                    mainSize = maxMainSize;
                                    hasMoreSpace = true;
                                }
                            }
                            itemMainSizeList[index] = Math.Max(mainSize, 0f);
                        }

                        _growableItemBuffer.Clear();
                        if (hasMoreSpace) {
                            _growableItemBuffer.AddRange(_growableItemBuffer.Except(_calculationBuffer));
                            restSpace = GetRestSpace();
                        }
                    }
                    break;
                case < 0:
                    var totalShrink = Math.Min(_shrinkableItemBuffer.Sum(i => _children[i].flex.FlexShrink), 1f);
                    var totalShrinkSize = _shrinkableItemBuffer.Aggregate(0f, (ret, index) => ret + (itemMainSizeList[index] * _children[index].flex.FlexShrink));
                    
                    while (_shrinkableItemBuffer.Count > 0 && restSpace < 0) {
                        var needMoreSpace = false;

                        for (var i = 0; i < _shrinkableItemBuffer.Count; i++) {
                            var index = _shrinkableItemBuffer[i];
                            var flex = _children[index].flex;
                            var minMainSize = flex.MinSize[mainAxis];

                            var mainSize = itemMainSizeList[index];
                            if (totalShrinkSize > 0) {
                                mainSize += totalShrink * restSpace * flex.FlexShrink * mainSize / totalShrinkSize;
                            }

                            if (minMainSize >= 0) {
                                if (mainSize <= minMainSize) {
                                    _calculationBuffer.Add(index);
                                }
                                if (mainSize < minMainSize) {
                                    mainSize = minMainSize;
                                    needMoreSpace = true;
                                }
                            }

                            itemMainSizeList[index] = Math.Max(mainSize, 0f);
                        }

                        _shrinkableItemBuffer.Clear();
                        if (needMoreSpace) {
                            _shrinkableItemBuffer.AddRange(_shrinkableItemBuffer.Except(_calculationBuffer));
                            restSpace = GetRestSpace();
                        }
                    }
                    break;
            }

            for (var index = 0; index < itemMainSizeList.Count; index++) {
                var item = _children[index];
                var rect = item.rect;
                rect.sizeDelta = CreateVector2AccordingMainAxis(itemMainSizeList[index], _itemCrossSizeBuffer[index]);
            }

            #endregion

            #region JustifyContent

            var totalMainDelta = 0f;
            var spacing = gap[mainAxis];

            switch (justifyContent) {
                case JustifyContent.FlexStart:
                    break;
                case JustifyContent.FlexEnd:
                    totalMainDelta = GetRestSpace();
                    break;
                case JustifyContent.Center:
                    totalMainDelta = GetRestSpace() / 2;
                    break;
                case JustifyContent.SpaceBetween:
                    spacing += GetRestSpace() / Math.Max(_children.Count - 1, 1);
                    break;
                case JustifyContent.SpaceAround: {
                    var halfSpacing = GetRestSpace() / ((Math.Max(_children.Count - 1, 0) * 2) + 2);
                    totalMainDelta = halfSpacing;
                    spacing += halfSpacing * 2;
                    break;
                }
                case JustifyContent.SpaceEvenly: {
                    var innerSpacing = GetRestSpace() / (_children.Count + 1);
                    totalMainDelta = innerSpacing;
                    spacing += innerSpacing;
                    break;
                }
            }

            var prevMainDelta = totalMainDelta;
            foreach (var item in _children) {
                var rect = item.rect;
                rect.anchoredPosition = CreateVector2AccordingMainAxis((prevMainDelta + (rect.rect.size[mainAxis] / 2)) * mainPositionScale, rect.anchoredPosition[crossAxis]);
                prevMainDelta += rect.rect.size[mainAxis] + spacing;
            }

            #endregion

            #region AlignItems

            foreach (var item in _children) {
                var rect = item.rect;
                var flex = item.flex;

                if (flex.AlignSelf == AlignSelf.FlexStart || (flex.AlignSelf == AlignSelf.Auto && alignItems == AlignItems.FlexStart)) {
                    rect.anchoredPosition = CreateVector2AccordingMainAxis(rect.anchoredPosition[mainAxis], -rect.rect.size[crossAxis] / 2 * mainPositionScale);
                }

                if (flex.AlignSelf == AlignSelf.Center || (flex.AlignSelf == AlignSelf.Auto && alignItems == AlignItems.Center)
                    || flex.AlignSelf == AlignSelf.Stretch || (flex.AlignSelf == AlignSelf.Auto && alignItems == AlignItems.Stretch)) {
                    rect.anchoredPosition = CreateVector2AccordingMainAxis(rect.anchoredPosition[mainAxis], -ContainerRect.rect.size[crossAxis] / 2 * mainPositionScale);
                }

                if (flex.AlignSelf == AlignSelf.FlexEnd || (flex.AlignSelf == AlignSelf.Auto && alignItems == AlignItems.FlexEnd)) {
                    rect.anchoredPosition = CreateVector2AccordingMainAxis(rect.anchoredPosition[mainAxis], ((rect.rect.size[crossAxis] / 2) - ContainerRect.rect.size[crossAxis]) * mainPositionScale);
                }
            }

            #endregion

            return;

            Vector2 CreateVector2AccordingMainAxis(float x, float y) => xIsMainAxis ? new Vector2(x, y) : new Vector2(y, x);

            float GetRestSpace() => ContainerRect.rect.size[mainAxis] - gapMainSize - itemMainSizeList.Sum();
        }

        private void RefreshChildren() {
            _children.Clear();

            for (var i = 0; i < ContainerRect.childCount; i++) {
                if (ContainerRect.GetChild(i) is not RectTransform childRect || !childRect.gameObject.activeSelf) {
                    continue;
                }

                var flex = childRect.GetComponent<FlexItem>();
                if (flex == null) continue;

                childRect.anchorMin = Vector2.up;
                childRect.anchorMax = Vector2.up;

                _children.Add((childRect, flex));
            }

            _children.Sort(static (a, b) => a.flex.Order - b.flex.Order);

            if (flexDirection is FlexDirection.RowReverse or FlexDirection.ColumnReverse) {
                _children.Reverse();
            }
        }

        private float GetActualSizeByAxis(int index, int axis, float size) {
            var flex = _children[index].flex;
            var minSize = flex.MinSize[axis];
            var maxSize = flex.MaxSize[axis];

            if (minSize >= 0) {
                size = Math.Max(size, minSize);
            }
            if (maxSize >= 0) {
                size = Math.Min(size, maxSize);
            }

            return size;
        }

        public void SetDirty() {
            if (!IsActive()) return;
            CanvasUpdateRegistry.TryRegisterCanvasElementForLayoutRebuild(this);
            _isDirty = true;
        }

        private void MarkDrivenSize() {
            _tracker.Clear();

            var prop = DrivenTransformProperties.None;
            if (autoWidth) {
                prop |= DrivenTransformProperties.SizeDeltaX;
            }
            if (autoHeight) {
                prop |= DrivenTransformProperties.SizeDeltaY;
            }

            if (prop != DrivenTransformProperties.None) {
                _tracker.Add(this, transform as RectTransform, prop);
            }
        }

        #endregion

        #region Unity Events

        protected override void OnEnable() {
            base.OnEnable();
            MarkDrivenSize();
            SetDirty();
        }

        protected override void OnDisable() {
            _tracker.Clear();
            base.OnDisable();
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);
            _isDirty = false;
        }

        protected override void OnDidApplyAnimationProperties() {
            base.OnDidApplyAnimationProperties();
            MarkDrivenSize();
            SetDirty();
        }

        protected override void OnRectTransformDimensionsChange() {
            base.OnRectTransformDimensionsChange();
            MarkDrivenSize();
            SetDirty();
        }

        #endregion
    }
}