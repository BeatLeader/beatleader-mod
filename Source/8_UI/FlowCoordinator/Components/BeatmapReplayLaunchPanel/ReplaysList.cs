using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using static BeatLeader.Models.LevelEndType;

namespace BeatLeader.UI.Hub {
    internal enum ReplaysListSorter {
        Difficulty,
        Player,
        Completion,
        Date
    }

    internal interface IReplaysList : IModifiableTableComponent<IReplayHeader> {
        ReplaysListSorter Sorter { get; set; }
        SortOrder SortOrder { get; set; }
    }

    internal class ReplaysList : Table<IReplayHeader, ReplaysList.Cell>, IReplaysList {
        #region Cell

        public class Cell : TableComponentCell<IReplayHeader> {
            #region Colors

            private static readonly StateColorSet colorSet = new() {
                States = {
                    GraphicState.None.WithColor(Color.clear),
                    GraphicState.Active.WithColor(new(0f, 0.75f, 1f)),
                    GraphicState.Hovered.WithColor(Color.white.ColorWithAlpha(0.2f)),
                    GraphicState.Hovered.And(GraphicState.Active).WithColor(new(0f, 0.75f, 1f, 0.75f))
                }
            };

            private static readonly StateColorSet highlightedColorSet = new() {
                States = {
                    GraphicState.None.WithColor(new(0f, 0.75f, 1f, 0.5f)),
                    GraphicState.Active.WithColor(new(0f, 0.75f, 1f)),
                    GraphicState.Hovered.WithColor(new(0f, 0.75f, 1f, 0.35f)),
                    GraphicState.Hovered.And(GraphicState.Active).WithColor(new(0f, 0.75f, 1f, 0.75f))
                }
            };

            public bool Highlighted {
                get => _highlighted;
                set {
                    _highlighted = value;
                    _button.GradientColors1 = value ? highlightedColorSet : colorSet;
                }
            }

            private bool _highlighted;

            #endregion

            #region Setup

            private ReplayManagerSearchTheme? _theme;
            private ReplaysList? _replaysList;

            public void Setup(ReplaysList replaysList, ReplayManagerSearchTheme? theme) {
                if (_theme != null) {
                    _theme.SearchThemeUpdatedEvent -= HandleThemeUpdated;
                }
                _replaysList = replaysList;
                _theme = theme;
                if (_theme != null) {
                    _theme.SearchThemeUpdatedEvent += HandleThemeUpdated;
                }
            }

            protected override void OnDestroy() {
                if (_theme != null) {
                    _theme.SearchThemeUpdatedEvent += HandleThemeUpdated;
                }
            }

            #endregion

            #region Texts

            public string? HighlightPhrase {
                set {
                    _highlightPhrase = value;
                    RefreshTexts();
                }
            }

            private string? _highlightPhrase;

            private string FormatByPhrase(string text) {
                if (_highlightPhrase == null) return text;
                var color = _theme?.SearchHighlightColor ?? Color.magenta;
                var phrase = _highlightPhrase;
                var bold = _theme?.SearchHighlightStyle.HasFlag(FontStyle.Bold) ?? false;
                return FormatUtils.MarkPhrase(text, phrase, color, bold);
            }

            private void RefreshTexts() {
                var info = Item.ReplayInfo;
                _topRightLabel.Text = FormatLevelEndType(info.LevelEndType, info.FailTime);
                _topLeftLabel.Text = FormatByPhrase(info.SongName);
                _bottomRightLabel.Text = FormatUtils.GetDateTimeString(info.Timestamp);
                _bottomLeftLabel.Text = $"[<color=#89ff89>{FormatByPhrase(info.SongDifficulty)}</color>] ";
                _bottomLeftLabel.Text += FormatByPhrase(info.PlayerName);
            }

            private static string FormatLevelEndType(LevelEndType levelEndType, float failTime) {
                return levelEndType switch {
                    Clear => "Completed",
                    Quit or Restart => "Unfinished",
                    Fail => $"Failed at {FormatUtils.FormatTime(failTime)}",
                    _ => "Unknown"
                };
            }

            #endregion

            #region Init

            private IReplayHeader? _prevItem;

            protected override void OnInit(IReplayHeader item) {
                RefreshTexts();
                RefreshTags();
                
                if (_prevItem != null) {
                    _prevItem.ReplayMetadata.TagAddedEvent -= HandleTagAddedOrRemoved;
                    _prevItem.ReplayMetadata.TagRemovedEvent -= HandleTagAddedOrRemoved;
                }
                item.ReplayMetadata.TagAddedEvent += HandleTagAddedOrRemoved;
                item.ReplayMetadata.TagRemovedEvent += HandleTagAddedOrRemoved;
                
                _prevItem = item;
            }

            #endregion

            #region Tags

            private readonly List<TagPanel> _spawnedTags = new();
            private int _maxTags = 3;

            private void RefreshTags() {
                if (_replaysList == null) {
                    return;
                }

                var tags = Item.ReplayMetadata.Tags;
                RefreshTagPanels(tags);

                var index = 0;
                foreach (var tag in tags) {
                    if (index == _maxTags) {
                        break;
                    }
                    var panel = _spawnedTags[index];
                    panel.Interactable = false;
                    panel.Animated = false;
                    panel.SetTag(tag);
                    panel.AsFlexItem(size: new() { y = 4f });
                    panel.LayoutDriver = _tagsContainer;
                    index++;
                }
            }

            private void RefreshTagPanels(ICollection<ReplayTag> tags) {
                var delta = tags.Count - _spawnedTags.Count;
                if (delta < 0) {
                    for (var i = -delta - 1; i >= 0; i--) {
                        var tag = _spawnedTags[i];
                        _replaysList!._tagsPool.Despawn(tag);
                        _spawnedTags.RemoveAt(i);
                    }
                } else if (delta > 0) {
                    for (var i = 0; i < delta; i++) {
                        if (_spawnedTags.Count == _maxTags) {
                            break;
                        }
                        var tag = _replaysList!._tagsPool.Spawn();
                        _spawnedTags.Add(tag);
                    }
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
            private ImageButton _button = null!;
            private Dummy _tagsContainer = null!;

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
                        Overflow = overflow,
                        Alignment = alignment,
                        FontStyle = FontStyles.Italic,
                        FontSize = fontSize,
                        Color = color
                    }.AsFlexItem(
                        size: new() { y = "100%", x = $"{size}%" },
                        position: frame
                    ).Bind(ref variable);
                }

                return new Dummy {
                    Children = {
                        new ImageButton {
                            Image = {
                                Sprite = BundleLoader.Sprites.background,
                                PixelsPerUnit = 10f,
                                Material = GameResources.UINoGlowAdditiveMaterial,
                                Skew = UIStyle.Skew,
                                Color = Color.white
                            },
                            GradientColors1 = colorSet,
                            Latching = true,
                            OnStateChanged = _ => SelectSelf(true),
                            Children = {
                                //top left
                                new Dummy {
                                    Children = {
                                        new Label {
                                            Overflow = TextOverflowModes.Ellipsis,
                                            Alignment = TextAlignmentOptions.TopLeft,
                                            FontStyle = FontStyles.Italic,
                                            FontSize = 4f,
                                            Color = textColor
                                        }.AsFlexItem(size: new() { x = "auto" }).Bind(ref _topLeftLabel),

                                        new Dummy()
                                            .AsFlexGroup(
                                                justifyContent: Justify.FlexStart,
                                                padding: new() { top = 0.7f, left = 0.7f },
                                                gap: new() { x = 0.5f }
                                            )
                                            .AsFlexItem(grow: 1f)
                                            .Bind(ref _tagsContainer)
                                    }
                                }.AsFlexGroup(alignItems: Align.Stretch).AsFlexItem(
                                    size: new() { y = "100%", x = "70%" },
                                    position: new() { top = 0, left = 0.6f }
                                ),

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
                        }.AsFlexGroup().AsFlexItem(
                            grow: 1f,
                            margin: new() { right = 1f }
                        ).Bind(ref _button)
                    }
                }.AsFlexGroup().WithSizeDelta(0f, 8f).Use();
            }

            #endregion

            #region Callbacks

            private void HandleTagAddedOrRemoved(ReplayTag tag) {
                RefreshTags();
            }

            private void HandleThemeUpdated() {
                RefreshTexts();
            }

            protected override void OnCellStateChange(bool selected) {
                _button.Click(selected, false);
            }

            #endregion
        }

        #endregion

        #region Sorting

        private class HeaderComparator : IComparer<IReplayHeader> {
            public ReplaysListSorter sorter;

            public int Compare(IReplayHeader x, IReplayHeader y) {
                var xi = x.ReplayInfo;
                var yi = y.ReplayInfo;
                return sorter switch {
                    ReplaysListSorter.Difficulty =>
                        -CompareLong(
                            (int)Enum.Parse(typeof(BeatmapDifficulty), xi.SongDifficulty),
                            (int)Enum.Parse(typeof(BeatmapDifficulty), yi.SongDifficulty)
                        ),
                    ReplaysListSorter.Player =>
                        string.CompareOrdinal(xi.PlayerName, yi.PlayerName),
                    ReplaysListSorter.Completion =>
                        CompareLong((int)xi.LevelEndType, (int)yi.LevelEndType),
                    ReplaysListSorter.Date =>
                        -CompareLong(xi.Timestamp, yi.Timestamp),
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
                Refresh();
            }
        }

        public SortOrder SortOrder {
            get => _sortOrder;
            set {
                _sortOrder = value;
                RefreshSorting();
                Refresh();
            }
        }

        private readonly HeaderComparator _headerComparator = new();
        private SortOrder _sortOrder;

        private void RefreshSorting() {
            Items.Sort(_headerComparator);
            if (_sortOrder is SortOrder.Descending) Items.Reverse();
        }

        protected override void OnEarlyRefresh() {
            RefreshSorting();
        }

        #endregion

        #region Setup

        public readonly HashSet<IReplayHeader> HighlightedItems = new();

        private readonly ReactivePool<TagPanel> _tagsPool = new();
        private ReplayManagerSearchTheme? _theme;

        public void Setup(ReplayManagerSearchTheme theme) {
            _theme = theme;
        }

        protected override void OnCellConstruct(Cell cell) {
            if (Filter is ITextTableFilter<IReplayHeader> filter) {
                cell.HighlightPhrase = filter.GetMatchedPhrase(cell.Item);
            } else {
                cell.HighlightPhrase = null;
            }
            cell.Setup(this, _theme);
            cell.Highlighted = HighlightedItems.Contains(cell.Item);
        }

        #endregion
    }
}