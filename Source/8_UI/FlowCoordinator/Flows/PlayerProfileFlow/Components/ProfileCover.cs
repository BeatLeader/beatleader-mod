using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components.GIF;
using Reactive.Yoga;
using UnityEngine;
using AnimationCurve = Reactive.AnimationCurve;

namespace BeatLeader.UI.Hub {
    internal class ProfileCover : ReactiveComponent {
        #region Props

        public string? CoverUrl {
            get => _coverUrl.Value;
            set => _coverUrl.Value = value;
        }

        public string? FallbackUrl {
            get => _fallbackUrl.Value;
            set => _fallbackUrl.Value = value;
        }

        #endregion

        #region Loading

        private static readonly Animation<IReactiveComponent> pulse = new(
            AnimationTracks.Fade(0.35f, 0.9f),
            700.ms,
            AnimationCurve.EaseInOut,
            AnimationIteration.Infinite,
            AnimationDirection.Alternate
        );

        private CancellationTokenSource? _tokenSource;

        private async Task LoadAsync(string? cover, string? fallback) {
            _tokenSource?.Cancel();
            var tokenSource = _tokenSource = new();

            _sprite.Value = null;
            _skeletonPlayer.Play(pulse);

            var sprite = await LoadSpriteAsync(cover, tokenSource.Token);
            var isFallback = sprite == null;

            if (isFallback) {
                sprite = await LoadSpriteAsync(fallback, tokenSource.Token);
            }

            if (tokenSource.IsCancellationRequested) {
                return;
            }

            _skeletonPlayer.Cancel();
            _isFallback.Value = isFallback;
            _sprite.Value = sprite;
        }

        private static async Task<Sprite?> LoadSpriteAsync(string? url, CancellationToken token) {
            if (string.IsNullOrEmpty(url) || url!.EndsWith(".webp")) {
                return null;
            }

            try {
                return (await ImageLoader.LoadImage(url, token))?.Sprite;
            } catch {
                return null;
            }
        }

        #endregion

        #region Construct

        private State<string?> _coverUrl = null!;
        private State<string?> _fallbackUrl = null!;
        private State<Sprite?> _sprite = null!;
        private State<bool> _isFallback = null!;
        private AnimationPlayer<IReactiveComponent> _skeletonPlayer = null!;

        protected override GameObject Construct() {
            _coverUrl = Remember<string?>(null);
            _fallbackUrl = Remember<string?>(null);
            _sprite = Remember<Sprite?>(null);
            _isFallback = Remember(false);

            var urls = RememberDerived(_coverUrl, _fallbackUrl, static (cover, fallback) => (cover, fallback));
            urls.AddCallback(this, static (x, y) => _ = x.LoadAsync(y.cover, y.fallback).RunCatching(), lazy: true);

            var alpha = RememberAnimated(_sprite.Map(static x => x != null ? 1f : 0f), 400.ms, AnimationCurve.EaseOut);
            var color = RememberDerived(alpha, _isFallback, static (x, y) => (Color.white * (y ? 0.35f : 0.6f)).ColorWithAlpha(x));

            return new Background {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    JustifyContent = Justify.Center
                },

                Sprite = BundleLoader.Sprites.background,
                PixelsPerUnit = 10f,

                Children = {
                    new Background {
                            FlexItem = {
                                PositionType = PositionType.Absolute,
                                Position = 0f
                            }
                        }
                        .AsBlurBackground()
                        .With(x => {
                            _skeletonPlayer = RememberAnimationPlayer<IReactiveComponent>(x);
                        }),

                    new Image {
                        FlexItem = {
                            Size = new() { x = 100.pct },
                            sAspectRatio = _sprite.Map(static x => x == null ? 1f : x.rect.width / x.rect.height)
                        },

                        sSprite = _sprite,
                        sColor = color
                    }
                }
            }.With(static x => {
                x.Content.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = false;
            }).Use();
        }

        protected override void OnDestroy() {
            _tokenSource?.Cancel();
        }

        #endregion
    }
}