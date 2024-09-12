using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class PushContainer : DrivingReactiveComponent {
        #region Setup
        
        public string Placeholder {
            get => _label.Text;
            set => _label.Text = value;
        }

        public bool Opened {
            get => _opened;
            set {
                if (_opened == value) return;
                _opened = value;
                _progress = 0f;
            }
        }
        
        public void Push() {
            _opened = !_opened;
            _progress = 0f;
        }

        #endregion

        #region Construct

        public Image BackgroundImage => _backgroundImage;

        protected override Transform ChildrenContainer => _childrenContainer;

        private Image _backgroundImage = null!;
        private Label _label = null!;
        private CanvasGroup _canvasGroup = null!;
        private RectTransform _childrenContainer = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new Image()
                        .WithRectExpand()
                        .AsBlurBackground()
                        .Bind(ref _childrenContainer)
                        .Bind(ref _backgroundImage),
                    //
                    new Label {
                        Enabled = false,
                    }.WithRectExpand().Bind(ref _label)
                }
            }.WithNativeComponent(out _canvasGroup).Use();
        }

        protected override void OnInitialize() {
            RefreshContent();
        }

        protected override void AppendReactiveChild(ReactiveComponentBase comp) {
            base.AppendReactiveChild(comp);
            comp.WithinLayoutIfDisabled = true;
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
            _label.Enabled = !_opened;
            foreach (var child in Children) {
                if (child is not ReactiveComponent comp) continue;
                comp.Enabled = _opened;
            }
        }

        private void RefreshVisuals() {
            if (_progress > 0.5f) {
                RefreshContent();
            }
            var t = Mathf.Abs((_progress - 0.5f) * 2.0f);
            _canvasGroup.alpha = t;
            ContentTransform.localScale = new Vector3(1.0f, t, 1.0f);
            _backgroundImage.Color = _backgroundImage.Color.ColorWithAlpha(t);
        }

        #endregion
    }
}