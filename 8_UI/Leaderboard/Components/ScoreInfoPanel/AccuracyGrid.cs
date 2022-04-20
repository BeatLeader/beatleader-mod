using System.Collections.Generic;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreInfoPanel.AccuracyGrid.bsml")]
    internal class AccuracyGrid : ReeUIComponent {
        #region IndexingUtils

        private const int LayersCount = 3;
        private const int CellsPerLayer = 4;
        private const int TotalCellsCount = LayersCount * CellsPerLayer;

        private static int GetCellIndex(int layerIndex, int columnIndex) {
            return layerIndex * CellsPerLayer + columnIndex;
        }

        #endregion

        #region Components

        private readonly AccuracyGridCell[] _accuracyGridCells;

        public AccuracyGrid() {
            _accuracyGridCells = new AccuracyGridCell[TotalCellsCount];
            for (var i = 0; i < TotalCellsCount; i++) {
                _accuracyGridCells[i] = Instantiate<AccuracyGridCell>();
            }
        }

        [UIValue("top-layer-cells"), UsedImplicitly]
        private List<AccuracyGridCell> TopLayerCells => GetLayerCells(2);

        [UIValue("upper-layer-cells"), UsedImplicitly]
        private List<AccuracyGridCell> UpperLayerCells => GetLayerCells(1);

        [UIValue("base-layer-cells"), UsedImplicitly]
        private List<AccuracyGridCell> BaseLayerCells => GetLayerCells(0);

        private List<AccuracyGridCell> GetLayerCells(int layerIndex) {
            var tmp = new List<AccuracyGridCell>();
            for (var columnIndex = 0; columnIndex < CellsPerLayer; columnIndex++) tmp.Add(_accuracyGridCells[GetCellIndex(layerIndex, columnIndex)]);
            return tmp;
        }

        #endregion

        #region SetScoreStats

        public void SetScoreStats(ScoreStats scoreStats) {
            var tracker = scoreStats.accuracyTracker;
            for (var i = 0; i < TotalCellsCount; i++) {
                _accuracyGridCells[i].SetScore(tracker.gridAcc[i]);
            }
        }

        #endregion

        #region Clear

        public void Clear() {
            for (var i = 0; i < TotalCellsCount; i++) {
                _accuracyGridCells[i].Clear();
            }
        }

        #endregion

        #region SetActive

        public void SetActive(bool value) {
            Active = value;
        }

        #endregion

        #region Active

        private bool _active = true;

        [UIValue("active"), UsedImplicitly]
        private bool Active {
            get => _active;
            set {
                if (_active.Equals(value)) return;
                _active = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}