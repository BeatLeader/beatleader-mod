using System;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
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
            _radiusSlider.Value = settings.CurvatureRadius;
            _radiusToggle.Toggled = settings.CurvatureEnabled;
            _isInitialized = true;
            HandleRadiusChanged(_radiusSlider.Value);
            HandleRadiusStateChanged(_radiusToggle.Toggled);
        }

        #endregion

        #region Hide & Present

        public bool IsPresent {
            get => _panelScale.TargetValue != Vector3.zero;
            set {
                _panelScale.TargetValue = value ? Vector3.one : Vector3.zero;
                _canvasAlpha.TargetValue = value ? 0.8f : 1f;
                _screen!.BlockRaycasts = value;
            }
        }

        #endregion

        #region Construct

        private AnimatedState<Vector3> _panelScale = null!;
        private AnimatedState<float> _canvasAlpha = null!;

        private BsSlider _radiusSlider = null!;
        private BsToggle _radiusToggle = null!;
        private float _radius;

        protected override GameObject Construct() {
            _panelScale = RememberAnimated(Vector3.zero, 15.fact);
            _canvasAlpha = RememberAnimated(1f, 10.fact);

            _canvasAlpha.AddCallback(
                x => {
                    if (_screen != null) {
                        _screen.Alpha = x;
                    }
                },
                lazy: true
            );

            return new Layout {
                    FlexController = {
                        ConstrainHorizontal = false,
                        ConstrainVertical = false
                    },

                    ContentTransform = {
                        pivot = new(0.5f, 1f),
                        slocalScale = _panelScale.InLazy()
                    },
                    Children = {
                        new Background {
                            FlexController = {
                                FlexDirection = FlexDirection.Column,
                                Padding = new() { top = 1f, bottom = 1f, right = 2f, left = 2f },
                                Gap = 1f
                            },

                            FlexItem = {
                                FlexGrow = 1f,
                                Margin = new() { top = 1f }
                            },

                            ContentTransform = {
                                pivot = new(0.5f, 1f)
                            },
                            Children = {
                                //radius toggle
                                new BsToggle {
                                        OnToggleStateChanged = HandleRadiusStateChanged
                                    }
                                    .Bind(ref _radiusToggle)
                                    .InNamedRail("Enable Radius"),
                                //radius slider
                                new BsSlider {
                                    FlexItem = {
                                        Size = new() { x = 36f, y = 6f }
                                    },

                                    MinValue = 40f,
                                    MaxValue = 120f,
                                    ValueStep = 10f,
                                    OnValueChanged = HandleRadiusChanged
                                }.Bind(ref _radiusSlider).InNamedRail("Radius"),
                                //ok button
                                new BsPrimaryTextButton {
                                    Text = "OK",
                                    Skew = 0f,
                                    OnClick = () => CloseButtonClicked?.Invoke()
                                }
                            }
                        }.AsBackground(
                            color: new(0.1f, 0.1f, 0.1f, 1f),
                            pixelsPerUnit: 7f
                        )
                    }
                }
                .WithNativeComponent(out CanvasGroup group)
                .WithNativeComponent(out FloatingScreen _)
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