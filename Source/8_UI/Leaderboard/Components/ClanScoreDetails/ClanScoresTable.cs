using BeatLeader.API;
using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Leaderboard.Components.MainPanel.ScoresTable.bsml")]
    internal class ClanScoresTable : AbstractScoresTable<ScoreRow> {
        #region Properties

        protected override int RowsCount => ScoresOfClanRequest.ScoresPerPage;
        protected override float RowWidth => 77;
        protected override float Spacing => 1.3f;
        protected override ScoreRowCellType CellTypeMask {
            get {
                var mask = PluginConfig.LeaderboardTableMask;
                mask &= ~ScoreRowCellType.Clans;
                return mask;
            }
        }

        #endregion

        #region Initialize/Dispose

        protected override void OnInitialize() {
            base.OnInitialize();

            ScoresOfClanRequest.StateChangedEvent += OnScoresRequestStateChanged;
            ClanPlayersRequest.StateChangedEvent += OnScoresRequestStateChanged;

            PluginConfig.LeaderboardTableMaskChangedEvent += OnLeaderboardTableMaskChanged;
            HiddenPlayersCache.HiddenPlayersUpdatedEvent += UpdateLayout;

            OnLeaderboardTableMaskChanged(PluginConfig.LeaderboardTableMask);
        }

        protected override void OnDispose() {
            base.OnDispose();

            ScoresOfClanRequest.StateChangedEvent -= OnScoresRequestStateChanged;
            ClanPlayersRequest.StateChangedEvent -= OnScoresRequestStateChanged;

            PluginConfig.LeaderboardTableMaskChangedEvent -= OnLeaderboardTableMaskChanged;
            HiddenPlayersCache.HiddenPlayersUpdatedEvent -= UpdateLayout;
        }

        #endregion

        #region Events

        private void OnScoresRequestStateChanged(WebRequests.IWebRequest<ScoresTableContent> instance, WebRequests.RequestState state, string? failReason) {
            if (state is not WebRequests.RequestState.Finished) {
                PresentContent(null);
                return;
            }

            PresentContent(instance.Result);
        }

        private void OnLeaderboardTableMaskChanged(ScoreRowCellType value) {
            UpdateLayout();
        }

        #endregion
    }
}