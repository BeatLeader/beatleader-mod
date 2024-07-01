using System;
using BeatLeader.UI.Components.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class AlphaSelector : LayoutComponentBase<AlphaSelector> {
        #region Events

        [ExternalProperty, UsedImplicitly]
        public event Action<float>? ValueChangedEvent;

        #endregion

        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public bool Interactable {
            get => _button.Interactable;
            set => _button.Interactable = value;
        }
        
        #endregion
        
        #region Context

        public class AlphaModalContext : ModalContext {
            [ExternalProperty, UsedImplicitly]
            public float Alpha {
                get => _alpha;
                set {
                    _alpha = value;
                    NotifyContextUpdated();
                }
            }

            private float _alpha;
        }

        [ExternalProperty(prefix: null), UsedImplicitly]
        public readonly AlphaModalContext context = new();

        #endregion

        #region Modal

        private class AlphaModal : PersistentModalComponentBase<AlphaModal, AlphaModalContext> {
            [UIComponent("slider"), UsedImplicitly]
            private AlphaSlider _slider = null!;

            protected override void OnContextUpdate() {
                _slider.Value = Context.Alpha;
            }

            [UIAction("value-change"), UsedImplicitly]
            private void HandleValueChanged(float value) {
                UpdateContext(() => Context.Alpha = value);
            }
        }

        private void OpenModal() {
            ModalSystemHelper.OpenPersistentModalRelatively<AlphaModal, AlphaModalContext>(
                context,
                transform,
                ContentTransform,
                ModalSystemHelper.RelativePlacement.TopRight
            );
        }

        #endregion

        #region Construct

        private ModalButton _button = null!;
        
        protected override void OnConstruct(Transform parent) {
            _button = ModalButton.Instantiate(parent);
            _button.Icon.Sprite = BundleLoader.AlphaIcon;
            _button.ShowText = false;
            _button.InheritSize = true;
            _button.GrowOnHover = true;
            _button.HoverLerpMul = 10f;
            _button.HoverScaleSum = Vector3.one * 0.07f;
            _button.ClickEvent += HandleButtonClicked;
            context.ContextUpdatedEvent += HandleContextUpdated;
        }

        #endregion
        
        #region Callbacks

        private void HandleButtonClicked() {
            OpenModal();
        }

        private void HandleContextUpdated() {
            ValueChangedEvent?.Invoke(context.Alpha);
        }

        #endregion
    }
}