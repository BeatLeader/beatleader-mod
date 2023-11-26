using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using UnityEngine;

namespace BeatLeader.Components {
    internal class PlayerListEditorComponent : LayoutEditorComponent<PlayerListEditorComponent> {
        #region UI Components

        private PlayerList _playerList = null!;

        #endregion
        
        #region Setup

        public void Setup(IBeatmapTimeController timeController, IEnumerable<IReplay> replays) {
           _playerList.Setup(timeController);
           _playerList.items.AddRange(replays);
           _playerList.Refresh();
        }

        #endregion
        
        #region LayoutEditorComponent

        public override string ComponentName => "Player List";
        
        protected override GameObject ConstructInternal(Transform parent) {
            _playerList = PlayerList.Instantiate(parent);
            _playerList.InheritSize = true;
            _playerList.ContentTransform!.SetParent(parent, false);
            return _playerList.Content!;
        }
        
        #endregion
    }
}