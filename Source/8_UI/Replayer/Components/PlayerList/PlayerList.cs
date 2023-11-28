using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class PlayerList : ListComponentBase<PlayerList, IVirtualPlayer> {
        #region Cell

        private class Cell : ListCellWithComponent<(IReplay, IBeatmapTimeController), PlayerListCell> { }

        private class PlayerListCell : ReeUIComponentV3<PlayerListCell>, Cell.IComponent, Cell.IStateHandler {
            #region UI Components

            [UIComponent("timeline"), UsedImplicitly]
            private EventTimeline _eventTimeline = null!;

            [UIComponent("mini-profile"), UsedImplicitly]
            private QuickMiniProfile _miniProfile = null!;

            [UIComponent("background-image"), UsedImplicitly]
            private AdvancedImage _backgroundImage = null!;
            
            #endregion

            private IBeatmapTimeController _timeController = null!;

            public void Init((IReplay, IBeatmapTimeController) data) {
                var (replay, timeController) = data;
                var replayData = replay.ReplayData;
                _eventTimeline.Range = new(
                    replayData.PracticeSettings?.startSongTime ?? 0,
                    replayData.FinishTime
                );
                _miniProfile.SetPlayer(replay.ReplayData.Player!);
                _timeController = timeController;
            }

            private void Update() {
                _eventTimeline.Value = _timeController.SongTime;
            }

            public void OnStateChange(bool selected, bool highlighted) {
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

        protected override ListComponentBaseCell ConstructCell(IVirtualPlayer data) {
            var cell = DequeueReusableCell(Cell.CellName) as Cell ?? Cell.InstantiateCell<Cell>();
            cell.Init((data.Replay, _timeController!));
            return cell;
        }

        protected override bool OnValidation() {
            return _timeController is not null;
        }

        #endregion
    }
}