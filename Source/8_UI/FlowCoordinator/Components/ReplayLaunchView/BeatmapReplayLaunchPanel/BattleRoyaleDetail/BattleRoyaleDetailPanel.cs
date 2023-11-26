using System;
using System.Collections.Generic;
using BeatLeader.API;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.Replayer;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class BattleRoyaleDetailPanel : ReeUIComponentV3<BattleRoyaleDetailPanel>, BeatmapReplayLaunchPanel.IDetailPanel, IReplayFilter {
        #region ReplayFilter

        public IPreviewBeatmapLevel? BeatmapLevel => _beatmapFiltersPanel.BeatmapLevel;
        public BeatmapCharacteristicSO? BeatmapCharacteristic => _beatmapFiltersPanel.BeatmapCharacteristic;
        public BeatmapDifficulty? BeatmapDifficulty => _beatmapFiltersPanel.BeatmapDifficulty;
        public string? PlayerName { get; private set; }

        public bool Enabled => true;

        public event Action? FilterUpdatedEvent;

        private void NotifyFilterUpdated() {
            FilterUpdatedEvent?.Invoke();
        }

        #endregion

        #region UI Components

        [UIComponent("opponents-list"), UsedImplicitly]
        private BattleRoyaleOpponentsList _opponentsList = null!;

        [UIComponent("object-switcher"), UsedImplicitly]
        private ObjectSwitcher _objectSwitcher = null!;

        [UIValue("beatmap-filters-panel"), UsedImplicitly]
        private BeatmapFiltersPanel _beatmapFiltersPanel = null!;

        #endregion

        #region Setup

        public bool AllowReplayMultiselect => true;

        private IBeatmapReplayLaunchPanel _replayLaunchPanel = null!;
        private IListComponent<IReplayHeader> _replaysList = null!;
        private IReplayFilter? _originalReplayFilter;

        public void SetData(IBeatmapReplayLaunchPanel launchPanel, IReadOnlyList<IReplayHeader> headers) {
            _replayLaunchPanel = launchPanel;
            _replaysList = launchPanel.List;
            _opponentsList.items.Clear();
            _opponentsList.items.AddRange(headers);
            _opponentsList.Refresh();
            _opponentsList.Setup(_replaysList);
        }

        public void OnStateChange(bool active) {
            if (active) {
                _originalReplayFilter = _replayLaunchPanel.Filter;
                if (_originalReplayFilter is not null) {
                    _originalReplayFilter.FilterUpdatedEvent += HandleOriginalFilterUpdated;
                    HandleOriginalFilterUpdated();
                }
                _replayLaunchPanel.Filter = this;
            } else {
                _replayLaunchPanel.Filter = _originalReplayFilter;
                if (_originalReplayFilter is not null) {
                    _originalReplayFilter.FilterUpdatedEvent -= HandleOriginalFilterUpdated;
                }
            }
        }

        protected override void OnInitialize() {
            _objectSwitcher.ShowObjectWithIndex(0);
            _beatmapFiltersPanel.DisplayToggles = false;
        }

        protected override void OnInstantiate() {
            _beatmapFiltersPanel = ReeUIComponentV2.Instantiate<BeatmapFiltersPanel>(transform);
            _beatmapFiltersPanel.FilterUpdatedEvent += HandleFilterUpdated;
        }

        #endregion

        #region Callbacks

        private void HandleFilterUpdated() {
            NotifyFilterUpdated();
        }

        private void HandleOriginalFilterUpdated() {
            PlayerName = _originalReplayFilter!.PlayerName;
            NotifyFilterUpdated();
        }

        [UIAction("remove-all-opponents-click"), UsedImplicitly]
        private void HandleRemoveAllOpponentsButtonClicked() {
            _opponentsList.items.Clear();
            _opponentsList.Refresh();
            _replaysList.ClearSelection();
        }

        [UIAction("cell-select"), UsedImplicitly]
        private void HandleCellWithKeySelected(string key) {
            _objectSwitcher.ShowObjectWithKey(key);
        }

        [UIAction("start-button-click"), UsedImplicitly]
        private async void HandleStartButtonClicked() {
            var replays = new Dictionary<Replay, Player?>();
            foreach (var header in _opponentsList.items) {
                var replayTask = header.LoadReplayAsync(default);
                var playerRequest = PlayerRequest.SendRequest(header.ReplayInfo!.PlayerID);
                var replay = await replayTask;
                await playerRequest.Join();
                replays.Add(replay!, playerRequest.Result);
            }
            _ = ReplayerMenuLoader.Instance!.StartReplaysAsync(replays, null, default);
        }

        #endregion
    }
}