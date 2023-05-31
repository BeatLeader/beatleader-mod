using System;
using System.Collections;
using System.Linq;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class ReplayLaunchPanel : ReeUIComponentV2 {
        #region Init / Dispose

        protected override void OnInstantiate() {
            _replayPanel = Instantiate<ReplayPanel>(transform);
            _replayPanel.followMenuLoaderDownloadState = false;
        }

        protected override void OnInitialize() {
            InitializeReplayPanel();
            InitializeReplayList();
            LeaderboardState.AddSelectedBeatmapListener(OnSelectedBeatmapChanged);
            LeaderboardEvents.ReplayLaunchPanelButtonWasPressedEvent += ShowModal;
            LeaderboardEvents.HideAllOtherModalsEvent += OnHideModalsEvent;
            LeaderboardState.IsVisibleChangedEvent += OnLeaderboardVisibilityChanged;
        }

        protected override void OnDispose() {
            LeaderboardEvents.ReplayLaunchPanelButtonWasPressedEvent -= ShowModal;
            LeaderboardEvents.HideAllOtherModalsEvent -= OnHideModalsEvent;
            LeaderboardState.IsVisibleChangedEvent -= OnLeaderboardVisibilityChanged;
        }

        #endregion

        #region Events

        private void OnSelectedBeatmapChanged(bool selectedAny, LeaderboardKey leaderboardKey, IDifficultyBeatmap beatmap) {
            StopReplaysLoading();
            _isLeaderboardChanged = true;
            _previewBeatmap = beatmap.level;
        }

        private void OnHideModalsEvent(ModalView except) {
            if (_modal == null || _modal.Equals(except)) return;
            _modal.Hide(false);
        }

        private void OnLeaderboardVisibilityChanged(bool isVisible) {
            if (!isVisible) HideAnimated();
        }

        private void OnModalShown() {
            DeselectCellReplayList();
            ResetReplayPanel();
            if (!_isLeaderboardChanged) return;
            ReloadReplays();
            _isLeaderboardChanged = false;
        }

        #endregion

        #region Modal

        [UIComponent("modal")]
        private readonly ModalView _modal = null!;

        private bool _isLeaderboardChanged = true;

        private void ShowModal() {
            if (_modal == null) return;
            LeaderboardEvents.FireHideAllOtherModalsEvent(_modal);
            _modal.Show(true, true);
            OnModalShown();
        }

        private void HideAnimated() {
            if (_modal == null) return;
            _modal.Hide(true);
        }

        #endregion

        #region ReplayPanel

        [UIValue("replay-panel"), UsedImplicitly]
        private ReplayPanel _replayPanel = null!;

        private void InitializeReplayPanel() {
            ResetReplayPanel();
        }

        private void ResetReplayPanel() {
            _replayPanel.SetReplayPath(null);
        }

        #endregion

        #region ReplayList

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class ReplayDataCell {
            private static readonly Material noGlowMaterial = Resources.FindObjectsOfTypeAll
                <Material>().FirstOrDefault(x => x.name == "UINoGlowAdditive")!;

            #region Data

            [UIValue("player-name")]
            public string? playerName;

            [UIValue("result")]
            public string? result;

            [UIValue("date")]
            public string? date;

            public string? filePath;

            #endregion

            #region UI

            [UIComponent("background")]
            private readonly ImageView _background = null!;

            [UIAction("refresh-visuals")]
            private void Refresh(bool selected, bool highlighted) {
                _background.color1 = (selected switch {
                    false => highlighted ? Color.grey : Color.clear,
                    true => Color.magenta
                }).ColorWithAlpha(0.9f);
            }

            [UIAction("#post-parse")]
            private void OnPostParse() {
                _background.sprite = BundleLoader.WhiteBG;
                _background.gradient = true;
                _background.material = noGlowMaterial;
            }

            #endregion
        }

        #region Components

        [UIComponent("replay-list")]
        private readonly CustomCellListTableData _replayList = null!;

        [UIComponent("replay-list")]
        private readonly RectTransform _replayListRect = null!;

        [UIComponent("replay-list-container")]
        private readonly RectTransform _replayListContainerRect = null!;

        [UIComponent("loading-container")]
        private readonly ImageView _loadingContainerBackground = null!;

        [UIObject("loading-container")]
        private readonly GameObject _loadingContainerObject = null!;

        [UIObject("replay-list")]
        private readonly GameObject _replayListObject = null!;

        [UIObject("empty-text")]
        private readonly GameObject _emptyTextObject = null!;

        [UIObject("refresh-button")]
        private readonly GameObject _refreshButtonObject = null!;

        #endregion

        #region UI

        [UIValue("max-cells-count")]
        private const int MaxCellsCount = 4;

        [UIValue("cell-height")]
        private const float Height = 10.3f;

        private void InitializeReplayList() {
            _loadingContainerBackground.sprite = BundleLoader.TransparentPixel;
            _loadingContainerBackground.color = Color.grey.ColorWithAlpha(0.95f);
        }

        private void SetReplayListLoadingState(bool loading) {
            _loadingContainerObject.SetActive(loading);
            _refreshButtonObject.SetActive(!loading);
            _replayList.clickableCells = !loading;
            ReloadReplayListCells();
            if (loading) return;
            SetReplayListEmptinessState(_replayList.data.Count == 0);
        }

        private void SetReplayListEmptinessState(bool empty) {
            _emptyTextObject.SetActive(empty);
            _replayListObject.SetActive(!empty);
        }

        private void DeselectCellReplayList() {
            _replayList.tableView.ClearSelection();
        }

        private void ReloadReplays() {
            Plugin.Log.Notice("Loading local replays...");
            ClearReplayList();
            SetReplayListEmptinessState(false);
            SetReplayListLoadingState(true);
            StartReplayInfosEnumerationCoroutine((info, path) => {
                if (!IsValidReplay(info)) return;
                var dataCell = new ReplayDataCell() {
                    playerName = info.playerName,
                    result = info.failTime == 0 ? "Completed" : "Failed",
                    date = FormatUtils.GetDateTimeString(info.timestamp),
                    filePath = path
                };
                Plugin.Log.Info($"Found local replay: {dataCell.playerName} | {dataCell.date}");
                AddReplayToList(dataCell);
                ReloadReplayListCells();
            }, () => {
                SetReplayListLoadingState(false);
                Plugin.Log.Notice("Loading finished.");
            });
        }

        private void ReloadReplayListCells() {
            _replayList.tableView.ReloadData();
            var height = Mathf.Clamp(_replayList
                .data.Count * Height, 0, MaxCellsCount * Height);
            _replayListRect.sizeDelta = new(_replayListRect.sizeDelta.x, height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_replayListContainerRect);
        }

        [UIAction("stop-loading"), UsedImplicitly]
        private void StopReplaysLoading() {
            StopReplayInfosEnumerationCoroutine();
            SetReplayListLoadingState(false);
        }

        [UIAction("select-cell"), UsedImplicitly]
        private void SelectReplayCell(TableView view, ReplayDataCell? cell) {
            _replayPanel.SetReplayPath(cell?.filePath);
        }

        [UIAction("refresh"), UsedImplicitly]
        private void RefreshReplayList() {
            ReloadReplays();
        }

        #endregion

        private void AddReplayToList(ReplayDataCell dataCell) {
            _replayList.data.Add(dataCell);
            ReloadReplayListCells();
        }

        private void ClearReplayList() {
            _replayList.data.Clear();
            ReloadReplayListCells();
        }

        #endregion

        #region ReplayTools

        private static IPreviewBeatmapLevel? _previewBeatmap;
        private static IManagedRoutine? _routine;

        private static void StartReplayInfosEnumerationCoroutine(Action<ReplayInfo, string> callback, Action? finishCallback) {
            _routine = RoutineFactory.StartManagedRoutine(ReplayInfosEnumerationCoroutine(callback, finishCallback));
        }

        private static void StopReplayInfosEnumerationCoroutine() {
            if (_routine is null) return;
            _routine.Cancel();
            _routine = null;
        }

        private static IEnumerator ReplayInfosEnumerationCoroutine(
            Action<ReplayInfo, string> callback, Action? finishCallback) {
            var files = FileManager.GetAllReplayPaths();
            foreach (var file in files) {
                if (FileManager.TryReadReplay(file, out var replay)) {
                    callback(replay!.info, file);
                }
                yield return null;
            }
            finishCallback?.Invoke();
        }

        private static bool IsValidReplay(ReplayInfo info) {
            return _previewBeatmap?.levelID.Replace("custom_level_", "") == info.hash;
        }

        #endregion
    }

}