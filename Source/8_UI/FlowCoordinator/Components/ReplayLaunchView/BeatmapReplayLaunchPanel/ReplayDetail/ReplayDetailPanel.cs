using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using static BeatLeader.Models.FileStatus;

namespace BeatLeader.Components {
    internal class ReplayDetailPanel : ReeUIComponentV2 {
        #region UI Components

        [UIValue("replay-info-panel"), UsedImplicitly]
        private ReplayStatisticsPanel _replayStatisticsPanel = null!;

        [UIValue("player-info-panel"), UsedImplicitly]
        private HorizontalMiniProfileContainer _miniProfile = null!;

        [UIValue("watch-button-interactable"), UsedImplicitly]
        private bool WatchButtonInteractable {
            get => _watchButtonInteractable;
            set {
                _watchButtonInteractable = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("delete-button-interactable"), UsedImplicitly]
        private bool DeleteButtonInteractable {
            get => _deleteButtonInteractable;
            set {
                _deleteButtonInteractable = value;
                NotifyPropertyChanged();
            }
        }

        private bool _deleteButtonInteractable;
        private bool _watchButtonInteractable;

        #endregion

        #region Init

        private ReplayerMenuLoader _menuLoader = null!;
        private bool _isInitialized;
        
        public void Setup(ReplayerMenuLoader loader) {
            _menuLoader = loader;
            _isInitialized = true;
        }
        
        protected override void OnInstantiate() {
            _replayStatisticsPanel = Instantiate<ReplayStatisticsPanel>(transform);
            _miniProfile = Instantiate<HorizontalMiniProfileContainer>(transform);
            _replayStatisticsPanel.SetData(null, null, true, true);
            _miniProfile.SetPlayer(null);
            _miniProfile.PlayerLoadedEvent += HandlePlayerLoaded;
        }

        #endregion

        #region Data

        private IReplayHeader? _header;
        private Player? _player;

        public void SetData(IReplayHeader? header) {
            var invalid = header is null || header.FileStatus is Corrupted;
            _replayStatisticsPanel.SetData(null, null, invalid, header is null);
            _header = header;
            DeleteButtonInteractable = header is not null;
            WatchButtonInteractable = false;
            if (!invalid) _ = ProcessDataAsync(header!);
            else _miniProfile.SetPlayer(null);
        }

        private async Task ProcessDataAsync(IReplayHeader header) {
            _miniProfile.SetPlayer(header.ReplayInfo?.playerID);
            DeleteButtonInteractable = false;
            var replay = await header.LoadReplayAsync(default);
            DeleteButtonInteractable = true;
            var stats = default(ScoreStats?);
            var score = default(Score?);
            if (replay is not null) {
                await Task.Run(() => stats = ReplayStatisticUtils.ComputeScoreStats(replay));
                score = ReplayUtils.ComputeScore(replay);
            }
            _replayStatisticsPanel.SetData(score, stats, score is null || stats is null);
            WatchButtonInteractable = _isInitialized && await _menuLoader.CanLaunchReplay(header.ReplayInfo!);
        }

        #endregion

        #region Callbacks

        private void HandlePlayerLoaded(Player player) {
            _player = player;
        }

        [UIAction("delete-button-click"), UsedImplicitly]
        private void HandleDeleteButtonClicked() {
            _header?.DeleteReplayAsync(default);
        }

        [UIAction("watch-button-click"), UsedImplicitly]
        private void HandleWatchButtonClicked() {
            if (_header is null || _header.FileStatus is Corrupted) return;
            _ = _menuLoader.StartReplayAsync(_header.LoadReplayAsync(default).Result!, _player);
        }

        #endregion
    }
}