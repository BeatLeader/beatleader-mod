using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal abstract class ComponentLayoutBase<T> : ReeUIComponentV3<T> where T : ReeUIComponentV3<T> {
        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public float Size {
            set {
                Width = value;
                Height = value;
            }
        }

        [ExternalProperty, UsedImplicitly]
        public float Width {
            get => _layoutElement.preferredWidth;
            set => _layoutElement.preferredWidth = value;
        }

        [ExternalProperty, UsedImplicitly]
        public float Height {
            get => _layoutElement.preferredHeight;
            set => _layoutElement.preferredHeight = value;
        }
        
        [ExternalProperty, UsedImplicitly]
        public bool InheritSize {
            set {
                InheritWidth = value;
                InheritHeight = value;
            }
        }

        [ExternalProperty, UsedImplicitly]
        public bool InheritWidth {
            get => _sizeFitter.horizontalFit is ContentSizeFitter.FitMode.Unconstrained;
            set => _sizeFitter.horizontalFit = value ? ContentSizeFitter.FitMode.Unconstrained : ContentSizeFitter.FitMode.PreferredSize;
        }

        [ExternalProperty, UsedImplicitly]
        public bool InheritHeight {
            get => _sizeFitter.verticalFit is ContentSizeFitter.FitMode.Unconstrained;
            set => _sizeFitter.verticalFit = value ? ContentSizeFitter.FitMode.Unconstrained : ContentSizeFitter.FitMode.PreferredSize;
        }

        [ExternalProperty, UsedImplicitly]
        public RectOffset Pad {
            get => _layoutGroup.padding;
            set => _layoutGroup.padding = value;
        }

        #endregion

        #region UI Components

        [UIComponent("root"), UsedImplicitly]
        protected LayoutElement _layoutElement = null!;

        [UIComponent("root"), UsedImplicitly]
        protected VerticalLayoutGroup _layoutGroup = null!;

        [UIComponent("root"), UsedImplicitly]
        protected ContentSizeFitter _sizeFitter = null!;

        #endregion

        #region Markup

        protected override string Markup => "<vertical id=\"root\" vertical-fit=\"PreferredSize\"/>";

        #endregion
    }
}