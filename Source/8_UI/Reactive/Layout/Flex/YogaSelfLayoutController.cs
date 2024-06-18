namespace BeatLeader.UI.Reactive.Yoga {
    internal sealed class YogaSelfLayoutController : YogaLayoutController {
        private YogaNode _pseudoRootNode;
        private bool _pseudoParentConnected;
        private ILayoutItem? _layoutItem;

        public override void ApplySelf(ILayoutItem item) {
            if (item.LayoutModifier is not YogaModifier mod || item.LayoutDriver != null) {
                DisconnectPseudoRoot();
                return;
            }
            if (!_pseudoRootNode.IsInitialized) {
                _pseudoRootNode.Touch();
                _pseudoRootNode.StyleSetJustifyContent(Justify.Center);
                _pseudoRootNode.StyleSetAlignItems(Align.Auto);
                RecalculateInternal();
            }
            var recalculationRequired = false;
            if (!_pseudoParentConnected) {
                _pseudoRootNode.InsertChild(YogaNode, 0);
                _pseudoParentConnected = true;
                recalculationRequired = true;
            }
            if (_layoutItem != item) {
                recalculationRequired = true;
                _layoutItem = item;
            }
            if (recalculationRequired) CalculatePseudoRoot(mod);
            item.ApplyTransforms(x => YogaNode.ApplySizeTo(x));
        }

        protected override void RecalculateInternal() {
            if (!_pseudoRootNode.IsInitialized) {
                base.RecalculateInternal();
            } else if (_layoutItem is { LayoutModifier: YogaModifier mod }) {
                CalculatePseudoRoot(mod);
            }
        }

        protected override void ReloadChildrenInternal(YogaNode node, YogaNode? fromNode) {
            DisconnectPseudoRoot();
            base.ReloadChildrenInternal(node, fromNode);
        }

        private void CalculatePseudoRoot(YogaModifier yogaModifier) {
            var applyX = yogaModifier.Size.x != YogaValue.Undefined ||
                yogaModifier.MinSize.x != YogaValue.Undefined ||
                yogaModifier.MaxSize.x != YogaValue.Undefined;
            var applyY = yogaModifier.Size.y != YogaValue.Undefined ||
                yogaModifier.MinSize.y != YogaValue.Undefined ||
                yogaModifier.MaxSize.y != YogaValue.Undefined;
            //making dynamic axis the main one to stretch against the opposite axis if needed
            _pseudoRootNode.StyleSetFlexDirection(applyX ? FlexDirection.Row : FlexDirection.Column);
            _pseudoRootNode.StyleSetAlignItems(!applyX || !applyY ? Align.Stretch : Align.Auto);
            //calculating
            _pseudoRootNode.CalculateLayout(
                applyX ? float.MaxValue : Rect.width,
                applyY ? float.MaxValue : Rect.height,
                Direction.Inherit
            );
        }

        private void DisconnectPseudoRoot() {
            if (!_pseudoRootNode.IsInitialized || !_pseudoParentConnected) return;
            _pseudoRootNode.RemoveAllChildren();
            _pseudoParentConnected = false;
        }
    }
}