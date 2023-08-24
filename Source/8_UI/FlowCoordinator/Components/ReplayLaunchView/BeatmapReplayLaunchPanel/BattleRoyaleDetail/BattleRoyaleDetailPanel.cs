using System.Collections.Generic;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class BattleRoyaleDetailPanel : ReeUIComponentV3<BattleRoyaleDetailPanel>, BeatmapReplayLaunchPanel.IDetailPanel {
        #region UI Components

        [UIComponent("opponents-list"), UsedImplicitly]
        private BattleRoyaleOpponentsList _opponentsList = null!;

        #endregion

        #region Setup

        public bool AllowReplayMultiselect => true;

        private IListComponent<IReplayHeader> _replaysList = null!;
        
        public void SetData(IListComponent<IReplayHeader> list, IReadOnlyList<IReplayHeader> headers) {
            _replaysList = list;
            _opponentsList.items.Clear();
            _opponentsList.items.AddRange(headers);
            _opponentsList.Refresh();
            _opponentsList.Setup(list);
        }

        protected override void OnInitialize() { }

        #endregion

        #region Callbacks

        [UIAction("remove-all-opponents-click"), UsedImplicitly]
        private void HandleRemoveAllOpponentsButtonClicked() {
            _opponentsList.items.Clear();
            _opponentsList.Refresh();
            _replaysList.ClearSelection();
        }

        #endregion
    }
}