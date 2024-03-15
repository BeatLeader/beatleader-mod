using System.Collections.Generic;
using IPA.Utilities;

namespace BeatLeader.UI.Reactive.Yoga {
    internal sealed class YogaLayoutController : ReactiveLayoutController {
        public YogaLayoutController() {
            _node.Touch();
            Refresh();
            Recalculate();
        }

        ~YogaLayoutController() {
            _node.Dispose();
        }

        #region Properties

        public Direction Direction {
            get => _direction;
            set {
                _direction = value;
                _node.StyleSetDirection(value);
                Recalculate();
            }
        }

        public FlexDirection FlexDirection {
            get => _flexDirection;
            set {
                _flexDirection = value;
                _node.StyleSetFlexDirection(value);
                Recalculate();
            }
        }

        public Wrap FlexWrap {
            get => _flexWrap;
            set {
                _flexWrap = value;
                _node.StyleSetFlexWrap(value);
                Recalculate();
            }
        }

        public Justify JustifyContent {
            get => _justifyContent;
            set {
                _justifyContent = value;
                _node.StyleSetJustifyContent(value);
                Recalculate();
            }
        }

        public Align AlignItems {
            get => _alignItems;
            set {
                _alignItems = value;
                _node.StyleSetAlignItems(value);
                Recalculate();
            }
        }

        public Align AlignContent {
            get => _alignContent;
            set {
                _alignContent = value;
                _node.StyleSetAlignContent(value);
                Recalculate();
            }
        }

        public YogaFrame Padding {
            get => _padding;
            set {
                _padding = value;
                RefreshPadding();
                Recalculate();
            }
        }
        
        private Direction _direction = Direction.Inherit;
        private FlexDirection _flexDirection = FlexDirection.Row;
        private Justify _justifyContent = Justify.FlexStart;
        private Align _alignItems = Align.FlexStart;
        private Align _alignContent = Align.Auto;
        private Wrap _flexWrap = Wrap.Wrap;
        private YogaFrame _padding = YogaFrame.Zero;

        #endregion

        private readonly Dictionary<ILayoutItem, YogaNode> _nodes = new();
        private YogaNode _node;

        public override void Recalculate() {
            if (Item is null) return;
            var rect = Item.RectTransform.rect;
            _node.CalculateLayout(rect.width, rect.height, _direction);
            foreach (var (child, node) in _nodes) {
                node.ApplyTo(child.RectTransform);
            }
        }

        private void Refresh() {
            _node.StyleSetDirection(_direction);
            _node.StyleSetFlexDirection(_flexDirection);
            _node.StyleSetFlexWrap(_flexWrap);
            _node.StyleSetJustifyContent(_justifyContent);
            _node.StyleSetAlignItems(_alignItems);
            _node.StyleSetAlignContent(_alignContent);
            RefreshPadding();
        }
        
        private void RefreshPadding() {
            _node.StyleSetPadding(Edge.Top, _padding.top);
            _node.StyleSetPadding(Edge.Bottom, _padding.bottom);
            _node.StyleSetPadding(Edge.Left, _padding.left);
            _node.StyleSetPadding(Edge.Right, _padding.right);
        }


        protected override void OnChildrenUpdated() {
            if (Item is null) return;
            _node.RemoveAllChildren();
            _nodes.Clear();
            var index = 0;
            foreach (var child in Item.Children) {
                if (child.LayoutModifier is not YogaModifier node) continue;
                _node.InsertChild(node.YogaNode, index);
                _nodes[child] = node.YogaNode;
                index++;
            }
        }
    }
}