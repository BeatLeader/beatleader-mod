using System;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class FilterSection : ReactiveComponent {
        #region Public API

        public string Caption {
            get => _caption.Value;
            set => _caption.Value = value;
        }

        public ILayoutItem? FilterView {
            get;
            set {
                if (field != null) {
                    _contentContainer.Children.Remove(field);
                }

                field = value;

                if (field != null) {
                    _contentContainer.Children.Add(field);
                }
            }
        }

        public bool Toggled {
            get => _toggled.Value;
            set => _toggled.Value = value;
        }

        public bool Locked {
            get => _locked.Value;
            set => _locked.Value = value;
        }

        public Action<bool>? OnToggled { get; set; }

        #endregion

        #region Construct

        private State<string> _caption = null!;
        private State<bool> _toggled = null!;
        private State<bool> _locked = null!;
        private Layout _contentContainer = null!;

        protected override GameObject Construct() {
            _caption = Remember(string.Empty);
            _toggled = Remember(true);
            _locked = Remember(false);

            var applies = RememberDerived(_toggled, _locked, static (toggled, locked) => toggled && !locked);

            return new Background {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    Padding = new() { left = 1.5f, right = 1.5f, top = 1f, bottom = 1.5f },
                    Gap = 0.5f
                },

                Children = {
                    new Layout {
                        FlexController = {
                            AlignItems = Align.Center
                        },

                        FlexItem = {
                            Size = new() { y = 4.pt }
                        },

                        Children = {
                            new Label {
                                FlexItem = {
                                    FlexGrow = 1f
                                },

                                FontSize = 2.5f,
                                Color = BeatSaberStyle.SecondaryTextColor,
                                Alignment = TextAlignmentOptions.MidlineLeft,
                                sText = _caption.Map(static x => x.ToUpper())
                            },

                            new BsToggle {
                                FlexItem = {
                                    Size = new() { x = 18.pt, y = 4.pt }
                                },

                                sToggled = _toggled,
                                sInteractable = _locked.Map(static x => !x),
                                OnToggleStateChanged = x => OnToggled?.Invoke(x)
                            }
                        }
                    },

                    new Layout {
                        FlexController = {
                            FlexDirection = FlexDirection.Column
                        },

                        Children = {
                            new Layout {
                                FlexController = {
                                    FlexDirection = FlexDirection.Column
                                }
                            }.Bind(ref _contentContainer),

                            new Image {
                                IgnoreLayout = true,
                                Sprite = BundleLoader.Sprites.transparentPixel,
                                RaycastTarget = true,
                                sEnabled = applies.Map(static x => !x)
                            }.WithRectExpand()
                        }
                    }.WithAlpha(applies.Map(static x => x ? 1f : 0.3f))
                }
            }.AsBlurBackground(color: (Color.white * 0.8f).ColorWithAlpha(1f)).Use();
        }

        #endregion
    }
}
