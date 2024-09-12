using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.UI.Replayer;
using Reactive;
using UnityEngine;

namespace BeatLeader.Components {
    internal class PlayerListEditorComponent : LayoutEditorComponent {
        #region Setup

        private IVirtualPlayersManager? _playersManager;

        public void Setup(
            IVirtualPlayersManager playersManager,
            IBeatmapTimeController timeController
        ) {
            _playersManager = playersManager;
            _playerList.Setup(timeController, playersManager);
            _playerList.Items.AddRange(playersManager.Players);
            _playerList.Refresh();
        }

        protected override void OnInitialize() {
            _playerList.WithListener(x => x.SelectedIndexes, HandleItemsWithIndexesSelected);
        }

        #endregion

        #region LayoutEditorComponent

        protected override Vector2 MinSize { get; } = new(60, 40);
        public override string ComponentName => "Player List";

        private PlayerList _playerList = null!;

        protected override void ConstructInternal(Transform parent) {
            _playerList = new PlayerList();
            _playerList.WithRectExpand().Use(parent);
        }

        #endregion

        #region Callbacks

        private void HandleItemsWithIndexesSelected(IReadOnlyCollection<int> indexes) {
            if (indexes.Count is 0) return;
            var item = _playerList.Items[indexes.First()];
            _playersManager?.SetPrimaryPlayer(item);
        }

        #endregion
    }
}