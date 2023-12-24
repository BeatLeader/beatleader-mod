using System;
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
            get => LayoutElement.preferredWidth;
            set {
                LayoutElement.preferredWidth = value;
                OnLayoutPropertySet();
            }
        }

        [ExternalProperty, UsedImplicitly]
        public float Height {
            get => LayoutElement.preferredHeight;
            set {
                LayoutElement.preferredHeight = value;
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
            get => SizeFitter.horizontalFit is ContentSizeFitter.FitMode.Unconstrained;
            set {
                SizeFitter.horizontalFit = value ? ContentSizeFitter.FitMode.Unconstrained : ContentSizeFitter.FitMode.PreferredSize;
                OnLayoutPropertySet();
            }
        }

        [ExternalProperty, UsedImplicitly]
        public bool InheritHeight {
            get => SizeFitter.verticalFit is ContentSizeFitter.FitMode.Unconstrained;
            set {
                SizeFitter.verticalFit = value ? ContentSizeFitter.FitMode.Unconstrained : ContentSizeFitter.FitMode.PreferredSize;
                OnLayoutPropertySet();
            }
        }

        [ExternalProperty, UsedImplicitly]
        public bool IgnoreLayout {
            get => LayoutElement.ignoreLayout;
            set => LayoutElement.ignoreLayout = value;
        }

        [ExternalProperty, UsedImplicitly]
        public RectOffset Pad {
            get => LayoutGroup.padding;
            set {
                LayoutGroup.padding = value;
                OnLayoutPropertySet();
            }
        }

        protected virtual void OnLayoutPropertySet() { }

        #endregion

        #region UI Components

        [ExternalComponent, UsedImplicitly]
        private RectTransform RectTransform => ContentTransform;

        protected LayoutElement LayoutElement => _layoutElement;
        protected HorizontalOrVerticalLayoutGroup LayoutGroup => _layoutGroup;
        protected ContentSizeFitter SizeFitter => _sizeFitter;

        private LayoutElement _layoutElement = null!;
        private HorizontalOrVerticalLayoutGroup _layoutGroup = null!;
        private ContentSizeFitter _sizeFitter = null!;

        #endregion

        #region Construction

        protected enum LayoutGroupType {
            Vertical,
            Horizontal
        }

        protected sealed override string Markup => throw new InvalidOperationException("Unable to access markup into the manual component");
        protected virtual LayoutGroupType LayoutGroupDirection => LayoutGroupType.Vertical;

        protected sealed override GameObject Construct(Transform parent) {
            var wrapper = new GameObject("BaseWrapper");
            var wrapperTransform = wrapper.AddComponent<RectTransform>();
            wrapperTransform.SetParent(parent, false);
            OnConstruct(wrapperTransform);
            ProvideLayoutControllers(wrapper, out _layoutElement, out _layoutGroup, out _sizeFitter);
            wrapperTransform.anchorMin = Vector2.zero;
            wrapperTransform.anchorMax = Vector2.one;
            wrapperTransform.sizeDelta = Vector2.zero;
            return wrapper;
        }
        
        protected virtual void OnConstruct(Transform parent) { }

        protected virtual void ProvideLayoutControllers(
            GameObject go,
            out LayoutElement element,
            out HorizontalOrVerticalLayoutGroup group,
            out ContentSizeFitter sizeFitter
        ) {
            element = go.AddComponent<LayoutElement>();
            group = LayoutGroupDirection switch {
                LayoutGroupType.Vertical => go.AddComponent<VerticalLayoutGroup>(),
                LayoutGroupType.Horizontal => go.AddComponent<HorizontalLayoutGroup>(),
                _ => throw new ArgumentOutOfRangeException()
            };
            sizeFitter = go.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        #endregion
    }
}