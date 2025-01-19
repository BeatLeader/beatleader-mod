using BeatLeader.API.Methods;
using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Leaderboard.Components.MainPanel.ScoresTable.bsml")]
    internal class ClanScoresTable : AbstractScoresTable<ScoreRow> {
        #region Properties

        protected override int RowsCount => ClanScoresRequest.ScoresPerPage;
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

            ClanScoresRequest.AddStateListener(OnScoresRequestStateChanged);

            PluginConfig.LeaderboardTableMaskChangedEvent += OnLeaderboardTableMaskChanged;
            HiddenPlayersCache.HiddenPlayersUpdatedEvent += UpdateLayout;

            OnLeaderboardTableMaskChanged(PluginConfig.LeaderboardTableMask);
        }

        protected override void OnDispose() {
            base.OnDispose();

            ClanScoresRequest.RemoveStateListener(OnScoresRequestStateChanged);

            PluginConfig.LeaderboardTableMaskChangedEvent -= OnLeaderboardTableMaskChanged;
            HiddenPlayersCache.HiddenPlayersUpdatedEvent -= UpdateLayout;
        }

        #endregion

        #region Events

        private void OnScoresRequestStateChanged(API.RequestState state, ScoresTableContent result, string failReason) {
            if (state is not API.RequestState.Finished) {
                PresentContent(null);
                return;
            }

            PresentContent(result);
        }

        private void OnLeaderboardTableMaskChanged(ScoreRowCellType value) {
            UpdateLayout();
        }

        #endregion
    }
}