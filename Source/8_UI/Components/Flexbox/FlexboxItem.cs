using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class FlexboxItem : ReeUIComponentV3<FlexboxItem> {
        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public float FlexShrink {
            get => _flexItem.FlexShrink;
            set => _flexItem.FlexShrink = value;
        }

        [ExternalProperty, UsedImplicitly]
        public float FlexGrow {
            get => _flexItem.FlexGrow;
            set => _flexItem.FlexGrow = value;
        }

        [ExternalProperty, UsedImplicitly]
        public Vector2 FlexSize {
            get => _flexItem.FlexBasis;
            set => _flexItem.FlexBasis = value;
        }
        
        [ExternalProperty, UsedImplicitly]
        public float FlexWidth {
            get => _flexItem.FlexBasis.x;
            set => _flexItem.FlexBasis = new(value, _flexItem.FlexBasis.y);
        }

        [ExternalProperty, UsedImplicitly]
        public float FlexHeight {
            get => _flexItem.FlexBasis.y;
            set => _flexItem.FlexBasis = new(_flexItem.FlexBasis.x, value);
        }
        
        [ExternalProperty, UsedImplicitly]
        public Vector2 MinSize {
            get => _flexItem.MinSize;
            set => _flexItem.MinSize = value;
        }
        
        [ExternalProperty, UsedImplicitly]
        public float MinWidth {
            get => _flexItem.MinSize.x;
            set => _flexItem.MinSize = new(value, _flexItem.MinSize.y);
        }

        [ExternalProperty, UsedImplicitly]
        public float MinHeight {
            get => _flexItem.MinSize.y;
            set => _flexItem.MinSize = new(_flexItem.MinSize.x, value);
        }

        #endregion

        #region Setup

        [ExternalComponent, UsedImplicitly]
        private FlexItem _flexItem = null!;

        protected override GameObject Construct(Transform parent) {
            var go = new GameObject("FlexItem");
            go.transform.SetParent(parent, false);
            _flexItem = go.AddComponent<FlexItem>();
            return go;
        }

        #endregion
    }
}