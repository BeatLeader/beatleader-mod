using System;
using BeatLeader.UI.Reactive.Yoga;

namespace BeatLeader.UI.Reactive {
    internal class YogaModifier : ModifierBase<YogaModifier> {
        #region Properties

        public PositionType PositionType {
            get => _positionType;
            set {
                _positionType = value;
                if (!HasValidNode) return;
                YogaNode.StyleSetPositionType(value);
                Refresh();
            }
        }

        public Align AlignSelf {
            get => _alignSelf;
            set {
                _alignSelf = value;
                if (!HasValidNode) return;
                YogaNode.StyleSetAlignSelf(value);
                Refresh();
            }
        }

        public YogaValue FlexBasis {
            get => _flexBasis;
            set {
                _flexBasis = value;
                if (!HasValidNode) return;
                YogaNode.StyleSetFlexBasis(value);
                Refresh();
            }
        }

        public float FlexGrow {
            get => _flexGrow;
            set {
                _flexGrow = value;
                if (!HasValidNode) return;
                YogaNode.StyleSetFlexGrow(value);
                Refresh();
            }
        }

        public float FlexShrink {
            get => _flexShrink;
            set {
                _flexShrink = value;
                if (!HasValidNode) return;
                YogaNode.StyleSetFlexShrink(value);
                Refresh();
            }
        }

        public YogaFrame Position {
            get => _position;
            set {
                _position = value;
                if (!HasValidNode) return;
                RefreshPosition();
                Refresh();
            }
        }

        public YogaVector Size {
            get => _size;
            set {
                _size = value;
                if (!HasValidNode) return;
                RefreshSize();
                Refresh();
            }
        }

        public YogaVector MinSize {
            get => _minSize;
            set {
                _minSize = value;
                if (!HasValidNode) return;
                YogaNode.StyleSetMinWidth(value.x);
                YogaNode.StyleSetMinHeight(value.y);
                Refresh();
            }
        }

        public YogaVector MaxSize {
            get => _maxSize;
            set {
                _maxSize = value;
                if (!HasValidNode) return;
                YogaNode.StyleSetMaxWidth(value.x);
                YogaNode.StyleSetMaxHeight(value.y);
                Refresh();
            }
        }

        public YogaValue AspectRatio {
            get => _aspectRatio;
            set {
                _aspectRatio = value;
                if (!HasValidNode) return;
                YogaNode.StyleSetAspectRatio(value.value);
                Refresh();
            }
        }

        public YogaFrame Margin {
            get => _margin;
            set {
                _margin = value;
                if (!HasValidNode) return;
                RefreshMargin();
                Refresh();
            }
        }

        private float _flexGrow = 0;
        private float _flexShrink = 1;
        private Align _alignSelf = Align.Auto;
        private PositionType _positionType = PositionType.Relative;
        private YogaFrame _position = YogaFrame.Undefined;
        private YogaValue _flexBasis = YogaValue.Undefined;
        private YogaVector _size = YogaVector.Undefined;
        private YogaVector _minSize = YogaVector.Undefined;
        private YogaVector _maxSize = YogaVector.Undefined;
        private YogaValue _aspectRatio = YogaValue.Undefined;
        private YogaFrame _margin = YogaFrame.Zero;

        private void RefreshMargin() {
            YogaNode.StyleSetMargin(Edge.Top, _margin.top);
            YogaNode.StyleSetMargin(Edge.Bottom, _margin.bottom);
            YogaNode.StyleSetMargin(Edge.Left, _margin.left);
            YogaNode.StyleSetMargin(Edge.Right, _margin.right);
        }

        private void RefreshPosition() {
            YogaNode.StyleSetPosition(Edge.Top, _position.top);
            YogaNode.StyleSetPosition(Edge.Bottom, _position.bottom);
            YogaNode.StyleSetPosition(Edge.Left, _position.left);
            YogaNode.StyleSetPosition(Edge.Right, _position.right);
        }

        private void RefreshSize() {
            if (_size.x.unit is Unit.Auto && (LayoutItem?.DesiredWidth.HasValue ?? false)) {
                YogaNode.StyleSetWidth(LayoutItem.DesiredWidth!.Value);
            } else {
                YogaNode.StyleSetWidth(_size.x);
            }

            if (_size.y.unit is Unit.Auto && (LayoutItem?.DesiredHeight.HasValue ?? false)) {
                YogaNode.StyleSetHeight(LayoutItem.DesiredHeight!.Value);
            } else {
                YogaNode.StyleSetHeight(_size.y);
            }
        }

        private void RefreshAllProperties() {
            YogaNode.StyleSetPositionType(_positionType);
            YogaNode.StyleSetAlignSelf(_alignSelf);
            YogaNode.StyleSetFlexBasis(_flexBasis);
            YogaNode.StyleSetFlexGrow(_flexGrow);
            YogaNode.StyleSetFlexShrink(_flexShrink);
            YogaNode.StyleSetMinWidth(_minSize.x);
            YogaNode.StyleSetMinHeight(_minSize.y);
            YogaNode.StyleSetMaxWidth(_maxSize.x);
            YogaNode.StyleSetMaxHeight(_maxSize.y);
            YogaNode.StyleSetAspectRatio(_aspectRatio.value);
            RefreshMargin();
            RefreshPosition();
            RefreshSize();
        }

        #endregion

        #region Context

        public override Type ContextType { get; } = typeof(YogaContext);

        public override object CreateContext() => new YogaContext();

        public override void ProvideContext(object context) {
            _node = ((YogaContext)context).YogaNode;
            RefreshAllProperties();
        }

        #endregion

        #region Modifier

        public YogaNode YogaNode {
            get {
                if (!_node.IsInitialized) {
                    throw new Exception("Node was not initialized");
                }
                return _node;
            }
        }

        private bool HasValidNode => _node.IsInitialized;

        private YogaNode _node;

        protected override void OnLayoutItemUpdate() {
            RefreshSize();
        }

        public override void CopyFromSimilar(YogaModifier similar) {
            SuppressRefresh = true;
            PositionType = similar.PositionType;
            AlignSelf = similar.AlignSelf;
            FlexBasis = similar.FlexBasis;
            FlexGrow = similar.FlexGrow;
            FlexShrink = similar.FlexShrink;
            Position = similar.Position;
            Size = similar.Size;
            MinSize = similar.MinSize;
            MaxSize = similar.MinSize;
            AspectRatio = similar.AspectRatio;
            Margin = similar.Margin;
            SuppressRefresh = false;
            Refresh();
        }

        #endregion
    }
}