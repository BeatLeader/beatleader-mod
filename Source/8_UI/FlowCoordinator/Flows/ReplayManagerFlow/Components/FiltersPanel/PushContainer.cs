using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class PushContainer : ReactiveComponent {
        #region Setup

        public ILayoutItem? OpenedView {
            get => _openedView;
            set {
                if (_openedView != null) {
                    _backgroundImage.Children.Remove(_openedView);
                }
                _openedView = value;
                if (_openedView != null) { 
                    if (_openedView is ReactiveComponent comp) {
                        comp.WithinLayoutIfDisabled = true;
                    }
                    _backgroundImage.Children.Add(_openedView);
                }
            }
        }

        public ILayoutItem? ClosedView {
            get => _closedView;
            set {
                if (_closedView != null) {
                    _backgroundImage.Children.Remove(_closedView);
                }
                _closedView = value;
                if (_closedView != null) {
                    if (_closedView is ReactiveComponent comp) {
                        comp.WithinLayoutIfDisabled = true;
                    }
                    _backgroundImage.Children.Add(_closedView);
                }
            }
        }

        public bool Opened {
            get => _opened;
            set {
                if (_opened == value) return;
                _opened = value;
                _progress = 0f;
            }
        }

        public Color Color {
            get => _backgroundImage.Color;
            set => _backgroundImage.Color = value;
        }

        private ILayoutItem? _openedView;
        private ILayoutItem? _closedView;

        public void Push() {
            _opened = !_opened;
            _progress = 0f;
        }

        #endregion

        #region Construct

        public Background BackgroundImage => _backgroundImage;

        private Background _backgroundImage = null!;
        private CanvasGroup _canvasGroup = null!;

        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    new Background()
                        .AsFlexGroup()
                        .AsFlexItem(grow: 1f)
                        .AsBlurBackground()
                        .Bind(ref _backgroundImage),
                }
            }.AsFlexGroup().WithNativeComponent(out _canvasGroup).Use();
        }

        protected override void OnInitialize() {
            RefreshContent();
        }

        #endregion

        #region Animation

        private const float LerpCoefficient = 10.0f;
        private const float SnapBorder = 1.0f - 1e-5f;

        private float _progress = 1.0f;
        private bool _opened;

        protected override void OnUpdate() {
            _progress = _progress > SnapBorder ? 1.0f : Mathf.Lerp(_progress, 1f, Time.deltaTime * LerpCoefficient);
            RefreshVisuals();
        }

        private void RefreshContent() {
            _canvasGroup.blocksRaycasts = _opened;
            _canvasGroup.interactable = _opened;

            if (OpenedView is ReactiveComponent opened) {
                opened.Enabled = _opened;
            } else if (OpenedView != null) {
                OpenedView.WithinLayout = _opened;
            }

            if (ClosedView is ReactiveComponent closed) {
                closed.Enabled = !_opened;
            } else if (ClosedView != null) {
                ClosedView.WithinLayout = !_opened;
            }
        }

        private void RefreshVisuals() {
            if (_progress > 0.5f) {
                RefreshContent();
            }
            var t = Mathf.Abs((_progress - 0.5f) * 2.0f);
            _canvasGroup.alpha = t;
            ContentTransform.localScale = new Vector3(1.0f, t, 1.0f);
        }

        #endregion
    }
}