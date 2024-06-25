using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using HMUI;
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

    internal interface IReplaysList : IModifiableListComponent<IReplayHeader> {
        ReplaysListSorter Sorter { get; set; }
        SortOrder SortOrder { get; set; }
    }

    internal class ReplaysList : ReactiveListComponentBase<IReplayHeader, ReplaysList.Cell>, IReplaysList {
        #region Cell

        public class Cell : ReactiveTableCell<IReplayHeader> {
            #region Colors

            public bool UseAlternativeColors {
                set => ApplyColors(value);
            }

            private static readonly Color selectedColor = new(0f, 0.75f, 1f);
            private static readonly Color highlightedSelectedColor = selectedColor.ColorWithAlpha(0.75f);
            private static readonly Color highlightedColor = Color.white.ColorWithAlpha(0.2f);
            private static readonly Color idlingColor = Color.clear;

            private static readonly Color alternativeSelectedColor = new(1f, 0.63f, 0f);
            private static readonly Color alternativeHighlightedSelectedColor = alternativeSelectedColor.ColorWithAlpha(0.75f);
            private static readonly Color alternativeHighlightedColor = alternativeHighlightedSelectedColor;
            private static readonly Color alternativeIdlingColor = alternativeSelectedColor.ColorWithAlpha(0.5f);

            private Color _selectedColor;
            private Color _highlightedColor;
            private Color _highlightedSelectedColor;
            private Color _idlingColor;

            private void ApplyColors(bool alternative) {
                _selectedColor = alternative ? alternativeSelectedColor : selectedColor;
                _highlightedColor = alternative ? alternativeHighlightedColor : highlightedColor;
                _highlightedSelectedColor = alternative ? alternativeHighlightedSelectedColor : highlightedSelectedColor;
                _idlingColor = alternative ? alternativeIdlingColor : idlingColor;
            }

            private void RefreshColors(bool selected, bool highlighted) {
                _background.GradientColor1 = selected switch {
                    false => highlighted ? _highlightedColor : _idlingColor,
                    true => highlighted ? _highlightedSelectedColor : _selectedColor
                };
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
                return _highlightPhrase == null ? text : FormatUtils.MarkPhrase(text, _highlightPhrase);
            }

            private void RefreshTexts() {
                var info = Item!.ReplayInfo;
                _topRightLabel.Text = FormatLevelEndType(info.LevelEndType, info.FailTime);
                _topLeftLabel.Text = info.SongName;
                _bottomRightLabel.Text = FormatUtils.GetDateTimeString(info.Timestamp);
                _bottomLeftLabel.Text = $"[<color=#89ff89>{FormatByPhrase(info.SongDifficulty)}</color>] ";
                _bottomLeftLabel.Text += FormatByPhrase(info.PlayerName);
            }

            protected override void Init(IReplayHeader item) {
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
            private Image _background = null!;

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

                return new Button {
                    GrowOnHover = false,
                    Children = {
                        new Image {
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
                        }.AsFlexGroup().AsBackground(
                            material: GameResources.UINoGlowAdditiveMaterial,
                            gradientDirection: ImageView.GradientDirection.Horizontal
                        ).AsFlexItem(
                            grow: 1f,
                            margin: new() { right = 1f }
                        ).Bind(ref _background)
                    }
                }.AsFlexGroup().Use();
            }

            protected override void OnInitialize() {
                _background.Skew = 0.18f;
                ApplyColors(false);
            }

            #endregion

            #region Callbacks

            public override void OnCellStateChange(bool selected, bool highlighted) {
                RefreshColors(selected, highlighted);
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

        protected override float CellSize => 8;

        public readonly HashSet<IReplayHeaderBase> highlightedItems = new();

        protected override void OnCellConstruct(Cell cell) {
            if (Filter is ITextListFilter<IReplayHeader> filter) {
                cell.HighlightPhrase = filter.GetMatchedPhrase(cell.Item!);
            } else {
                cell.HighlightPhrase = null;
            }
            cell.UseAlternativeColors = highlightedItems.Contains(cell.Item!);
        }

        #endregion
    }
}