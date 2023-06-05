using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ReplaysList : ReeUIComponentV2, TableView.IDataSource {
        #region Cells Pool

        private static readonly IReplayHeader emptyReplayHeader =
            new GenericReplayHeader(null!, string.Empty, default(ReplayInfo?));


        [UsedImplicitly]
        public class DataCellsMemoryPool : StaticMemoryPool<DataCellsMemoryPool, AbstractDataCell> {
            public const int DefaultReservedCellsCount = 500;

            public override void Expand(int count) {
                for (var i = 0; i < count; i++) {
                    items.Push(AbstractDataCell.Create<ReplayDataCell>(emptyReplayHeader));
                }
            }
        }

        #endregion

        #region Cells

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        public abstract class AbstractDataCell : TableCell {
            #region Config

            public const int CellHeight = 8;

            private const string MarkupPath = Plugin.ResourcesPath +
                ".BSML.FlowCoordinator.Components.ReplayLaunchView.BeatmapReplayLaunchPanel.ReplaysList.ReplaysListCell.bsml";

            #endregion

            #region Resources

            private static readonly string markup = Utilities
                .GetResourceContent(typeof(AbstractDataCell).Assembly, MarkupPath);

            private static readonly Material noGlowMaterial = Resources.FindObjectsOfTypeAll
                <Material>().FirstOrDefault(x => x.name == "UINoGlowAdditive")!;

            private static readonly Sprite backgroundSprite = Resources.FindObjectsOfTypeAll
                <Sprite>().FirstOrDefault(x => x.name == "RoundRect10Thin")!;

            #endregion

            #region Data

            public IReplayHeader? ReplayHeader { get; private set; } = null!;

            #endregion

            #region Construction

            private bool _isInitialized;

            public static T Create<T>(IReplayHeader header) where T : AbstractDataCell {
                if (header is null) throw new ArgumentNullException(nameof(header));
                var instance = new GameObject(nameof(AbstractDataCell)).AddComponent<T>();
                instance.Init(header);
                return instance;
            }

            public void MakeReusable() {
                interactable = false;
                ReplayHeader = null;
            }

            public AbstractDataCell Init(IReplayHeader header) {
                interactable = true;
                ReplayHeader = header;
                if (!_isInitialized) {
                    PersistentSingleton<BSMLParser>.instance.Parse(markup, gameObject, this);
                    gameObject.AddComponent<Touchable>();
                    name = nameof(AbstractDataCell);
                    reuseIdentifier = name;
                    _isInitialized = true;
                }
                OnConstruct();
                return this;
            }

            protected virtual void OnConstruct() { }

            #endregion

            #region Colors

            protected abstract Color HighlightColor { get; }
            protected abstract Color HighlightSelectedColor { get; }
            protected abstract Color SelectColor { get; }
            protected abstract Color IdlingColor { get; }

            #endregion

            #region UI

            public string? TopLeftText {
                get => _topLeftTextComponent.text;
                set => _topLeftTextComponent.text = value;
            }

            protected string? BottomLeftText {
                get => _bottomLeftTextComponent.text;
                set => _bottomLeftTextComponent.text = value;
            }

            protected string? TopRightText {
                get => _topRightTextComponent.text;
                set => _topRightTextComponent.text = value;
            }

            protected string? BottomRightText {
                get => _bottomRightTextComponent.text;
                set => _bottomRightTextComponent.text = value;
            }

            [UIComponent("top-left")]
            protected readonly TMP_Text _topLeftTextComponent = null!;

            [UIComponent("bottom-left")]
            protected readonly TMP_Text _bottomLeftTextComponent = null!;

            [UIComponent("top-right")]
            protected readonly TMP_Text _topRightTextComponent = null!;

            [UIComponent("bottom-right")]
            protected readonly TMP_Text _bottomRightTextComponent = null!;

            [UIComponent("background")]
            protected readonly ImageView background = null!;

            protected override void HighlightDidChange(TransitionType transitionType) {
                RefreshVisuals();
            }

            protected override void SelectionDidChange(TransitionType transitionType) {
                RefreshVisuals();
            }

            protected void RefreshVisuals() {
                background.color1 = selected switch {
                    false => highlighted ? HighlightColor : IdlingColor,
                    true => highlighted ? HighlightSelectedColor : SelectColor
                };
            }

            [UIAction("#post-parse")]
            protected void HandlePostParse() {
                background.sprite = backgroundSprite;
                background.gradient = true;
                background.material = noGlowMaterial;
                background.SetField("_skew", 0.18f);
                background.__Refresh();
            }

            #endregion
        }

        [UsedImplicitly]
        private class ReplayDataCell : AbstractDataCell {
            static ReplayDataCell() {
                ColorUtility.TryParseHtmlString("#00C0FFFF", out selectColor);
                highlightSelectedColor = selectColor.ColorWithAlpha(0.75f);
            }

            private static readonly Color selectColor;
            private static readonly Color highlightSelectedColor;
            private static readonly Color highlightColor = Color.white.ColorWithAlpha(0.2f);
            private static readonly Color idlingColor = Color.clear;

            public bool ShowBeatmapName {
                set {
                    if (ReplayHeader?.Info is not { } info) return;
                    TopLeftText = value ? info.songName : MakeDiff(info.difficulty);
                    BottomLeftText = $"{(value ? MakeDiff(info.difficulty) : string.Empty)} {info.playerName}";
                    static string MakeDiff(string diff) => $"[<color=#89ff89>{diff}</color>]";
                }
            }

            protected override void OnConstruct() {
                if (ReplayHeader!.Status is ReplayStatus.Corrupted
                    || ReplayHeader.Info is not { } info) return;
                ShowBeatmapName = false;
                TopRightText = info.failTime == 0 ? "Completed" : "Failed";
                BottomRightText = FormatUtils.GetDateTimeString(info.timestamp);
            }

            #region Colors

            protected override Color HighlightColor => highlightColor;
            protected override Color HighlightSelectedColor => highlightSelectedColor;
            protected override Color SelectColor => selectColor;
            protected override Color IdlingColor => idlingColor;

            #endregion
        }

        [UsedImplicitly]
        private class CorruptedReplayDataCell : AbstractDataCell {
            private static readonly Color selectColor = Color.red;
            private static readonly Color highlightSelectedColor = selectColor.ColorWithAlpha(0.75f);
            private static readonly Color highlightColor = selectColor.ColorWithAlpha(0.5f);
            private static readonly Color idlingColor = selectColor.ColorWithAlpha(0.3f);

            protected override void OnConstruct() {
                const string UNKNOWN = "Unknown";
                TopLeftText = "Corrupted file";
                BottomLeftText = Path.GetFileNameWithoutExtension(ReplayHeader!.FilePath);
                TopRightText = UNKNOWN;
                BottomRightText = UNKNOWN;
            }

            #region Colors

            protected override Color HighlightColor => highlightColor;
            protected override Color HighlightSelectedColor => highlightSelectedColor;
            protected override Color SelectColor => selectColor;
            protected override Color IdlingColor => idlingColor;

            #endregion
        }

        #endregion

        #region Events

        public event Action<AbstractDataCell?>? ReplaySelectedEvent;
        public event Action<bool>? ShowEmptyScreenChangedEvent;

        #endregion

        #region UI Components

        [UIComponent("list")]
        private readonly CustomCellListTableData _replaysList = null!;

        [UIObject("empty-text")]
        private readonly GameObject _emptyTextObject = null!;

        #endregion

        #region Init

        protected override void OnInitialize() {
            _reusableCells.AddRange(DataCellsMemoryPool.Instance
                .Shrink(DataCellsMemoryPool.DefaultReservedCellsCount));
            _tableView = _replaysList.tableView;
            _tableView.SetDataSource(this, true);
            _tableView.didSelectCellWithIdxEvent += HandleCellSelected;
            ShowEmptyScreen(false);
            Refresh();
        }

        #endregion

        #region TableView

        public List<AbstractDataCell> ActualCells { get; } = new();
        public List<AbstractDataCell> Cells { get; } = new();

        private readonly List<AbstractDataCell> _reusableCells = new();

        private TableView _tableView = null!;

        float TableView.IDataSource.CellSize() => AbstractDataCell.CellHeight;

        int TableView.IDataSource.NumberOfCells() => Cells.Count;

        TableCell TableView.IDataSource.CellForIdx(TableView tableView, int idx) {
            return idx >= Cells.Count ? AbstractDataCell.Create<CorruptedReplayDataCell>(emptyReplayHeader) : Cells[idx];
        }

        private AbstractDataCell? GetReusableCell<T>() where T : AbstractDataCell {
            var cellIdx = _reusableCells.FindIndex(x => x is T);
            if (cellIdx == -1) return null;
            var cell = _reusableCells[cellIdx];
            _reusableCells.RemoveAt(cellIdx);
            return cell;
        }

        private AbstractDataCell AddCell<T>(IReplayHeader header) where T : AbstractDataCell {
            var cell = GetReusableCell<T>()?.Init(header) ?? AbstractDataCell.Create<T>(header);
            ActualCells.Add(cell);
            Cells.Add(cell);
            return cell;
        }

        private void AddReusableCell(AbstractDataCell cell) {
            _reusableCells.Add(cell);
            cell.MakeReusable();
        }

        public void ShowEmptyScreen(bool show) {
            if (!show && Cells.Count == 0) return;
            _replaysList.gameObject.SetActive(!show);
            _emptyTextObject.SetActive(show);
            ShowEmptyScreenChangedEvent?.Invoke(show);
        }

        #endregion

        #region Data

        public void SetData(IEnumerable<IReplayHeader>? headers = null, bool showBeatmapNameIfCorrect = true) {
            if (ActualCells.Count != 0) ClearData();
            if (headers is null) return;
            foreach (var header in headers) {
                AddReplay(header, showBeatmapNameIfCorrect);
            }
            Refresh();
        }

        public void RemoveReplay(IReplayHeader header) {
            var cell = ActualCells.FirstOrDefault(x => x.ReplayHeader == header);
            if (cell is null || !ActualCells.Remove(cell) || !Cells.Remove(cell)) return;
            Refresh();
        }

        public void AddReplay(IReplayHeader header, bool showBeatmapNameIfCorrect = true) {
            ShowEmptyScreen(false);
            if (header.Status is ReplayStatus.Corrupted || AddCell<ReplayDataCell>(header) is not ReplayDataCell cell) {
                AddCell<CorruptedReplayDataCell>(header);
                return;
            }
            cell.ShowBeatmapName = showBeatmapNameIfCorrect;
        }

        private void ClearData() {
            ActualCells.ForEach(AddReusableCell);
            ActualCells.Clear();
            Cells.Clear();
            _tableView.ClearSelection();
            ShowEmptyScreen(true);
            ReplaySelectedEvent?.Invoke(null);
            Refresh();
        }

        public void Refresh() {
            _tableView.ClearSelection();
            _tableView.ReloadData();
            ShowEmptyScreen(Cells.Count == 0);
        }

        #endregion

        #region Callbacks

        private void HandleCellSelected(TableView view, int cellIdx) {
            ReplaySelectedEvent?.Invoke(Cells[cellIdx]);
        }

        #endregion
    }
}