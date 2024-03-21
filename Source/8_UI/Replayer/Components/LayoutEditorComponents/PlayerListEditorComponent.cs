using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader.Components {
    internal class PlayerListEditorComponent : LayoutEditorComponent {
        #region UI Components

        private PlayerList _playerList = null!;

        #endregion

        #region Setup

        private IVirtualPlayersManager? _playersManager;

        public void Setup(
            IVirtualPlayersManager playersManager,
            IBeatmapTimeController timeController
        ) {
            _playersManager = playersManager;
            _playerList.Setup(timeController);
            _playerList.items.AddRange(playersManager.Players);
            _playerList.Refresh();
        }

        protected override void OnInitialize() {
            _playerList.ItemsWithIndexesSelectedEvent += HandleItemsWithIndexesSelected;
        }

        #endregion

        #region LayoutEditorComponent

        protected override Vector2 MinSize { get; } = new(60, 40);
        public override string ComponentName => "Player List";

        protected override void ConstructInternal(Transform parent) {
            _playerList = PlayerList.Instantiate(parent);
            _playerList.InheritSize = true;
            _playerList.ContentTransform.SetParent(parent, false);
        }

        #endregion

        #region Callbacks

        private void HandleItemsWithIndexesSelected(ICollection<int> indexes) {
            if (indexes.Count is 0) return;
            var item = _playerList.items[indexes.First()];
            _playersManager?.SetPrimaryPlayer(item);
        }

        #endregion
    }
}