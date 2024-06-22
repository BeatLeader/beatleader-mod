using System.Collections;
using BeatLeader.UI.Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class TextArea : ReactiveComponent, IGraphicComponent {
        #region UI Props

        public string Text {
            get => _text;
            set {
                _text = value;
                RefreshText();
                RefreshClearButton();
                NotifyPropertyChanged();
            }
        }

        public string Placeholder {
            get => _placeholder;
            set {
                _placeholder = value;
                RefreshText();
                NotifyPropertyChanged();
            }
        }

        public Sprite? Icon {
            get => _icon.Sprite;
            set {
                _icon.Enabled = value != null;
                _icon.Sprite = value;
            }
        }

        public bool ShowClearButton {
            get => _showClearButton;
            set {
                _showClearButton = value;
                RefreshClearButton();
            }
        }

        public bool ShowCaret {
            get => _showCaret;
            set {
                _showCaret = value;
                RefreshCaret();
            }
        }

        public bool RaycastTarget {
            get => _backgroundButton.RaycastTarget;
            set => _backgroundButton.RaycastTarget = value;
        }

        private string _text = string.Empty;
        private string _placeholder = "Enter";
        private bool _showClearButton = true;
        private bool _showCaret = false;

        #endregion

        #region Setup

        public bool Focused {
            get => _focused;
            set => SetInputEnabled(value);
        }

        private static readonly Color placeholderColor = UIStyle.InactiveTextColor;
        private static readonly Color textColor = Color.white;

        private bool _focused;

        private void RefreshText() {
            var showPlaceholder = _focused && Text.Length > 0;
            _label.Text = showPlaceholder ? Text : _placeholder;
            _label.Color = showPlaceholder ? textColor : placeholderColor;
        }

        private void RefreshClearButton() {
            _clearButton.Enabled = _showClearButton && !_focused && Text.Length > 0;
        }

        private void SetInputEnabled(bool enabled) {
            _focused = enabled;
            RefreshCaret();
            RefreshClearButton();
            NotifyPropertyChanged(nameof(Focused));
        }

        protected override void OnInitialize() {
            _label.ModifierUpdatedEvent += RefreshCaretPos;
        }

        #endregion

        #region Caret

        private void RefreshCaret() {
            _caret.Enabled = _focused && ShowCaret;
            if (!ShowCaret) return;
            if (_focused) {
                StartCoroutine(CaretAnimationCoroutine());
            } else {
                StopAllCoroutines();
            }
        }

        private void RefreshCaretPos() {
            var transform = _caret.ContentTransform;
            var pos = transform.anchoredPosition;
            pos.x = Text.Length == 0 ? 0 : Mathf.Max(((ILayoutItem)_label).DesiredWidth.GetValueOrDefault(), 0f);
            transform.anchoredPosition = pos;
        }

        private IEnumerator CaretAnimationCoroutine() {
            while (true) {
                yield return new WaitForSeconds(0.4f);
                _caret.Enabled = !_caret.Enabled;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        #endregion

        #region Construct

        protected override float? DesiredHeight => 8f;

        private Button _clearButton = null!;
        private Button _backgroundButton = null!;
        private Label _label = null!;
        private Image _icon = null!;
        private Image _caret = null!;

        protected override GameObject Construct() {
            return new AeroButton {
                GrowOnHover = false,
                HoverLerpMul = float.MaxValue,
                Children = {
                    //icon
                    new Image {
                        Sprite = null,
                        PreserveAspect = true,
                        Skew = UIStyle.Skew,
                        Color = UIStyle.SecondaryTextColor
                    }.AsFlexItem(
                        size: new() { x = 4f, y = "auto" },
                        margin: new() { left = 1f }
                    ).Bind(ref _icon),

                    //text
                    new Label {
                        Alignment = TextAlignmentOptions.MidlineLeft,
                        Overflow = TextOverflowModes.Ellipsis,
                        FontStyle = FontStyles.Italic,
                        Color = UIStyle.InactiveTextColor,
                        FontSizeMin = 3f,
                        FontSizeMax = 4f,
                        EnableAutoSizing = true
                    }.With(
                        x => {
                            //caret
                            new Image {
                                Sprite = GameResources.Sprites.Caret,
                                Skew = UIStyle.Skew,
                                Enabled = false
                            }.AsRectItem(
                                sizeDelta: new() { x = 0.6f, y = 4f },
                                anchorMin: new(0f, 0.5f),
                                anchorMax: new(0f, 0.5f),
                                pivot: new(0f, 0.5f)
                            ).Bind(ref _caret).Use(x.Content);
                        }
                    ).AsFlexItem(grow: 1f).Bind(ref _label),

                    //clear button
                    new ImageButton {
                        Enabled = false,
                        WithinLayoutIfDisabled = true,
                        Image = {
                            Sprite = BundleLoader.Sprites.background,
                            Material = GameResources.UINoGlowMaterial
                        },
                        Colors = new StateColorSet {
                            Color = Color.black.ColorWithAlpha(0.5f),
                            HoveredColor = Color.black.ColorWithAlpha(0.8f)
                        },
                        HoverLerpMul = int.MaxValue,
                        GrowOnHover = false,
                        Children = {
                            new Image {
                                Sprite = BundleLoader.CrossIcon
                            }.AsFlexItem(grow: 1f, size: "auto")
                        }
                    }.AsFlexItem(
                        size: new() { x = 4f },
                        aspectRatio: 1f,
                        alignSelf: Align.Center,
                        margin: new() { right = 1f }
                    ).AsFlexGroup(
                        padding: 1f
                    ).WithClickListener(HandleClearButtonClicked).Bind(ref _clearButton),
                }
            }.AsFlexGroup(padding: 1f, gap: 1f).WithAnimation(
                x => _label.Color = Text.Length > 0 ? textColor : Color.Lerp(
                    placeholderColor,
                    placeholderColor.ColorWithAlpha(0.5f),
                    x
                )
            ).WithClickListener(() => SetInputEnabled(true)).Bind(ref _backgroundButton).Use();
        }

        #endregion

        #region Callbacks

        private void HandleClearButtonClicked() {
            Text = string.Empty;
            RefreshText();
            RefreshClearButton();
        }

        #endregion
    }
}