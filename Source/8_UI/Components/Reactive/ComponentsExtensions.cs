using System.Collections.Generic;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal static class ComponentsExtensions {
        #region ReeWrapper

        public static ReeWrapperV2<TRee> BindRee<TRee>(this ReeWrapperV2<TRee> comp, ref TRee variable)
            where TRee : ReeUIComponentV2 {
            variable = comp.ReeComponent;
            return comp;
        }

        #endregion

        #region LoadingContainer

        public static LoadingContainer InLoadingContainer(this ILayoutItem comp) {
            return new LoadingContainer {
                Component = comp
            };
        }

        #endregion

        #region InputField

        public static BsInputFieldContext WithItemsText(this BsInputFieldContext context, IEnumerable<string> items) {
            var text = string.Join(", ", items);
            context.Text = text.Length > 0 ? text : null;
            return context;
        }

        #endregion

        #region Effects

        public static T WithAlpha<T>(this T comp, IState<float> alphaState, bool invert = false) where T : IReactiveComponent {
            comp.WithNativeComponent(out CanvasGroup group);
            if (invert) {
                alphaState.AddCallbackUnity(group, static (x, y) => x.alpha = 1f - y);
            } else {
                alphaState.AddCallbackUnity(group, static (x, y) => x.alpha = y);
            }

            return comp;
        }

        public static T WithHoverScale<T>(this T comp, float baseScale, float hoverScale) where T : IReactiveComponent, IGraphic {
            var scale = StateUtils.RememberAnimatedFloat(comp, baseScale, 10.fact);

            comp.WithPointerEvents(
                onEnter: _ => {
                    if (comp is IInteractableComponent { Interactable: false }) {
                        return;
                    }
                    scale.TargetValue = hoverScale;
                },
                onLeave: _ => scale.TargetValue = baseScale
            );

            return comp.On(scale, static (x, y) => x.ContentTransform.localScale = Vector3.one * y);
        }

        #endregion
    }
}