using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Components.Basic;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using static BeatLeader.Models.LevelEndType;
using Background = Reactive.BeatSaber.Components.Background;
using Label = Reactive.BeatSaber.Components.Label;

namespace BeatLeader.UI.Hub {
    internal enum ReplaysListSorter {
        Difficulty,
        Player,
        Completion,
        Date
    }

    internal interface IReplaysList {
        ReplaysListSorter Sorter { get; set; }
        SortOrder SortOrder { get; set; }
    }

    internal class ReplaysList : ReactiveComponent, IReplaysList {
        #region Cell

        private class Cell : ReactiveComponent {
            #region Colors

            private static readonly ColorSet colorSet = new() {
                Color = Color.clear,
                ActiveColor = new(0f, 0.75f, 1f),
                HoveredColor = Color.white.ColorWithAlpha(0.2f),
                HoveredActiveColor = new(0f, 0.75f, 1f, 0.75f)
            };

            private static readonly ColorSet highlightedColorSet = new() {
                Color = new(0f, 0.75f, 1f, 0.5f),
                ActiveColor = new(0f, 0.75f, 1f),
                HoveredColor = new(0f, 0.75f, 1f, 0.35f),
                HoveredActiveColor = new(0f, 0.75f, 1f, 0.75f)
            };

            private State<GraphicState> _graphicState = null!;
            private State<bool> _highlighted = null!;

            #endregion

            #region Setup

            private readonly CellContext<IReplayHeader> _context;
            private readonly ReplaysList _replaysList;
            private IReplayHeader? _item;

            public Cell(CellContext<IReplayHeader> context, ReplaysList replaysList) {
                _context = context;
                _replaysList = replaysList;

                context.AddCallback(this, static (x, y) => x.HandleContextUpdated(y));
                replaysList._highlightedItems.AddCallback(this, static (x, _) => x.RefreshHighlight(), lazy: true);
                replaysList._filteredItems.AddCallback(this, static (x, _) => x.RefreshTexts(), lazy: true);
                replaysList._theme.AddCallback(this, static (x, _) => x.RefreshTexts(), lazy: true);
            }

            protected override void OnDestroy() {
                SetItem(null);
            }

            #endregion

            #region Texts

            private string FormatByPhrase(string text, string? phrase) {
                if (phrase == null) return text;
                var theme = _replaysList._theme.Value;
                var color = theme?.SearchHighlightColor ?? Color.magenta;
                var bold = theme?.SearchHighlightStyle.HasFlag(FontStyle.Bold) ?? false;
                return FormatUtils.MarkPhrase(text, phrase, color, bold);
            }

            private void RefreshTexts() {
                if (_item == null) return;
                var info = _item.ReplayInfo;
                var phrase = _replaysList.GetMatchedPhrase(_item);
                _topRightLabel.Text = FormatLevelEndType(info.LevelEndType, info.FailTime);
                _topLeftLabel.Text = FormatByPhrase(info.SongName, phrase);
                _bottomRightLabel.Text = FormatUtils.GetDateTimeString(info.Timestamp);
                _bottomLeftLabel.Text = $"[<color=#89ff89>{FormatByPhrase(info.SongDifficulty, phrase)}</color>] ";
                _bottomLeftLabel.Text += FormatByPhrase(info.PlayerName, phrase);
            }

            private static string FormatLevelEndType(LevelEndType levelEndType, float failTime) {
                return levelEndType switch {
                    Clear => "Completed",
                    Practice => "Practice",
                    Quit => "Quit",
                    Restart => "Restart",
                    Fail => $"Failed at {FormatUtils.FormatTime(failTime)}",
                    _ => "Unknown"
                };
            }

            #endregion

            #region Item

            private void SetItem(IReplayHeader? item) {
                if (_item != null) {
                    _item.ReplayMetadata.TagAddedEvent -= HandleTagAddedOrRemoved;
                    _item.ReplayMetadata.TagRemovedEvent -= HandleTagAddedOrRemoved;
                }
                _item = item;
                if (_item != null) {
                    _item.ReplayMetadata.TagAddedEvent += HandleTagAddedOrRemoved;
                    _item.ReplayMetadata.TagRemovedEvent += HandleTagAddedOrRemoved;
                }
            }

            private void RefreshHighlight() {
                _highlighted.Value = _item != null && _replaysList._highlightedItems.Value.Contains(_item);
            }

            #endregion

            #region Tags

            private const int MaxTags = 3;

            private readonly List<TagPanel> _spawnedTags = new();

            private void RefreshTags() {
                var tags = _item!.ReplayMetadata.Tags;
                RefreshTagPanels(Math.Min(tags.Count, MaxTags));

                var index = 0;
                foreach (var tag in tags) {
                    if (index == MaxTags) {
                        break;
                    }

                    var panel = _spawnedTags[index];
                    panel.Interactable = false;
                    panel.AnimateMovement = false;
                    panel.Animated = false;
                    panel.Tag = tag;
                    panel.IsPresent = true;

                    panel.FixedHeight = 4f;
                    if (panel.LayoutDriver != _tagsContainer) {
                        panel.LayoutDriver?.Children.Remove(panel);
                        _tagsContainer.Children.Add(panel);
                    }
                    panel.RefreshContainerSize(true);

                    index++;
                }
            }

            private void RefreshTagPanels(int count) {
                var pool = _replaysList._tagsPool;

                while (_spawnedTags.Count > count) {
                    var last = _spawnedTags.Count - 1;
                    pool.Despawn(_spawnedTags[last]);
                    _spawnedTags.RemoveAt(last);
                }

                while (_spawnedTags.Count < count) {
                    _spawnedTags.Add(pool.Spawn());
                }
            }

            #endregion

            #region Construct

            private static readonly Color textColor = Color.white;
            private static readonly Color secondaryTextColor = Color.white.ColorWithAlpha(0.75f);

            private Label _topLeftLabel = null!;
            private Label _bottomLeftLabel = null!;
            private Label _topRightLabel = null!;
            private Label _bottomRightLabel = null!;
            private Layout _tagsContainer = null!;

            protected override GameObject Construct() {
                static Label CellLabel(
                    TextOverflowModes overflow,
                    TextAlignmentOptions alignment,
                    float fontSize,
                    float size,
                    YogaFrame frame,
                    Color color,
                    ref Label variable
                ) {
                    return new Label {
                        FlexItem = {
                            Size = new() { y = 100.pct, x = size.pct },
                            Position = frame,
                            PositionType = PositionType.Absolute
                        },

                        Overflow = overflow,
                        Alignment = alignment,
                        FontStyle = FontStyles.Italic,
                        FontSize = fontSize,
                        Color = color
                    }.Bind(ref variable);
                }

                _graphicState = Remember(GraphicState.None);
                _highlighted = Remember(false);

                var gradientColor = RememberDerived(
                    _highlighted, _graphicState,
                    (highlighted, graphicState) => (highlighted ? highlightedColorSet : colorSet).GetColor(graphicState)
                );

                return new Layout {
                    FlexItem = {
                        Size = new() { y = CellHeight }
                    },

                    Children = {
                        new Background {
                                FlexItem = {
                                    FlexGrow = 1f,
                                    Margin = new() { right = 1f }
                                },

                                Sprite = BundleLoader.Sprites.background,
                                PixelsPerUnit = 10f,
                                Material = GameResources.UINoGlowAdditiveMaterial,
                                Skew = BeatSaberStyle.Skew,
                                Color = Color.white,
                                UseGradient = true,
                                sGradientColor1 = gradientColor,
                                Children = {
                                    //top left
                                    new Layout {
                                        FlexController = {
                                            AlignItems = Align.Stretch
                                        },

                                        FlexItem = {
                                            Size = new() { y = 100.pct, x = 70.pct },
                                            Position = new() { top = 0, left = 0.6f },
                                            PositionType = PositionType.Absolute
                                        },

                                        Children = {
                                            new Label {
                                                FlexItem = {
                                                    Size = new() { x = YogaValue.Auto }
                                                },

                                                Overflow = TextOverflowModes.Ellipsis,
                                                Alignment = TextAlignmentOptions.TopLeft,
                                                FontStyle = FontStyles.Italic,
                                                FontSize = 4f,
                                                Color = textColor
                                            }.Bind(ref _topLeftLabel),

                                            new Layout {
                                                FlexController = {
                                                    JustifyContent = Justify.FlexStart,
                                                    Padding = new() { top = 0.7f, left = 0.7f },
                                                    Gap = new() { x = 0.5f }
                                                },

                                                FlexItem = {
                                                    FlexGrow = 1f
                                                }
                                            }.Bind(ref _tagsContainer)
                                        }
                                    },

                                    //bottom left
                                    CellLabel(
                                        TextOverflowModes.Ellipsis,
                                        TextAlignmentOptions.BottomLeft,
                                        3,
                                        70,
                                        new() { bottom = 0, left = 0 },
                                        secondaryTextColor,
                                        ref _bottomLeftLabel
                                    ),
                                    //top right
                                    CellLabel(
                                        TextOverflowModes.Overflow,
                                        TextAlignmentOptions.TopRight,
                                        3,
                                        40,
                                        new() { top = 0, right = 1.5f },
                                        textColor,
                                        ref _topRightLabel
                                    ),
                                    //bottom right
                                    CellLabel(
                                        TextOverflowModes.Overflow,
                                        TextAlignmentOptions.BottomRight,
                                        3,
                                        30,
                                        new() { bottom = 0, right = 2 },
                                        secondaryTextColor,
                                        ref _bottomRightLabel
                                    )
                                }
                            }
                            .WithPointerEvents(
                                onEnter: _ => _graphicState.IsHovered = true,
                                onLeave: _ => _graphicState.IsHovered = false,
                                onDown: _ => {
                                    global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                                    _context.Selected = true;
                                }
                            )
                            .AsFlexGroup()
                    }
                }.AsFlexGroup().WithSizeDelta(0f, CellHeight).Use();
            }

            #endregion

            #region Callbacks

            private void HandleContextUpdated(CellContext<IReplayHeader> context) {
                _graphicState.IsActive = context.Selected;

                if (context.Item == _item) {
                    return;
                }

                SetItem(context.Item);
                RefreshTexts();
                RefreshTags();
                RefreshHighlight();
            }

            private void HandleTagAddedOrRemoved(ReplayTag tag) {
                RefreshTags();
            }

            #endregion
        }

        #endregion

        #region Sorting

        private class HeaderComparator : IComparer<IReplayHeader> {
            public ReplaysListSorter sorter;

            public int Compare(IReplayHeader? x, IReplayHeader? y) {
                if (x is not { ReplayInfo: var xi }) {
                    return 0;
                }

                if (y is not { ReplayInfo: var yi }) {
                    return 0;
                }

                return sorter switch {
                    ReplaysListSorter.Difficulty =>
                        -CompareLong(
                            (int)Enum.Parse(typeof(BeatmapDifficulty), xi.SongDifficulty),
                            (int)Enum.Parse(typeof(BeatmapDifficulty), yi.SongDifficulty)
                        ),
                    ReplaysListSorter.Player => string.CompareOrdinal(xi.PlayerName, yi.PlayerName),
                    ReplaysListSorter.Completion => CompareLong((int)xi.LevelEndType, (int)yi.LevelEndType),
                    ReplaysListSorter.Date => -CompareLong(xi.Timestamp, yi.Timestamp),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            private static int CompareLong(long x, long y) => Comparer<long>.Default.Compare(x, y);
        }

        public ReplaysListSorter Sorter {
            get => _headerComparator.sorter;
            set {
                _headerComparator.sorter = value;
                RefreshSorting();
            }
        }

        public SortOrder SortOrder {
            get => _sortOrder;
            set {
                _sortOrder = value;
                RefreshSorting();
            }
        }

        private readonly HeaderComparator _headerComparator = new();
        private SortOrder _sortOrder;

        private List<IReplayHeader> _sortedItems = new();

        private void RefreshSorting() {
            var sorted = new List<IReplayHeader>(_items);
            sorted.Sort(_headerComparator);

            if (_sortOrder is SortOrder.Descending) {
                sorted.Reverse();
            }

            _sortedItems = sorted;
            Refresh();
        }

        #endregion

        #region Items

        public IReadOnlyList<IReplayHeader> Items {
            get => _items;
            set {
                _items = value;
                RefreshSorting();
            }
        }

        public ITableFilter<IReplayHeader>? Filter {
            get => _filter;
            set {
                if (_filter != null) {
                    _filter.FilterUpdatedEvent -= Refresh;
                }
                _filter = value;
                if (_filter != null) {
                    _filter.FilterUpdatedEvent += Refresh;
                }
                Refresh();
            }
        }

        private IReadOnlyList<IReplayHeader> _items = Array.Empty<IReplayHeader>();
        private ITableFilter<IReplayHeader>? _filter;

        private readonly State<IReadOnlyList<IReplayHeader>> _filteredItems = new(Array.Empty<IReplayHeader>());

        public void Refresh() {
            var items = _filter.Apply(_sortedItems);

            _filteredItems.Value = items;
            _table.Items = items;
        }

        private string? GetMatchedPhrase(IReplayHeader item) {
            return (_filter as ITextTableFilter<IReplayHeader>)?.GetMatchedPhrase(item);
        }

        private int IndexOf(IReplayHeader item) {
            var items = _filteredItems.Value;
            for (var i = 0; i < items.Count; i++) {
                if (items[i] == item) {
                    return i;
                }
            }
            return -1;
        }

        #endregion

        #region Selection

        public Action<IReadOnlyCollection<IReplayHeader>>? OnSelectedItemsChanged { get; set; }

        public bool Select(IReplayHeader item) {
            if (IndexOf(item) == -1) {
                return false;
            }
            _table.SelectedItems = [item];
            return true;
        }

        public bool ScrollTo(IReplayHeader item, bool immediately = false) {
            var index = IndexOf(item);
            if (index == -1) {
                return false;
            }
            _scrollContext.ScrollTo(index * _scrollContext.LineSize, immediately);
            return true;
        }

        #endregion

        #region Highlighting

        public IReadOnlyCollection<IReplayHeader> HighlightedItems => _highlightedItems.Value;

        private readonly State<HashSet<IReplayHeader>> _highlightedItems = new(new HashSet<IReplayHeader>());

        public void SetItemHighlighted(IReplayHeader item, bool highlighted) {
            var items = _highlightedItems.Value;
            var changed = highlighted ? items.Add(item) : items.Remove(item);

            if (changed) {
                _highlightedItems.EnforceInvalidation();
            }
        }

        #endregion

        #region Setup

        private const float CellHeight = 8f;

        public ScrollContext ScrollContext => _scrollContext;

        public int VisibleCells => (int)(ContentTransform.rect.height / CellHeight);

        private readonly ScrollContext _scrollContext = new();
        private readonly ReactivePool<TagPanel> _tagsPool = new();
        private readonly State<ReplayManagerSearchTheme?> _theme = new(null);

        public void Setup(ReplayManagerSearchTheme theme) {
            if (_theme.Value != null) {
                _theme.Value.SearchThemeUpdatedEvent -= HandleThemeUpdated;
            }
            _theme.Value = theme;
            theme.SearchThemeUpdatedEvent += HandleThemeUpdated;
        }

        protected override void OnDestroy() {
            if (_theme.Value != null) {
                _theme.Value.SearchThemeUpdatedEvent -= HandleThemeUpdated;
            }
            if (_filter != null) {
                _filter.FilterUpdatedEvent -= Refresh;
            }
        }

        private void HandleThemeUpdated() {
            _theme.EnforceInvalidation();
        }

        #endregion

        #region Construct

        private Table<IReplayHeader, Cell> _table = null!;

        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    new Table<IReplayHeader, Cell> {
                        FlexItem = {
                            FlexGrow = 1f
                        },

                        ScrollContext = _scrollContext,
                        Items = Array.Empty<IReplayHeader>(),
                        ConstructCell = ctx => new Cell(ctx, this),
                        OnSelectedItemsChanged = x => OnSelectedItemsChanged?.Invoke(x)
                    }.Bind(ref _table),

                    new Label {
                        FlexItem = {
                            Position = 0f,
                            PositionType = PositionType.Absolute
                        },

                        Text = "The monkey left you on your own!",
                        FontSize = 3.2f,
                        FontSizeMin = 1f,
                        FontSizeMax = 5f,
                        EnableAutoSizing = true,
                        EnableWrapping = true,
                        RaycastTarget = false,
                        sEnabled = _filteredItems.Map(static x => x.Count == 0)
                    }
                }
            }.AsFlexGroup().Use();
        }

        #endregion
    }
}