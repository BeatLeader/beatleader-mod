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
            var stats = default(ScoreStats?);
            _miniProfile.SetPlayer(header.ReplayInfo?.playerID);
            DeleteButtonInteractable = false;
            var replay = await header.LoadReplayAsync(default);
            DeleteButtonInteractable = true;
            await Task.Run(() => stats = ReplayStatisticUtils.ComputeScoreStats(replay!));
            var score = default(Score?);
            if (replay is not null) {
                //var diff = await RequestDifficultyInfoByReplay(replay);
                score = ReplayUtils.ComputeScore(replay);
            }
            _replayStatisticsPanel.SetData(score, stats, _header is null);
            WatchButtonInteractable = _isInitialized && await _menuLoader.CanLaunchReplay(header.ReplayInfo!);
        }

        #endregion

        #region DiffInfo

        //private DiffInfo? _cachedDiffInfo;
        //private string? _cachedDiffHash;
        //private async Task<DiffInfo?> RequestDifficultyInfoByReplay(Replay replay) {
        //    var mapHash = replay.info.hash;
        //    if (_cachedDiffHash == mapHash) return _cachedDiffInfo;
        //    var str = await (await new HttpClient().SendAsync(new() {
        //        Method = HttpMethod.Get,
        //        RequestUri = new Uri(BLConstants.BEATLEADER_API_URL + "/leaderboards/hash/" + mapHash, UriKind.Absolute)
        //    })).Content.ReadAsStringAsync();
        //    try {
        //        var obj = JsonConvert.DeserializeObject<HashLeaderboardsInfoResponse>(str, NetworkingUtils.SerializerSettings);
        //        _cachedDiffInfo = obj.leaderboards.First(x =>
        //            x.difficulty.difficultyName == replay.info.difficulty).difficulty;
        //        _cachedDiffHash = mapHash;
        //        return _cachedDiffInfo;
        //    } catch (Exception ex) {
        //        return null;
        //    }
        //}

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