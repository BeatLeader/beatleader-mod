using BeatLeader.Models;
using BeatLeader.UI.Replayer;
using Reactive;
using UnityEngine;

namespace BeatLeader.Components {
    internal class PlayerListEditorComponent : LayoutEditorComponent {
        #region Setup

        public void Setup(IVirtualPlayersManager playersManager, IBeatmapTimeController timeController) {
            _playerList.Setup(playersManager.Players, timeController, playersManager);
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
    }
}