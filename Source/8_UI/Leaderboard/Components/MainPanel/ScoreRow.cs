using BeatLeader.DataManager;
using BeatLeader.Manager;
using BeatLeader.Models;
using TMPro;

namespace BeatLeader.Components {
    internal class ScoreRow : AbstractScoreRow {
        #region Components

        private TextScoreRowCell _rankCell = null!;
        private TextScoreRowCell _usernameCell = null!;
        private TextScoreRowCell _modifiersCell = null!;
        private TextScoreRowCell _accuracyCell = null!;
        private TextScoreRowCell _ppCell = null!;
        private TextScoreRowCell _scoreCell = null!;
        private TextScoreRowCell _timeCell = null!;
        private TextScoreRowCell _pausesCell = null!;

        private void Awake() {
            _rankCell = InstantiateCell<TextScoreRowCell>(ScoreRowCellType.Rank);
            InstantiateCell<AvatarScoreRowCell>(ScoreRowCellType.Avatar);
            InstantiateCell<CountryScoreRowCell>(ScoreRowCellType.Country);
            _usernameCell = InstantiateCell<TextScoreRowCell>(ScoreRowCellType.Username);
            InstantiateCell<ClansScoreRowCell>(ScoreRowCellType.Clans);
            _modifiersCell = InstantiateCell<TextScoreRowCell>(ScoreRowCellType.Modifiers);
            _accuracyCell = InstantiateCell<TextScoreRowCell>(ScoreRowCellType.Accuracy);
            _ppCell = InstantiateCell<TextScoreRowCell>(ScoreRowCellType.PerformancePoints);
            _scoreCell = InstantiateCell<TextScoreRowCell>(ScoreRowCellType.Score);
            _timeCell = InstantiateCell<TextScoreRowCell>(ScoreRowCellType.Time);
            InstantiateCell<MistakesScoreRowCell>(ScoreRowCellType.Mistakes);
            _pausesCell = InstantiateCell<TextScoreRowCell>(ScoreRowCellType.Pauses);
        }

        protected override void OnInitialize() {
            base.OnInitialize();

            _rankCell.Setup(o => FormatUtils.FormatRank((int)o, false));
            _usernameCell.Setup(o => FormatUtils.FormatUserName((string)o), TextAlignmentOptions.Left, TextOverflowModes.Ellipsis, 3.4f, false);
            _modifiersCell.Setup(o => FormatUtils.FormatModifiers((string)o), TextAlignmentOptions.Right, TextOverflowModes.Overflow, 2.4f);
            _accuracyCell.Setup(o => FormatUtils.FormatAcc((float)o));
            _ppCell.Setup(o => FormatUtils.FormatPP((float)o));
            _scoreCell.Setup(o => FormatUtils.FormatScore((int)o), TextAlignmentOptions.Right);
            _timeCell.Setup(o => FormatUtils.FormatTimeset((string)o, true), TextAlignmentOptions.Center, TextOverflowModes.Overflow, 2.4f);
            _pausesCell.Setup(o => FormatUtils.FormatPauses((int)o));

            HiddenPlayersCache.HiddenPlayersUpdatedEvent += UpdateContent;
        }

        protected override void OnDispose() {
            base.OnDispose();

            HiddenPlayersCache.HiddenPlayersUpdatedEvent -= UpdateContent;
        }

        protected override void OnClick() {
            switch (ScoreRowContent) {
                case Score playerScore: {
                    LeaderboardEvents.NotifyScoreInfoButtonWasPressed(playerScore);
                    break;
                }
                case ClanScore clanScore: {
                    LeaderboardEvents.NotifyClanScoreInfoButtonWasPressed(clanScore);
                    break;
                }
                case ClanPlayer clanPlayer: {
                    if (clanPlayer.score == null) return;
                    LeaderboardEvents.NotifyScoreInfoButtonWasPressed(clanPlayer.score);
                    break;
                }
            }
        }

        #endregion
    }
}