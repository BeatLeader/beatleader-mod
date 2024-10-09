using System;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class AvatarPartControl : ReactiveComponent {
        #region Modal

        private class AlphaModal : ModalComponentBase {
            #region Setup

            private AvatarPartConfigWithModel? _config;
            private bool _ignoreChanges;
            private bool _alphaEnabled;
            private float _alpha;

            public void Setup(AvatarPartConfigWithModel config) {
                if (_ignoreChanges) return;
                _config = config;
                var alpha = config.PartConfig.Alpha;
                _alpha = alpha;
                _alphaEnabled = Math.Abs(alpha - 1f) > 0.01f;
                _toggle.SetActive(_alphaEnabled, false, true);
                _slider.SetValueSilent(alpha);
            }

            private void RefreshAlpha() {
                _ignoreChanges = true;
                _config!.PartConfig.Alpha = _alphaEnabled ? _alpha : 1f;
                _ignoreChanges = false;
            }

            #endregion

            #region Construct

            private Toggle _toggle = null!;
            private Slider _slider = null!;

            protected override GameObject Construct() {
                return new Image {
                    Children = {
                        new Toggle()
                            .WithListener(
                                x => x.Active,
                                HandleToggleStateChanged
                            )
                            .Bind(ref _toggle)
                            .InNamedRail("Transparent"),
                        //
                        new Slider {
                            ValueRange = {
                                Start = 0f,
                                End = 1f
                            },
                            ValueStep = 0.1f,
                            Value = 0.7f
                        }.WithListener(
                            x => x.Value,
                            HandleSliderValueChanged
                        ).Bind(ref _slider).InNamedRail("Value")
                    }
                }.AsFlexGroup(
                    direction: FlexDirection.Column,
                    gap: 1f,
                    padding: new() { left = 2f, right = 2f, bottom = 1f, top = 1f }
                ).AsBackground(
                    color: new(0.1f, 0.1f, 0.1f, 1f)
                ).Use();
            }

            protected override void OnInitialize() {
                base.OnInitialize();
                this.WithSizeDelta(50f, 20f);
            }

            #endregion

            #region Callbacks

            private void HandleToggleStateChanged(bool state) {
                if (_config == null) return;
                _alphaEnabled = state;
                RefreshAlpha();
            }

            private void HandleSliderValueChanged(float value) {
                if (_config == null) return;
                _alpha = value;
                RefreshAlpha();
            }

            #endregion
        }

        #endregion

        #region Setup

        private AvatarPartConfigWithModel? _config;
        private bool _ignoreUpdates;
        private bool _alphaModalOpened;

        public void Setup(AvatarPartConfigWithModel config) {
            if (_config != null) {
                _config.PartConfig.ConfigUpdatedEvent -= HandleConfigUpdated;
            }
            _config = config;
            _config.PartConfig.ConfigUpdatedEvent += HandleConfigUpdated;
            //
            _label.Text = config.PartModel.Name;
            _alphaButton.Enabled = config.PartModel.HasAlphaSupport;
            HandleConfigUpdated();
        }

        protected override void OnDestroy() {
            if (_config != null) {
                _config.PartConfig.ConfigUpdatedEvent -= HandleConfigUpdated;
            }
        }

        #endregion

        #region Construct

        private SharedModal<AlphaModal> _modal = null!;
        private ImageButton _alphaButton = null!;
        private Toggle _toggle = null!;
        private Label _label = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new SharedModal<AlphaModal>()
                        .WithOpenListener(HandleAlphaModalOpened)
                        .WithCloseListener(HandleAlphaModalClosed)
                        .WithShadow()
                        .WithAnchor(() => ContentTransform, RelativePlacement.TopRight)
                        .Bind(ref _modal),
                    //
                    new Label {
                        Alignment = TextAlignmentOptions.MidlineLeft
                    }.AsFlexItem(grow: 1f).Bind(ref _label),
                    //
                    new ImageButton {
                        Image = {
                            Sprite = BundleLoader.AlphaIcon
                        }
                    }.WithModal(_modal).AsFlexItem(aspectRatio: 1f, margin: 0.5f).Bind(ref _alphaButton),
                    //
                    new Toggle()
                        .WithListener(
                            x => x.Active,
                            HandleToggleStateChanged
                        )
                        .AsFlexItem()
                        .Bind(ref _toggle)
                }
            }.AsFlexGroup(gap: 1f).Use();
        }

        #endregion

        #region Callbacks

        private void HandleAlphaModalOpened(IModal modal, bool finished) {
            if (finished) return;
            _modal.Modal.Setup(_config!);
            _alphaModalOpened = true;
        }

        private void HandleAlphaModalClosed(IModal modal, bool finished) {
            if (!finished) return;
            _alphaModalOpened = false;
        }

        private void HandleConfigUpdated() {
            if (_ignoreUpdates) return;
            _toggle.SetActive(_config!.PartConfig.Active, false, true);
            _toggle.Interactable = !_config.PartConfig.ControlledByMask;
            if (_alphaModalOpened) {
                _modal.Modal.Setup(_config);
            }
        }

        private void HandleToggleStateChanged(bool state) {
            if (_config == null) return;
            _ignoreUpdates = true;
            _config.PartConfig.PotentiallyActive = state;
            _ignoreUpdates = false;
        }

        #endregion
    }
}