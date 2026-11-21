using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using UnityEngine;
using AnimationCurve = Reactive.AnimationCurve;

namespace BeatLeader.UI.Hub {
    internal abstract class BasicReplayDetailPanel : ReactiveComponent, BeatmapReplayLaunchPanel.IDetailPanel {
        #region Construct

        private ReplayStatisticsPanel _replayStatisticsPanel = null!;
        private QuickMiniProfile _miniProfile = null!;
        private TagsStrip _tagsStrip = null!;
        private State<bool> _detailsVisible = null!;
        private State<bool> _tagsEditorOpened = null!;

        protected sealed override GameObject Construct() {
            _detailsVisible = Remember(false);
            _tagsEditorOpened = Remember(false);

            var detailsAlpha = RememberAnimated(0f, 15.fact);
            _detailsVisible.AddCallback(x => detailsAlpha.TargetValue = x ? 1f : 0f, lazy: true);

            var tagsEditorOpenProgress = RememberAnimated(0f, 300.ms, AnimationCurve.EaseOutQuint);
            _tagsEditorOpened.AddCallback(x => tagsEditorOpenProgress.TargetValue = x ? 1f : 0f, lazy: true);

            return new Layout {
                Children = {
                    new Layout {
                        FlexController = {
                            FlexDirection = FlexDirection.Column,
                            Gap = new() { y = 0.5f },
                        },

                        Children = {
                            new QuickMiniProfile {
                                FlexItem = {
                                    Size = new() { y = 16f },
                                },
                            }.Bind(ref _miniProfile),

                            new TagsStrip {
                                FlexItem = {
                                    Size = new() { y = 5f },
                                },

                                ShowEditButton = AllowTagsEdit,
                                sEditorOpened = _tagsEditorOpened,
                                OnEditClicked = () => _tagsEditorOpened.Value = !_tagsEditorOpened.Value,
                            }.Bind(ref _tagsStrip),

                            new Layout {
                                FlexController = {
                                    FlexDirection = FlexDirection.Column,
                                },

                                FlexItem = {
                                    Size = new() { y = 37f },
                                    Margin = new() { bottom = 1f },
                                },

                                Children = {
                                    new ReplayStatisticsPanel {
                                            FlexItem = {
                                                FlexGrow = 1f,
                                                // Gives way much less than the editor comes in, as if there is no room to move
                                                sPosition = tagsEditorOpenProgress.Map(static x => new YogaFrame { top = (x * 30f).pct }),
                                            },

                                            ContentTransform = {
                                                pivot = new(0.5f, 0f),
                                                slocalScale = tagsEditorOpenProgress.Map(static x => Vector3.one * Mathf.Lerp(1f, 0.75f, x)),
                                            },
                                        }
                                        .WithAlpha(tagsEditorOpenProgress.Map(static x => 1f - x))
                                        .Bind(ref _replayStatisticsPanel),

                                    new TagSelector {
                                        FlexItem = {
                                            PositionType = PositionType.Absolute,
                                            Size = new() { y = 100.pct },

                                            // Kept above the visible area until it slides in
                                            sPosition = tagsEditorOpenProgress.Map(static x => new YogaFrame {
                                                top = ((x - 1f) * 100f).pct,
                                                left = 0f,
                                                right = 0f,
                                            }),
                                        },

                                        OnSelectedTagAdded = tag => Header?.ReplayMetadata.Tags.Add(tag),
                                        OnSelectedTagRemoved = tag => Header?.ReplayMetadata.Tags.Remove(tag),
                                    }.On(
                                        _tagsEditorOpened,
                                        (x, opened) => {
                                            if (opened && Header != null) {
                                                x.SelectedTags = Header.ReplayMetadata.Tags;
                                            }
                                        },
                                        lazy: true
                                    )
                                },
                            }.AsRectMask(),

                            ConstructButtons(),
                        }
                    }.WithAlpha(detailsAlpha).WithRectExpand(),

                    new Label {
                        Name = "EmptyLabel",

                        Text = EmptyText,
                        RaycastTarget = false,
                        EnableWrapping = true,
                        sColor = detailsAlpha.Map(static x => Color.white.ColorWithAlpha(1f - x))
                    }.WithRectExpand()
                }
            }.Use();
        }

        protected abstract ILayoutItem ConstructButtons();

        #endregion

        #region Setup

        protected abstract bool AllowTagsEdit { get; }
        protected abstract string EmptyText { get; }

        public virtual void Setup(IBeatmapReplayLaunchPanel? launchPanel) { }

        #endregion

        #region Data

        protected IReplayHeader? Header { get; private set; }
        protected CancellationToken CancellationToken => _tokenSource.Token;

        private CancellationTokenSource _tokenSource = new();
        private string? _lastPlayerId;

        public void SetData(IReplayHeader? header) {
            if (header == Header) return;
            _tagsEditorOpened.Value = false;
            _tokenSource.Cancel();
            _tokenSource = new();
            SetDataAsync(header, _tokenSource.Token).RunCatching();
        }

        private async Task SetDataAsync(IReplayHeader? header, CancellationToken token) {
            Header = header;
            _detailsVisible.Value = header != null;

            if (header == null) {
                _miniProfile.Player = null;
                _miniProfile.IsLoading = false;
                _lastPlayerId = null;
            } else {
                //starting tasks
                _tagsStrip.Metadata = header.ReplayMetadata;
                var playerTask = RefreshPlayerAsync(header, token);
                var statsTask = RefreshStatsAsync(header, token);
                var setDataTask = SetDataInternalAsync(header, token);
                //awaiting tasks
                await playerTask;
                await statsTask;
                await setDataTask;
            }
        }

        protected virtual Task SetDataInternalAsync(IReplayHeader header, CancellationToken token) {
            return Task.CompletedTask;
        }

        private async Task RefreshStatsAsync(IReplayHeader header, CancellationToken token) {
            _replayStatisticsPanel.SetLoading();

            await _replayStatisticsPanel.SetDataByHeaderAsync(header, token);
        }

        private async Task RefreshPlayerAsync(IReplayHeader header, CancellationToken token) {
            if (_lastPlayerId == header.ReplayInfo.PlayerID) {
                return;
            }

            _miniProfile.IsLoading = true;

            var player = await header.LoadPlayerAsync(false, token) as IPlayer;

            _miniProfile.Player = player;
            _miniProfile.IsLoading = false;
            _lastPlayerId = player.Id;
        }

        #endregion
    }
}