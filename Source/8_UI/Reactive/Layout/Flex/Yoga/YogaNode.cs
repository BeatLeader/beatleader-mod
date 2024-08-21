using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Yoga {
    internal struct YogaNode : IEquatable<YogaNode> {
        #region Create & Dispose

        public bool IsInitialized { get; private set; }

        private IntPtr NodePtr {
            get => _nodePtr == IntPtr.Zero ?
                throw new UnassignedReferenceException("YogaNode was not initialized") :
                _nodePtr;
        }

        private IntPtr _nodePtr;

        public void Touch() {
            if (IsInitialized) return;
            _nodePtr = YogaNative.YGNodeNew();
            IsInitialized = true;
        }

        public void Free() {
            if (NodePtr == IntPtr.Zero) return;
            YogaNative.YGNodeFree(NodePtr);
            _nodePtr = IntPtr.Zero;
            IsInitialized = false;
        }

        #endregion

        #region Impl

        public bool Equals(YogaNode other) {
            return NodePtr == other.NodePtr;
        }

        public override string ToString() {
            return $"calc: {{ w: {LayoutGetWidth()}, h: {LayoutGetHeight()} }}, set: {{ w: {StyleGetWidth()}, h: {StyleGetHeight()}, dsp: {StyleGetDisplay()} }}";
        }

        #endregion

        #region Automatic Style Setters

        public void StyleSetPosition(Edge edge, YogaValue value) {
            switch (value.unit) {
                case Unit.Undefined:
                    StyleSetPosition(edge, float.NaN);
                    break;

                case Unit.Point:
                    StyleSetPosition(edge, value.value);
                    break;

                case Unit.Percent:
                    StyleSetPositionPercent(edge, value.value);
                    break;

                case Unit.Auto:
                    throw new ArgumentOutOfRangeException(nameof(value), "Auto is not supported for position");
            }
        }

        public void StyleSetWidth(YogaValue value) {
            switch (value.unit) {
                case Unit.Undefined:
                    StyleSetWidth(float.NaN);
                    break;

                case Unit.Point:
                    StyleSetWidth(value.value);
                    break;

                case Unit.Percent:
                    StyleSetWidthPercent(value.value);
                    break;

                case Unit.Auto:
                    StyleSetWidthAuto();
                    break;
            }
        }

        public void StyleSetHeight(YogaValue value) {
            switch (value.unit) {
                case Unit.Undefined:
                    StyleSetHeight(float.NaN);
                    break;

                case Unit.Point:
                    StyleSetHeight(value.value);
                    break;

                case Unit.Percent:
                    StyleSetHeightPercent(value.value);
                    break;

                case Unit.Auto:
                    StyleSetHeightAuto();
                    break;
            }
        }

        public void StyleSetMinWidth(YogaValue value) {
            switch (value.unit) {
                case Unit.Undefined:
                    StyleSetMinWidth(float.NaN);
                    break;

                case Unit.Point:
                    StyleSetMinWidth(value.value);
                    break;

                case Unit.Percent:
                    StyleSetMinWidthPercent(value.value);
                    break;

                case Unit.Auto:
                    throw new ArgumentOutOfRangeException(nameof(value), "Auto is not supported for min width");
            }
        }

        public void StyleSetMaxWidth(YogaValue value) {
            switch (value.unit) {
                case Unit.Undefined:
                    StyleSetMaxWidth(float.NaN);
                    break;

                case Unit.Point:
                    StyleSetMaxWidth(value.value);
                    break;

                case Unit.Percent:
                    StyleSetMaxWidthPercent(value.value);
                    break;

                case Unit.Auto:
                    throw new ArgumentOutOfRangeException(nameof(value), "Auto is not supported for max width");
            }
        }

        public void StyleSetMinHeight(YogaValue value) {
            switch (value.unit) {
                case Unit.Undefined:
                    StyleSetMinHeight(float.NaN);
                    break;

                case Unit.Point:
                    StyleSetMinHeight(value.value);
                    break;

                case Unit.Percent:
                    StyleSetMinHeightPercent(value.value);
                    break;

                case Unit.Auto:
                    throw new ArgumentOutOfRangeException(nameof(value), "Auto is not supported for min height");
            }
        }

        public void StyleSetMaxHeight(YogaValue value) {
            switch (value.unit) {
                case Unit.Undefined:
                    StyleSetMaxHeight(float.NaN);
                    break;

                case Unit.Point:
                    StyleSetMaxHeight(value.value);
                    break;

                case Unit.Percent:
                    StyleSetMaxHeightPercent(value.value);
                    break;

                case Unit.Auto:
                    throw new ArgumentOutOfRangeException(nameof(value), "Auto is not supported for max height");
            }
        }

        public void StyleSetFlexBasis(YogaValue value) {
            switch (value.unit) {
                case Unit.Undefined:
                    StyleSetFlexBasis(float.NaN);
                    break;

                case Unit.Point:
                    StyleSetFlexBasis(value.value);
                    break;

                case Unit.Percent:
                    StyleSetFlexBasisPercent(value.value);
                    break;

                case Unit.Auto:
                    StyleSetFlexBasisAuto();
                    break;
            }
        }

        public void StyleSetMargin(Edge edge, YogaValue value) {
            switch (value.unit) {
                case Unit.Undefined:
                    StyleSetMargin(edge, float.NaN);
                    break;

                case Unit.Point:
                    StyleSetMargin(edge, value.value);
                    break;

                case Unit.Percent:
                    StyleSetMarginPercent(edge, value.value);
                    break;

                case Unit.Auto:
                    StyleSetMarginAuto(edge);
                    break;
            }
        }

        public void StyleSetPadding(Edge edge, YogaValue value) {
            switch (value.unit) {
                case Unit.Undefined:
                    StyleSetPadding(edge, float.NaN);
                    break;

                case Unit.Point:
                    StyleSetPadding(edge, value.value);
                    break;

                case Unit.Percent:
                    StyleSetPaddingPercent(edge, value.value);
                    break;

                case Unit.Auto:
                    throw new ArgumentOutOfRangeException(nameof(value), "Auto is not supported for padding");
            }
        }

        public void StyleSetGap(Gutter gutter, YogaValue value) {
            switch (value.unit) {
                case Unit.Undefined:
                    StyleSetGap(gutter, float.NaN);
                    break;

                case Unit.Point:
                    StyleSetGap(gutter, value.value);
                    break;

                case Unit.Percent:
                    StyleSetGapPercent(gutter, value.value);
                    break;

                case Unit.Auto:
                    throw new ArgumentOutOfRangeException(nameof(value), "Auto is not supported for gap");
            }
        }

        #endregion

        #region YGNode

        public void CalculateLayout(float availableWidth, float availableHeight, Direction ownerDirection) {
            YogaNative.YGNodeCalculateLayout(NodePtr, availableWidth, availableHeight, ownerDirection);
        }

        public void InsertChild(YogaNode child, int index) {
            YogaNative.YGNodeInsertChildSafe(NodePtr, child.NodePtr, index);
        }

        public void RemoveChild(YogaNode child) {
            YogaNative.YGNodeRemoveChildSafe(NodePtr, child.NodePtr);
        }

        public void RemoveAllChildren() {
            YogaNative.YGNodeRemoveAllChildrenSafe(NodePtr);
        }

        #endregion

        #region Layout

        public float LayoutGetLeft() {
            return YogaNative.YGNodeLayoutGetLeft(NodePtr);
        }

        public float LayoutGetTop() {
            return YogaNative.YGNodeLayoutGetTop(NodePtr);
        }

        public float LayoutGetWidth() {
            return YogaNative.YGNodeLayoutGetWidth(NodePtr);
        }

        public float LayoutGetHeight() {
            return YogaNative.YGNodeLayoutGetHeight(NodePtr);
        }

        public void ApplyTo(RectTransform rectTransform) {
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, LayoutGetTop(), LayoutGetHeight());
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, LayoutGetLeft(), LayoutGetWidth());
        }

        public void ApplySizeTo(RectTransform rectTransform) {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, LayoutGetWidth());
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, LayoutGetHeight());
        }

        #endregion

        #region Style

        public YogaValue StyleGetHeight() {
            return YogaNative.YGNodeStyleGetHeight(NodePtr);
        }

        public YogaValue StyleGetWidth() {
            return YogaNative.YGNodeStyleGetWidth(NodePtr);
        }

        public void StyleSetOverflow(Overflow overflow) {
            YogaNative.YGNodeStyleSetOverflow(NodePtr, overflow);
        }

        public void StyleSetDirection(Direction direction) {
            YogaNative.YGNodeStyleSetDirection(NodePtr, direction);
        }

        public void StyleSetFlexDirection(FlexDirection flexDirection) {
            YogaNative.YGNodeStyleSetFlexDirection(NodePtr, flexDirection);
        }

        public void StyleSetJustifyContent(Justify justifyContent) {
            YogaNative.YGNodeStyleSetJustifyContent(NodePtr, justifyContent);
        }

        public void StyleSetAlignContent(Align alignContent) {
            YogaNative.YGNodeStyleSetAlignContent(NodePtr, alignContent);
        }

        public void StyleSetAlignItems(Align alignItems) {
            YogaNative.YGNodeStyleSetAlignItems(NodePtr, alignItems);
        }

        public void StyleSetAlignSelf(Align alignSelf) {
            YogaNative.YGNodeStyleSetAlignSelf(NodePtr, alignSelf);
        }

        public void StyleSetPositionType(PositionType positionType) {
            YogaNative.YGNodeStyleSetPositionType(NodePtr, positionType);
        }

        public void StyleSetFlexWrap(Wrap flexWrap) {
            YogaNative.YGNodeStyleSetFlexWrap(NodePtr, flexWrap);
        }

        public void StyleSetFlexGrow(float flexGrow) {
            YogaNative.YGNodeStyleSetFlexGrow(NodePtr, flexGrow);
        }

        public void StyleSetFlexShrink(float flexShrink) {
            YogaNative.YGNodeStyleSetFlexShrink(NodePtr, flexShrink);
        }

        public void StyleSetFlexBasis(float flexBasis) {
            YogaNative.YGNodeStyleSetFlexBasis(NodePtr, flexBasis);
        }

        public void StyleSetFlexBasisPercent(float flexBasis) {
            YogaNative.YGNodeStyleSetFlexBasisPercent(NodePtr, flexBasis);
        }

        public void StyleSetFlexBasisAuto() {
            YogaNative.YGNodeStyleSetFlexBasisAuto(NodePtr);
        }

        public void StyleSetPosition(Edge edge, float position) {
            YogaNative.YGNodeStyleSetPosition(NodePtr, edge, position);
        }

        public void StyleSetPositionPercent(Edge edge, float position) {
            YogaNative.YGNodeStyleSetPositionPercent(NodePtr, edge, position);
        }

        public void StyleSetMargin(Edge edge, float margin) {
            YogaNative.YGNodeStyleSetMargin(NodePtr, edge, margin);
        }

        public void StyleSetMarginPercent(Edge edge, float margin) {
            YogaNative.YGNodeStyleSetMarginPercent(NodePtr, edge, margin);
        }

        public void StyleSetMarginAuto(Edge edge) {
            YogaNative.YGNodeStyleSetMarginAuto(NodePtr, edge);
        }

        public void StyleSetPadding(Edge edge, float padding) {
            YogaNative.YGNodeStyleSetPadding(NodePtr, edge, padding);
        }

        public void StyleSetPaddingPercent(Edge edge, float padding) {
            YogaNative.YGNodeStyleSetPaddingPercent(NodePtr, edge, padding);
        }

        public void StyleSetWidth(float width) {
            YogaNative.YGNodeStyleSetWidth(NodePtr, width);
        }

        public void StyleSetWidthPercent(float width) {
            YogaNative.YGNodeStyleSetWidthPercent(NodePtr, width);
        }

        public void StyleSetWidthAuto() {
            YogaNative.YGNodeStyleSetWidthAuto(NodePtr);
        }

        public void StyleSetHeight(float height) {
            YogaNative.YGNodeStyleSetHeight(NodePtr, height);
        }

        public void StyleSetHeightPercent(float height) {
            YogaNative.YGNodeStyleSetHeightPercent(NodePtr, height);
        }

        public void StyleSetHeightAuto() {
            YogaNative.YGNodeStyleSetHeightAuto(NodePtr);
        }

        public void StyleSetMinWidth(float minWidth) {
            YogaNative.YGNodeStyleSetMinWidth(NodePtr, minWidth);
        }

        public void StyleSetMinWidthPercent(float minWidth) {
            YogaNative.YGNodeStyleSetMinWidthPercent(NodePtr, minWidth);
        }

        public void StyleSetMinHeight(float minHeight) {
            YogaNative.YGNodeStyleSetMinHeight(NodePtr, minHeight);
        }

        public void StyleSetMinHeightPercent(float minHeight) {
            YogaNative.YGNodeStyleSetMinHeightPercent(NodePtr, minHeight);
        }

        public void StyleSetMaxWidth(float maxWidth) {
            YogaNative.YGNodeStyleSetMaxWidth(NodePtr, maxWidth);
        }

        public void StyleSetMaxWidthPercent(float maxWidth) {
            YogaNative.YGNodeStyleSetMaxWidthPercent(NodePtr, maxWidth);
        }

        public void StyleSetMaxHeight(float maxHeight) {
            YogaNative.YGNodeStyleSetMaxHeight(NodePtr, maxHeight);
        }

        public void StyleSetMaxHeightPercent(float maxHeight) {
            YogaNative.YGNodeStyleSetMaxHeightPercent(NodePtr, maxHeight);
        }

        public void StyleSetGap(Gutter gutter, float gap) {
            YogaNative.YGNodeStyleSetGap(NodePtr, gutter, gap);
        }

        public void StyleSetGapPercent(Gutter gutter, float gap) {
            YogaNative.YGNodeStyleSetGapPercent(NodePtr, gutter, gap);
        }

        public void StyleSetAspectRatio(float aspectRatio) {
            YogaNative.YGNodeStyleSetAspectRatio(NodePtr, aspectRatio);
        }

        public void StyleSetDisplay(Display display) {
            YogaNative.YGNodeStyleSetDisplay(NodePtr, display);
        }

        public Display StyleGetDisplay() {
            return YogaNative.YGNodeStyleGetDisplay(NodePtr);
        }

        #endregion
    }
}