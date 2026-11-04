using System;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Yoga;
using UnityEngine;
using UnityEngine.UI;
using AnimationCurve = Reactive.AnimationCurve;

namespace BeatLeader.UI.MainMenu {
    internal class SpecialEventPanel : ReactiveComponent {
        #region Public API

        public Action? OnBackClick { get; set; }

        public void SetData(PlatformEventStatus evt) {
            _event.Value = evt;
            _event.EnforceInvalidation();
            _spinnerAlpha.TargetValue = 0f;
        }

        public void SetLoading() {
            _spinnerAlpha.TargetValue = 1f;
        }

        public void PlayPreview() {
            _mapPanel.PlayPreview();
        }
        
        public void CancelPreview() {
            _mapPanel.CancelPreview();
        }

        #endregion

        #region Construct

        private State<PlatformEventStatus> _event = null!;
        private AnimatedState<float> _spinnerAlpha = null!;
        private SpecialEventMapPanel _mapPanel = null!;

        protected override GameObject Construct() {
            _event = Remember<PlatformEventStatus>(null!);
            _spinnerAlpha = RememberAnimated(0f, 200.ms, AnimationCurve.EaseInOut);

            return new Background {
                    FlexController = {
                        FlexDirection = FlexDirection.Column,
                        JustifyContent = Justify.Center,
                        Padding = 1.pt
                    },

                    FlexItem = {
                        Flex = 1f,
                        Size = new() { x = 52.pt }
                    },

                    Children = {
                        new NewsHeader {
                                sText = _event.Map(x => x.eventDescription.name).InLazy(),
                                sAccent = _event.Map(static x => x.eventDescription.MainColor() ?? Color.black.ColorWithAlpha(0.5f)).InLazy()
                            }
                            .WithAlpha(_spinnerAlpha, invert: true),

                        new SpecialEventMapPanel()
                            .WithAlpha(_spinnerAlpha, invert: true)
                            .On(_event, (x, y) => x.SetData(y.today), lazy: true)
                            .Bind(ref _mapPanel),

                        new SpecialEventBar {
                                OnBackClick = () => OnBackClick?.Invoke(),
                                OnPlayClick = () => MapDownloadDialog.OpenSongOrDownloadDialog(_mapPanel._map.Value.song, ContentTransform),
                                OnDayChanged = x => _mapPanel.SetData(x)
                            }
                            .WithAlpha(_spinnerAlpha, invert: true)
                            .On(_event, (x, y) => x.SetData(y), lazy: true),

                        new Spinner {
                            FlexItem = {
                                Position = new() { left = 0.pt, right = 0.pt },
                                PositionType = PositionType.Absolute,
                                Size = new() { y = 15.pt },
                                Margin = YogaFrame.Auto
                            },

                            Image = {
                                RaycastTarget = false
                            }
                        }.WithAlpha(_spinnerAlpha)
                    }
                }
                .AsBackground(color: Color.black.ColorWithAlpha(0.5f))
                .AsRectMask()
                .Use();
        }

        #endregion
    }
}