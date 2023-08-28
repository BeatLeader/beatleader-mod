using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    /// <summary>
    /// Universal ReeUIComponentV3 base for layout components
    /// </summary>
    /// <typeparam name="T">Inherited component</typeparam>
    internal abstract class LayoutComponentBase<T> : ReeUIComponentV3<T> where T : ReeUIComponentV3<T> {
        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public Vector3 Scale {
            get => ContentTransform!.localScale;
            set {
                ContentTransform!.localScale = value;
                OnLayoutPropertySet();
            }
        }

        [ExternalProperty, UsedImplicitly]
        public float Size {
            set {
                Width = value;
                Height = value;
                OnLayoutPropertySet();
            }
        }

        [ExternalProperty, UsedImplicitly]
        public float Width {
            get => _layoutElement.preferredWidth;
            set {
                _layoutElement.preferredWidth = value;
                OnLayoutPropertySet();
            }
        }

        [ExternalProperty, UsedImplicitly]
        public float Height {
            get => _layoutElement.preferredHeight;
            set {
                _layoutElement.preferredHeight = value;
                OnLayoutPropertySet();
            }
        }

        [ExternalProperty, UsedImplicitly]
        public bool InheritSize {
            set {
                InheritWidth = value;
                InheritHeight = value;
                OnLayoutPropertySet();
            }
        }

        [ExternalProperty, UsedImplicitly]
        public bool InheritWidth {
            get => _sizeFitter.horizontalFit is ContentSizeFitter.FitMode.Unconstrained;
            set {
                _sizeFitter.horizontalFit = value ? ContentSizeFitter.FitMode.Unconstrained : ContentSizeFitter.FitMode.PreferredSize;
                OnLayoutPropertySet();
            }
        }

        [ExternalProperty, UsedImplicitly]
        public bool InheritHeight {
            get => _sizeFitter.verticalFit is ContentSizeFitter.FitMode.Unconstrained;
            set {
                _sizeFitter.verticalFit = value ? ContentSizeFitter.FitMode.Unconstrained : ContentSizeFitter.FitMode.PreferredSize;
                OnLayoutPropertySet();
            }
        }

        [ExternalProperty, UsedImplicitly]
        public bool IgnoreLayout {
            get => _layoutElement.ignoreLayout;
            set => _layoutElement.ignoreLayout = value;
        }
        
        [ExternalProperty, UsedImplicitly]
        public RectOffset Pad {
            get => _layoutGroup.padding;
            set {
                _layoutGroup.padding = value;
                OnLayoutPropertySet();
            }
        }

        protected virtual void OnLayoutPropertySet() {}
        
        #endregion

        #region UI Components

        [UIComponent("root"), UsedImplicitly]
        protected LayoutElement _layoutElement = null!;

        [UIComponent("root"), UsedImplicitly]
        protected LayoutGroup _layoutGroup = null!;

        [UIComponent("root"), UsedImplicitly]
        protected ContentSizeFitter _sizeFitter = null!;
        
        [ExternalComponent, UsedImplicitly]
        private RectTransform RectTransform => (RectTransform)ContentTransform!;

        #endregion

        #region Markup

        protected enum LayoutGroupType {
            Vertical,
            Horizontal
        }
        
        protected sealed override string Markup => $"<{LayoutGroupName} id=\"root\" {LayoutGroupName}-fit=\"PreferredSize\">{InnerMarkup}</{LayoutGroupName}>";

        protected virtual string? InnerMarkup => null;
        
        protected virtual LayoutGroupType LayoutGroup => LayoutGroupType.Vertical;

        private string LayoutGroupName => LayoutGroup.ToString().ToLower();

        #endregion
    }
}