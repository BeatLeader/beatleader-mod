using System;
using System.Collections.Generic;
using IPA.Utilities;

namespace BeatLeader.UI.Reactive.Yoga {
    internal sealed class YogaLayoutController : ReactiveLayoutController {
        #region Properties

        public Direction Direction {
            get => _direction;
            set {
                _direction = value;
                YogaNode.StyleSetDirection(_direction);
                Refresh();
            }
        }

        public FlexDirection FlexDirection {
            get => _flexDirection;
            set {
                _flexDirection = value;
                YogaNode.StyleSetFlexDirection(_flexDirection);
                Refresh();
            }
        }

        public Wrap FlexWrap {
            get => _flexWrap;
            set {
                _flexWrap = value;
                YogaNode.StyleSetFlexWrap(_flexWrap);
                Refresh();
            }
        }

        public Justify JustifyContent {
            get => _justifyContent;
            set {
                _justifyContent = value;
                YogaNode.StyleSetJustifyContent(_justifyContent);
                Refresh();
            }
        }

        public Align AlignItems {
            get => _alignItems;
            set {
                _alignItems = value;
                YogaNode.StyleSetAlignItems(_alignItems);
                Refresh();
            }
        }

        public Align AlignContent {
            get => _alignContent;
            set {
                _alignContent = value;
                YogaNode.StyleSetAlignContent(_alignContent);
                Refresh();
            }
        }

        public YogaFrame Padding {
            get => _padding;
            set {
                _padding = value;
                RefreshPadding();
                Refresh();
            }
        }

        private Direction _direction = Direction.Inherit;
        private FlexDirection _flexDirection = FlexDirection.Row;
        private Justify _justifyContent = Justify.FlexStart;
        private Align _alignItems = Align.FlexStart;
        private Align _alignContent = Align.Auto;
        private Wrap _flexWrap = Wrap.Wrap;
        private YogaFrame _padding = YogaFrame.Zero;
        
        private void RefreshPadding() {
            YogaNode.StyleSetPadding(Edge.Top, _padding.top);
            YogaNode.StyleSetPadding(Edge.Bottom, _padding.bottom);
            YogaNode.StyleSetPadding(Edge.Left, _padding.left);
            YogaNode.StyleSetPadding(Edge.Right, _padding.right);
        }

        #endregion

        #region Context

        public override Type ContextType { get; } = typeof(YogaContext);

        public override object CreateContext() => new YogaContext();

        public override void ProvideContext(object context) {
            _node = ((YogaContext)context).YogaNode;
        }

        #endregion

        #region Calculations

        private YogaNode YogaNode {
            get {
                if (!_node.IsInitialized) {
                    throw new Exception("Node was not initialized");
                }
                return _node;
            }
        }

        private readonly Dictionary<ILayoutItem, YogaNode> _nodes = new();
        private YogaNode _node;

        public override void Recalculate() {
            YogaNode.CalculateLayout(Rect.width, Rect.height, _direction);
        }

        public override void Apply() {
            foreach (var (child, node) in _nodes) {
                node.ApplyTo(child.RectTransform);
            }
        }

        public override void ReloadChildren(IEnumerable<ILayoutItem> children) {
            YogaNode.RemoveAllChildren();
            _nodes.Clear();
            var index = 0;
            foreach (var child in children) {
                if (child.LayoutModifier is not YogaModifier node) continue;
                YogaNode.InsertChild(node.YogaNode, index);
                _nodes[child] = node.YogaNode;
                index++;
            }
        }

        #endregion
    }
}