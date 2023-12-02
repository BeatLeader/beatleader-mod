using System.Reflection;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class PlayerList : ReeListComponentBase<PlayerList, IVirtualPlayer, PlayerList.Cell> {
        #region Cell
        
        public class Cell : ReeTableCell<Cell, IVirtualPlayer> {
            #region UI Components

            [UIComponent("timeline"), UsedImplicitly]
            private EventTimeline _eventTimeline = null!;

            [UIComponent("mini-profile"), UsedImplicitly]
            private QuickMiniProfile _miniProfile = null!;

            [UIComponent("background-image"), UsedImplicitly]
            private AdvancedImage _backgroundImage = null!;
            
            #endregion

            protected override string Markup { get; } = BSMLUtility.ReadMarkupOrFallback(
                "PlayerListCell", Assembly.GetExecutingAssembly()
            );
            
            private IBeatmapTimeController _timeController = null!;

            public override void Init(IVirtualPlayer player) {
                var replay = player.Replay;
                var replayData = replay.ReplayData;
                _eventTimeline.Range = new(
                    replayData.PracticeSettings?.startSongTime ?? 0,
                    replayData.FinishTime
                );
                _miniProfile.SetPlayer(replay.ReplayData.Player!);
            }
            
            public void Init(IBeatmapTimeController timeController) {
                _timeController = timeController;
            }

            private void Update() {
                _eventTimeline.Value = _timeController.SongTime;
            }

            public override void OnStateChange(bool selected, bool highlighted) {
                _backgroundImage.Color = (selected ? Color.cyan : Color.black).ColorWithAlpha(0.5f);
            }
        }

        #endregion

        #region Setup

        protected override float CellSize => 20f;

        private IBeatmapTimeController? _timeController;

        public void Setup(IBeatmapTimeController timeController) {
            _timeController = timeController;
        }

        protected override void OnCellConstruct(Cell cell) {
            cell.Init(_timeController!);
        }

        protected override bool OnValidation() {
            return _timeController is not null;
        }

        #endregion
    }
}