using BeatLeader.UI.Reactive.Yoga;

namespace BeatLeader.UI.Reactive {
    internal class YogaModifier : ModifierBase<YogaModifier> {
        public YogaModifier() {
            _node.Touch();
        }

        ~YogaModifier() {
            _node.Dispose();
        }
        
        #region Item Properties

        public PositionType PositionType {
            get => _positionType;
            set {
                _positionType = value;
                _node.StyleSetPositionType(value);
                Refresh();
            }
        }

        public Align AlignSelf {
            get => _alignSelf;
            set {
                _alignSelf = value;
                _node.StyleSetAlignSelf(value);
                Refresh();
            }
        }

        public YogaValue FlexBasis {
            get => _flexBasis;
            set {
                _flexBasis = value;
                _node.StyleSetFlexBasis(value);
                Refresh();
            }
        }

        public float FlexGrow {
            get => _flexGrow;
            set {
                _flexGrow = value;
                _node.StyleSetFlexGrow(value);
                Refresh();
            }
        }

        public float FlexShrink {
            get => _flexShrink;
            set {
                _flexShrink = value;
                _node.StyleSetFlexShrink(value);
                Refresh();
            }
        }

        public YogaFrame Position {
            get => _position;
            set {
                _position = value;
                RefreshPosition();
                Refresh();
            }
        }

        public YogaVector Size {
            get => _size;
            set {
                _size = value;
                RefreshSize();
                Refresh();
            }
        }

        public YogaVector MinSize {
            get => _minSize;
            set {
                _minSize = value;
                _node.StyleSetMinWidth(value.x);
                _node.StyleSetMinHeight(value.y);
                Refresh();
            }
        }

        public YogaVector MaxSize {
            get => _maxSize;
            set {
                _maxSize = value;
                _node.StyleSetMaxWidth(value.x);
                _node.StyleSetMaxHeight(value.y);
                Refresh();
            }
        }

        public YogaValue AspectRatio {
            get => _aspectRatio;
            set {
                _aspectRatio = value;
                _node.StyleSetAspectRatio(value.value);
                Refresh();
            }
        }

        public YogaFrame Margin {
            get => _margin;
            set {
                _margin = value;
                RefreshMargin();
                Refresh();
            }
        }
        
        public YogaNode YogaNode => _node;

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
        private YogaNode _node;

        private void RefreshMargin() {
            _node.StyleSetMargin(Edge.Top, _margin.top);
            _node.StyleSetMargin(Edge.Bottom, _margin.bottom);
            _node.StyleSetMargin(Edge.Left, _margin.left);
            _node.StyleSetMargin(Edge.Right, _margin.right);
        }

        private void RefreshPosition() {
            _node.StyleSetPosition(Edge.Top, _position.top);
            _node.StyleSetPosition(Edge.Bottom, _position.bottom);
            _node.StyleSetPosition(Edge.Left, _position.left);
            _node.StyleSetPosition(Edge.Right, _position.right);
        }

        private void RefreshSize() {
            if (_size.x.unit is Unit.Auto && (LayoutItem?.DesiredWidth.HasValue ?? false)) {
                _node.StyleSetWidth(LayoutItem.DesiredWidth!.Value);
            } else {
                _node.StyleSetWidth(_size.x);
            }
            
            if (_size.y.unit is Unit.Auto && (LayoutItem?.DesiredHeight.HasValue ?? false)) {
                _node.StyleSetHeight(LayoutItem.DesiredHeight!.Value);
            } else {
                _node.StyleSetHeight(_size.y);
            }
        }
        
        #endregion

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
    }
}