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

        #region Hide & Present

        public void Present() {
            _panelScale.Value = Vector3.one;
            _canvasAlpha.Value = 0.8f;
            _screen!.BlockRaycasts = true;
        }

        public void Hide() {
            _panelScale.Value = Vector3.zero;
            _canvasAlpha.Value = 1f;
            _screen!.BlockRaycasts = false;
        }

        #endregion

        #region Construct

        private AnimatedValue<Vector3> _panelScale = null!;
        private AnimatedValue<float> _canvasAlpha = null!;

        private Slider _radiusSlider = null!;
        private Toggle _radiusToggle = null!;
        private float _radius;

        protected override GameObject Construct() {
            _panelScale = RememberAnimated(Vector3.zero, 15.fact());
            _canvasAlpha = RememberAnimated(1f, 10.fact());

            _canvasAlpha.ValueChangedEvent += x => {
                if (_screen != null) {
                    _screen.Alpha = x;
                }
            };

            return new Layout {
                    ContentTransform = {
                        pivot = new(0.5f, 1f)
                    },
                    Children = {
                        new Background {
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
                                    Text = "OK",
                                    Skew = 0f,
                                    OnClick = () => CloseButtonClicked?.Invoke()
                                }.AsFlexItem()
                            }
                        }.AsBackground(
                            color: new(0.1f, 0.1f, 0.1f, 1f),
                            pixelsPerUnit: 7f
                        ).AsFlexGroup(
                            direction: FlexDirection.Column,
                            padding: new() { top = 1f, bottom = 1f, right = 2f, left = 2f },
                            gap: 1f
                        ).AsFlexItem(
                            flexGrow: 1f,
                            margin: new() { top = 1f }
                        )
                    }
                }.AsFlexGroup(
                    constrainHorizontal: false,
                    constrainVertical: false
                )
                .WithNativeComponent(out CanvasGroup group)
                .WithNativeComponent(out FloatingScreen _)
                .Animate(_panelScale, x => x.ContentTransform.localScale)
                .With(_ => group.ignoreParentGroups = true)
                .Use();
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