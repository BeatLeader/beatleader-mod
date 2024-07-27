using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using BeatLeader.Utils;
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

    internal interface IReplaysList : IModifiableTableComponent<IReplayHeaderBase> {
        ReplaysListSorter Sorter { get; set; }
        SortOrder SortOrder { get; set; }
    }

    internal class ReplaysList : Table<IReplayHeaderBase, ReplaysList.Cell>, IReplaysList {
        #region Cell

        public class Cell : TableComponentCell<IReplayHeaderBase> {
            #region Colors

            private static readonly StateColorSet colorSet = new() {
                Color = Color.clear,
                ActiveColor = new(0f, 0.75f, 1f),
                HoveredColor = Color.white.ColorWithAlpha(0.2f),
                HoveredActiveColor = new(0f, 0.75f, 1f, 0.75f)
            };

            private static readonly StateColorSet highlightedColorSet = new() {
                Color = new(0f, 0.75f, 1f, 0.5f),
                ActiveColor = new(0f, 0.75f, 1f),
                HoveredColor = new(0f, 0.75f, 1f, 0.35f),
                HoveredActiveColor = new(0f, 0.75f, 1f, 0.75f)
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

            public void Setup(ReplayManagerSearchTheme? theme) {
                if (_theme != null) {
                    _theme.SearchThemeUpdatedEvent -= HandleThemeUpdated;
                }
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

            protected override void OnInit(IReplayHeaderBase item) {
                RefreshTexts();
            }

            #endregion

            #region Text Formatting

            private static string FormatLevelEndType(LevelEndType levelEndType, float failTime) {
                return levelEndType switch {
                    Clear => "Completed",
                    Quit or Restart => "Unfinished",
                    Fail => $"Failed at {FormatUtils.FormatTime(failTime)}",
                    _ => "Unknown"
                };
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
                            Colors = null,
                            GradientColors1 = colorSet,
                            GrowOnHover = false,
                            HoverLerpMul = float.MaxValue,
                            Sticky = true,
                            Children = {
                                //top left
                                CellLabel(
                                    TextOverflowModes.Ellipsis,
                                    TextAlignmentOptions.TopLeft,
                                    4,
                                    60,
                                    new() { top = 0, left = 0.6f },
                                    textColor,
                                    ref _topLeftLabel
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
                        }.WithStateListener(_ => SelectSelf(true)).AsFlexGroup().AsFlexItem(
                            grow: 1f,
                            margin: new() { right = 1f }
                        ).Bind(ref _button)
                    }
                }.AsFlexGroup().WithSizeDelta(0f, 8f).Use();
            }

            #endregion

            #region Callbacks

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

        private class HeaderComparator : IComparer<IReplayHeaderBase> {
            public ReplaysListSorter sorter;

            public int Compare(IReplayHeaderBase x, IReplayHeaderBase y) {
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

        private ReplayManagerSearchTheme? _theme;

        public void Setup(ReplayManagerSearchTheme theme) {
            _theme = theme;
        }

        public readonly HashSet<IReplayHeaderBase> HighlightedItems = new();

        protected override void OnCellConstruct(Cell cell) {
            if (Filter is ITextTableFilter<IReplayHeaderBase> filter) {
                cell.HighlightPhrase = filter.GetMatchedPhrase(cell.Item);
            } else {
                cell.HighlightPhrase = null;
            }
            cell.Setup(_theme);
            cell.Highlighted = HighlightedItems.Contains(cell.Item);
        }

        #endregion
    }
}