using System.Linq;
using BeatLeader.Components;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class Flexbox : ReactiveComponent {
        #region UI Props

        public FlexDirection FlexDirection {
            get => _flexContainer.FlexDirection;
            set => _flexContainer.FlexDirection = value;
        }
        
        public JustifyContent JustifyContent {
            get => _flexContainer.JustifyContent;
            set => _flexContainer.JustifyContent = value;
        }
        
        public AlignItems AlignItems {
            get => _flexContainer.AlignItems;
            set => _flexContainer.AlignItems = value;
        }
        
        public Vector2 Gap {
            get => _flexContainer.Gap;
            set => _flexContainer.Gap = value;
        }
        
        public bool AutoWidth {
            get => _flexContainer.AutoWidth;
            set => _flexContainer.AutoWidth = value;
        }
        
        public bool AutoHeight {
            get => _flexContainer.AutoHeight;
            set => _flexContainer.AutoHeight = value;
        }
        
        #endregion

        #region Construction
        
        private FlexContainer _flexContainer = null!;

        protected override void Construct(RectTransform rect) {
            _flexContainer = rect.gameObject.AddComponent<FlexContainer>();
        }

        protected override void OnChildModifierUpdate() {
            OnChildrenUpdate();
            _flexContainer.SetDirty();
        }

        protected override void OnChildrenUpdate() {
            _flexContainer.Children = GetChildrenWithModifiers<FlexModifier>()
                .Select(static x => (x.ContentTransform, (IFlexItem)x.Modifier))
                .ToList();
        }

        #endregion
    }
}