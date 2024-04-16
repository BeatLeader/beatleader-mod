using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Yoga {
    internal struct YogaNode : IDisposable, IEquatable<YogaNode> {
        private IntPtr _nodePtr;

        public void Touch() {
            _nodePtr = YogaNative.YGNodeNew();
        }
        
        public void Dispose() {
            if (_nodePtr == IntPtr.Zero) return;
            YogaNative.YGNodeFree(_nodePtr);
            _nodePtr = IntPtr.Zero;
        }

        public void ApplyTo(RectTransform rectTransform) {
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, LayoutGetTop(), LayoutGetHeight());
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, LayoutGetLeft(), LayoutGetWidth());
        }

        public bool Equals(YogaNode other) {
            return _nodePtr == other._nodePtr;
        }

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

        #region YGNode

        public void CalculateLayout(float availableWidth, float availableHeight, Direction ownerDirection) {
            YogaNative.YGNodeCalculateLayout(_nodePtr, availableWidth, availableHeight, ownerDirection);
        }

        public void InsertChild(YogaNode child, int index) {
            YogaNative.YGNodeInsertChildSafe(_nodePtr, child._nodePtr, index);
        }

        public void RemoveChild(YogaNode child) {
            YogaNative.YGNodeRemoveChildSafe(_nodePtr, child._nodePtr);
        }

        public void RemoveAllChildren() {
            YogaNative.YGNodeRemoveAllChildrenSafe(_nodePtr);
        }
        
        #endregion

        #region Layout

        public float LayoutGetLeft() {
            return YogaNative.YGNodeLayoutGetLeft(_nodePtr);
        }

        public float LayoutGetTop() {
            return YogaNative.YGNodeLayoutGetTop(_nodePtr);
        }

        public float LayoutGetWidth() {
            return YogaNative.YGNodeLayoutGetWidth(_nodePtr);
        }

        public float LayoutGetHeight() {
            return YogaNative.YGNodeLayoutGetHeight(_nodePtr);
        }

        #endregion

        #region Style

        public void StyleSetDirection(Direction direction) {
            YogaNative.YGNodeStyleSetDirection(_nodePtr, direction);
        }

        public void StyleSetFlexDirection(FlexDirection flexDirection) {
            YogaNative.YGNodeStyleSetFlexDirection(_nodePtr, flexDirection);
        }

        public void StyleSetJustifyContent(Justify justifyContent) {
            YogaNative.YGNodeStyleSetJustifyContent(_nodePtr, justifyContent);
        }

        public void StyleSetAlignContent(Align alignContent) {
            YogaNative.YGNodeStyleSetAlignContent(_nodePtr, alignContent);
        }

        public void StyleSetAlignItems(Align alignItems) {
            YogaNative.YGNodeStyleSetAlignItems(_nodePtr, alignItems);
        }

        public void StyleSetAlignSelf(Align alignSelf) {
            YogaNative.YGNodeStyleSetAlignSelf(_nodePtr, alignSelf);
        }

        public void StyleSetPositionType(PositionType positionType) {
            YogaNative.YGNodeStyleSetPositionType(_nodePtr, positionType);
        }

        public void StyleSetFlexWrap(Wrap flexWrap) {
            YogaNative.YGNodeStyleSetFlexWrap(_nodePtr, flexWrap);
        }

        public void StyleSetFlexGrow(float flexGrow) {
            YogaNative.YGNodeStyleSetFlexGrow(_nodePtr, flexGrow);
        }

        public void StyleSetFlexShrink(float flexShrink) {
            YogaNative.YGNodeStyleSetFlexShrink(_nodePtr, flexShrink);
        }

        public void StyleSetFlexBasis(float flexBasis) {
            YogaNative.YGNodeStyleSetFlexBasis(_nodePtr, flexBasis);
        }

        public void StyleSetFlexBasisPercent(float flexBasis) {
            YogaNative.YGNodeStyleSetFlexBasisPercent(_nodePtr, flexBasis);
        }

        public void StyleSetFlexBasisAuto() {
            YogaNative.YGNodeStyleSetFlexBasisAuto(_nodePtr);
        }

        public void StyleSetPosition(Edge edge, float position) {
            YogaNative.YGNodeStyleSetPosition(_nodePtr, edge, position);
        }

        public void StyleSetPositionPercent(Edge edge, float position) {
            YogaNative.YGNodeStyleSetPositionPercent(_nodePtr, edge, position);
        }

        public void StyleSetMargin(Edge edge, float margin) {
            YogaNative.YGNodeStyleSetMargin(_nodePtr, edge, margin);
        }

        public void StyleSetMarginPercent(Edge edge, float margin) {
            YogaNative.YGNodeStyleSetMarginPercent(_nodePtr, edge, margin);
        }

        public void StyleSetMarginAuto(Edge edge) {
            YogaNative.YGNodeStyleSetMarginAuto(_nodePtr, edge);
        }

        public void StyleSetPadding(Edge edge, float padding) {
            YogaNative.YGNodeStyleSetPadding(_nodePtr, edge, padding);
        }

        public void StyleSetPaddingPercent(Edge edge, float padding) {
            YogaNative.YGNodeStyleSetPaddingPercent(_nodePtr, edge, padding);
        }

        public void StyleSetWidth(float width) {
            YogaNative.YGNodeStyleSetWidth(_nodePtr, width);
        }

        public void StyleSetWidthPercent(float width) {
            YogaNative.YGNodeStyleSetWidthPercent(_nodePtr, width);
        }

        public void StyleSetWidthAuto() {
            YogaNative.YGNodeStyleSetWidthAuto(_nodePtr);
        }

        public void StyleSetHeight(float height) {
            YogaNative.YGNodeStyleSetHeight(_nodePtr, height);
        }

        public void StyleSetHeightPercent(float height) {
            YogaNative.YGNodeStyleSetHeightPercent(_nodePtr, height);
        }

        public void StyleSetHeightAuto() {
            YogaNative.YGNodeStyleSetHeightAuto(_nodePtr);
        }

        public void StyleSetMinWidth(float minWidth) {
            YogaNative.YGNodeStyleSetMinWidth(_nodePtr, minWidth);
        }

        public void StyleSetMinWidthPercent(float minWidth) {
            YogaNative.YGNodeStyleSetMinWidthPercent(_nodePtr, minWidth);
        }

        public void StyleSetMinHeight(float minHeight) {
            YogaNative.YGNodeStyleSetMinHeight(_nodePtr, minHeight);
        }

        public void StyleSetMinHeightPercent(float minHeight) {
            YogaNative.YGNodeStyleSetMinHeightPercent(_nodePtr, minHeight);
        }

        public void StyleSetMaxWidth(float maxWidth) {
            YogaNative.YGNodeStyleSetMaxWidth(_nodePtr, maxWidth);
        }

        public void StyleSetMaxWidthPercent(float maxWidth) {
            YogaNative.YGNodeStyleSetMaxWidthPercent(_nodePtr, maxWidth);
        }

        public void StyleSetMaxHeight(float maxHeight) {
            YogaNative.YGNodeStyleSetMaxHeight(_nodePtr, maxHeight);
        }

        public void StyleSetMaxHeightPercent(float maxHeight) {
            YogaNative.YGNodeStyleSetMaxHeightPercent(_nodePtr, maxHeight);
        }

        public void StyleSetAspectRatio(float aspectRatio) {
            YogaNative.YGNodeStyleSetAspectRatio(_nodePtr, aspectRatio);
        }

        #endregion
    }
}