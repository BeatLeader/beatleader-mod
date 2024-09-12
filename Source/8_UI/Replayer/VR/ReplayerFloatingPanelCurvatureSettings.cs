using System;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class ReplayerFloatingPanelCurvatureSettings : ReactiveComponent {
        #region Setup

        public event Action? CloseButtonClicked;

        private FloatingScreen? _screen;
        private ReplayerFloatingUISettings? _settings;
        private bool _isInitialized;

        public void Setup(FloatingScreen screen, ReplayerFloatingUISettings settings) {
            _screen = screen;
            _settings = settings;
            _isInitialized = true;
            _radiusSlider.Value = settings.CurvatureRadius;
            _radiusToggle.SetActive(settings.CurvatureEnabled, false, false);
        }

        #endregion

        #region Animation

        private readonly ValueAnimator _valueAnimator = new() { LerpCoefficient = 15f };

        public void Present() {
            _valueAnimator.Push();
            _screen!.BlockRaycasts = true;
        }

        public void Hide() {
            _valueAnimator.Pull();
            _screen!.BlockRaycasts = false;
        }

        private void RefreshPanelAnimation(float progress) {
            ContentTransform.localScale = Vector3.one * Mathf.Lerp(0f, 1f, progress);
        }

        protected override void OnUpdate() {
            _valueAnimator.Update();
            RefreshPanelAnimation(_valueAnimator.Progress);
        }

        #endregion

        #region Construct

        private Slider _radiusSlider = null!;
        private Toggle _radiusToggle = null!;
        private float _radius;

        protected override GameObject Construct() {
            return new Dummy {
                ContentTransform = {
                    pivot = new(0.5f, 1f)
                },
                Children = {
                    new Image {
                        ContentTransform = {
                            pivot = new(0.5f, 1f)
                        },
                        Children = {
                            //radius toggle
                            new Toggle()
                                .WithListener(
                                    x => x.Active,
                                    HandleRadiusStateChanged
                                )
                                .Bind(ref _radiusToggle)
                                .InNamedRail("Enable Radius"),
                            //radius slider
                            new Slider {
                                ValueRange = new() {
                                    Start = 40f,
                                    End = 120f
                                },
                                ValueStep = 10f
                            }.WithListener(
                                x => x.Value,
                                HandleRadiusChanged
                            ).AsFlexItem(
                                size: new() { x = 36f, y = 6f }
                            ).Bind(ref _radiusSlider).InNamedRail("Radius"),
                            //ok button
                            new BsPrimaryButton {
                                OnClick = () => CloseButtonClicked?.Invoke()
                            }.WithLabel("OK").AsFlexItem()
                        }
                    }.AsBackground(
                        color: new(0.1f, 0.1f, 0.1f, 1f),
                        pixelsPerUnit: 7f
                    ).AsFlexGroup(
                        direction: FlexDirection.Column,
                        padding: new() { top = 1f, bottom = 1f, right = 2f, left = 2f }
                    ).AsFlexItem(
                        grow: 1f,
                        margin: new() { top = 1f }
                    )
                }
            }.AsFlexGroup().WithSizeDelta(50f, 24f).Use();
        }

        protected override void OnInitialize() {
            Content.AddComponent<FloatingScreen>();
        }

        #endregion

        #region Callbacks

        private void HandleRadiusStateChanged(bool state) {
            if (!_isInitialized) return;
            _radiusSlider.Interactable = state;
            _screen!.CurvatureRadius = state ? _radius : 0f;
            _settings!.CurvatureEnabled = state;
        }

        private void HandleRadiusChanged(float radius) {
            if (!_isInitialized) return;
            _screen!.CurvatureRadius = radius;
            _settings!.CurvatureRadius = radius;
            _radius = radius;
        }

        #endregion
    }
}