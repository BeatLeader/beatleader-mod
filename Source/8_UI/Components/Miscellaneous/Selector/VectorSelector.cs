using System;
using BeatLeader.UI.Components.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class VectorSelector : TextSettingComponentBase<VectorSelector> {
        #region Events

        [ExternalProperty, UsedImplicitly]
        public event Action<Vector3>? ValueChangedEvent;

        #endregion

        #region UI Properties

        public override bool Interactable { get; set; }

        #endregion
        
        #region Context

        public class VectorModalContext : ModalContext {
            [ExternalProperty, UsedImplicitly]
            public Vector3 MinVector {
                get => _minVector;
                set {
                    _minVector = value;
                    NotifyContextUpdated();
                }
            }

            [ExternalProperty, UsedImplicitly]
            public Vector3 MaxVector {
                get => _maxVector;
                set {
                    _maxVector = value;
                    NotifyContextUpdated();
                }
            }

            [ExternalProperty, UsedImplicitly]
            public Vector3 StepVector {
                get => _stepVector;
                set {
                    _stepVector = value;
                    NotifyContextUpdated();
                }
            }

            [ExternalProperty, UsedImplicitly]
            public Vector3 Vector {
                get => _vector;
                set {
                    _vector = value;
                    NotifyContextUpdated();
                }
            }

            /// <summary>
            /// Use 0 to hide the dimension and 1 to show
            /// </summary>
            [ExternalProperty, UsedImplicitly]
            public Vector3 DimensionMask {
                get => _dimensionMask;
                set {
                    _dimensionMask = value;
                    NotifyContextUpdated();
                }
            }

            private Vector3 _vector = Vector3.one;
            private Vector3 _minVector;
            private Vector3 _maxVector = Vector3.one;
            private Vector3 _stepVector;
            private Vector3 _dimensionMask = Vector3.one;
        }

        [ExternalProperty(prefix: null), UsedImplicitly]
        public readonly VectorModalContext context = new();

        private static string GetVectorFromContext(VectorModalContext context) {
            var text = "Vector (";
            var vector = context.Vector;
            var mask = context.DimensionMask;
            if (mask.x is 1) {
                text += $" X: {vector.x:F2}";
            }
            if (mask.y is 1) {
                text += $" Y: {vector.y:F2}";
            }
            if (mask.z is 1) {
                text += $" Z: {vector.z:F2}";
            }
            text += ")";
            return text;
        }

        #endregion

        #region Modal

        private class VectorModal : PersistentModalComponentBase<VectorModal, VectorModalContext> {
            #region UI Components

            [UIComponent("text"), UsedImplicitly]
            private TMP_Text _text = null!;

            [UIComponent("x-slider"), UsedImplicitly]
            private NormalizedSlider _xSlider = null!;

            [UIComponent("y-slider"), UsedImplicitly]
            private NormalizedSlider _ySlider = null!;

            [UIComponent("z-slider"), UsedImplicitly]
            private NormalizedSlider _zSlider = null!;

            #endregion

            #region Text

            private void RefreshText() {
                _text.text = GetVectorFromContext(Context);
            }

            #endregion

            #region Setup

            protected override void OnContextUpdate() {
                var vector = Context.Vector;
                var minVector = Context.MinVector;
                var maxVector = Context.MaxVector;
                var stepVector = Context.StepVector;
                var maskVector = Context.DimensionMask;

                _xSlider.ValueRange = new(minVector.x, maxVector.x);
                _xSlider.ValueStep = stepVector.x;
                _xSlider.Value = vector.x;
                _xSlider.Content.SetActive(maskVector[0] is 1);

                _ySlider.ValueRange = new(minVector.y, maxVector.y);
                _ySlider.ValueStep = stepVector.y;
                _ySlider.Value = vector.y;
                _ySlider.Content.SetActive(maskVector[1] is 1);

                _zSlider.ValueRange = new(minVector.z, maxVector.z);
                _zSlider.ValueStep = stepVector.z;
                _zSlider.Value = vector.z;
                _zSlider.Content.SetActive(maskVector[2] is 1);
                RefreshText();
            }

            private void ModifyVector(float value, int dimension) {
                var vector = Context.Vector;
                vector[dimension] = value;
                UpdateContext(() => Context.Vector = vector);
                RefreshText();
            }

            #endregion

            #region Callbacks

            [UIAction("x-value-change"), UsedImplicitly]
            private void HandleXValueChanged(float value) {
                ModifyVector(value, 0);
            }

            [UIAction("y-value-change"), UsedImplicitly]
            private void HandleYValueChanged(float value) {
                ModifyVector(value, 1);
            }

            [UIAction("z-value-change"), UsedImplicitly]
            private void HandleZValueChanged(float value) {
                ModifyVector(value, 2);
            }

            #endregion
        }

        private void OpenModal() {
            ModalSystemHelper.OpenPersistentModalRelatively<VectorModal, VectorModalContext>(
                context,
                transform,
                ContentTransform,
                ModalSystemHelper.RelativePlacement.TopRight
            );
        }

        #endregion

        #region Construct

        private TMP_Text _text = null!;

        protected override RectTransform ConstructSetting(Transform parent) {
            var container = parent.gameObject.CreateChild("ButtonContainer");
            var containerRect = container.AddComponent<RectTransform>();
            var containerGroup = container.AddComponent<HorizontalLayoutGroup>();
            containerGroup.childForceExpandWidth = false;
            containerGroup.childControlWidth = false;
            containerGroup.childAlignment = TextAnchor.MiddleRight;

            var glassButton = EmptyGlassButton.Instantiate(containerRect);
            glassButton.ClickEvent += HandleButtonClicked;
            glassButton.HoverScaleSum = Vector3.one * 0.05f;

            var buttonGroup = glassButton.LayoutComponentContent!.GetComponent<LayoutGroup>();
            buttonGroup.padding = new(1, 1, 0, 0);

            var textGo = glassButton.LayoutComponentContent!.CreateChild("Text");
            _text = textGo.AddComponent<TextMeshProUGUI>();
            _text.fontSize = 3.8f;
            _text.alignment = TextAlignmentOptions.Center;

            var textFitter = textGo.AddComponent<ContentSizeFitter>();
            textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return containerRect;
        }
        
        protected override void OnInitialize() {
            base.OnInitialize();
            context.ContextUpdatedEvent += HandleContextUpdated;
            HandleContextUpdated();
        }

        #endregion

        #region Callbacks

        private void HandleButtonClicked() {
            OpenModal();
        }

        private void HandleContextUpdated() {
            _text.text = GetVectorFromContext(context);
            ValueChangedEvent?.Invoke(context.Vector);
        }

        #endregion
    }
}