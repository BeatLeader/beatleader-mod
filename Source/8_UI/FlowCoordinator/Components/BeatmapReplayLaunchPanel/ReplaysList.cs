using System;
using System.Collections.Generic;
using System.Reflection;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
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

    internal class ReplaysList : ReeListComponentBase<ReplaysList, IReplayHeader, ReplaysList.Cell>, IReplaysList {
        #region Cell

        public class Cell : ReeTableCell<Cell, IReplayHeader> {
            #region UI Components

            [UIComponent("top-left")]
            private readonly TMP_Text _topLeftTextComponent = null!;

            [UIComponent("bottom-left")]
            private readonly TMP_Text _bottomLeftTextComponent = null!;

            [UIComponent("top-right")]
            private readonly TMP_Text _topRightTextComponent = null!;

            [UIComponent("bottom-right")]
            private readonly TMP_Text _bottomRightTextComponent = null!;

            [UIComponent("background")]
            private readonly ImageView _background = null!;

            #endregion

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

            public override void OnStateChange(bool selected, bool highlighted) {
                _background.color1 = selected switch {
                    false => highlighted ? _highlightedColor : _idlingColor,
                    true => highlighted ? _highlightedSelectedColor : _selectedColor
                };
            }

            private void ApplyColors(bool alternative) {
                _selectedColor = alternative ? alternativeSelectedColor : selectedColor;
                _highlightedColor = alternative ? alternativeHighlightedColor : highlightedColor;
                _highlightedSelectedColor = alternative ? alternativeHighlightedSelectedColor : highlightedSelectedColor;
                _idlingColor = alternative ? alternativeIdlingColor : idlingColor;
            }

            #endregion

            #region Setup

            protected override string Markup { get; } = BSMLUtility.ReadMarkupOrFallback(
                "ReplaysListCell", Assembly.GetExecutingAssembly()
            );
            
            protected override void Init(IReplayHeader item) {
                var info = item.ReplayInfo!;
                _topRightTextComponent.text = FormatLevelEndType(info.LevelEndType, info.FailTime);
                _topLeftTextComponent.text = info.SongName;
                _bottomRightTextComponent.text = FormatUtils.GetDateTimeString(info.Timestamp);
                _bottomLeftTextComponent.text = $"[<color=#89ff89>{info.SongDifficulty}</color>] {info.PlayerName}";
            }

            protected override void OnInitialize() {
                _background.sprite = BundleLoader.WhiteBG;
                _background.gradient = true;
                _background.material = GameResources.UINoGlowAdditiveMaterial;
                _background.SetField("_skew", 0.18f);
                _background.__Refresh();
                
                Content.GetComponentInParent<SelectableCell>().SetField("_wasPressedSignal", GameResources.ClickSignal);
                Content.AddComponent<Touchable>();
                ApplyColors(false);
                
                //when you finish a map it invokes the finish event and executes score sending and replay saving.
                //cell generation also happens there (on the game scene). as we know unity destroys objects after the scene transition
                //so we need to use DontDestroyOnLoad to keep this cell alive
                //DontDestroyOnLoad(Content);
                //DontDestroyOnLoad(gameObject);
            }

            #endregion

            #region Formatting

            private static string FormatLevelEndType(LevelEndType levelEndType, float failTime) {
                return levelEndType switch {
                    Clear => "Completed",
                    Quit or Restart => "Unfinished",
                    Fail => $"Failed at {FormatTime(Mathf.FloorToInt(failTime))}",
                    _ => "Unknown"
                };
            }

            private static string FormatTime(int seconds) {
                var minutes = seconds / 60;
                var hours = minutes / 60;
                var secDiv = seconds % 60;
                var minDiv = minutes % 60;
                return $"{(hours is not 0 ? $"{Zero(hours)}{hours}:" : "")}{Zero(minDiv)}{minDiv}:{Zero(secDiv)}{secDiv}";
                static string Zero(int number) => number > 9 ? "" : "0";
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
                return xi is null || yi is null ? 0 : sorter switch {
                    ReplaysListSorter.Difficulty =>
                        -CompareLong(
                            (int)StringConverter.Convert<BeatmapDifficulty>(xi.SongDifficulty),
                            (int)StringConverter.Convert<BeatmapDifficulty>(yi.SongDifficulty)
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
            items.Sort(_headerComparator);
            if (_sortOrder is SortOrder.Ascending) items.Reverse();
        }

        protected override void OnEarlyRefresh() {
            RefreshSorting();
        }

        #endregion

        #region Setup

        protected override float CellSize => 8;

        public readonly HashSet<IReplayHeaderBase> highlightedItems = new();

        protected override void OnCellConstruct(Cell cell) {
            cell.UseAlternativeColors = highlightedItems.Contains(cell.Item!);
        }

        #endregion
    }
}