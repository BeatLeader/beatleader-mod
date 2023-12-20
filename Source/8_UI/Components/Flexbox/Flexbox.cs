using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class Flexbox : ReeUIComponentV3<Flexbox> {
        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public FlexDirection FlexDirection {
            get => _flexContainer.FlexDirection;
            set => _flexContainer.FlexDirection = value;
        }
        
        [ExternalProperty, UsedImplicitly]
        public JustifyContent JustifyContent {
            get => _flexContainer.JustifyContent;
            set => _flexContainer.JustifyContent = value;
        }

        [ExternalProperty, UsedImplicitly]
        public AlignItems AlignItems {
            get => _flexContainer.AlignItems;
            set => _flexContainer.AlignItems = value;
        }

        [ExternalProperty, UsedImplicitly]
        public Vector2 Gap {
            get => _flexContainer.Gap;
            set => _flexContainer.Gap = value;
        }

        [ExternalProperty, UsedImplicitly]
        public bool AutoWidth {
            get => _flexContainer.AutoWidth;
            set => _flexContainer.AutoWidth = value;
        }

        [ExternalProperty, UsedImplicitly]
        public bool AutoHeight {
            get => _flexContainer.AutoHeight;
            set => _flexContainer.AutoHeight = value;
        }
 
        #endregion
        
        #region Construct

        [ExternalComponent, UsedImplicitly]
        private RectTransform RectTransform => ContentTransform!;
        
        [ExternalComponent, UsedImplicitly]
        private LayoutElement _layoutElement = null!;
        
        [ExternalComponent, UsedImplicitly]
        private FlexContainer _flexContainer = null!;

        protected override GameObject Construct(Transform parent) {
            var go = new GameObject("Flexbox");
            go.transform.SetParent(parent, false);
            _flexContainer = go.AddComponent<FlexContainer>();
            _layoutElement = go.AddComponent<LayoutElement>();
            return go;
        }

        #endregion
    }
}