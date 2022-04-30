using System.Collections.Generic;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class AccuracyGrid : ReeUIComponentV2 {
        #region IndexingUtils

        private const int LayersCount = 3;
        private const int CellsPerLayer = 4;
        private const int TotalCellsCount = LayersCount * CellsPerLayer;

        private static int GetCellIndex(int layerIndex, int columnIndex) {
            return layerIndex * CellsPerLayer + columnIndex;
        }

        #endregion

        #region Components

        private AccuracyGridCell[] _accuracyGridCells;

        private void Awake() {
            _accuracyGridCells = new AccuracyGridCell[TotalCellsCount];
            for (var i = 0; i < TotalCellsCount; i++) {
                _accuracyGridCells[i] = Instantiate<AccuracyGridCell>(transform);
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
            var range = GetScoresRange(tracker.gridAcc);

            for (var i = 0; i < TotalCellsCount; i++) {
                var score = tracker.gridAcc[i];
                var quality = range.GetRatioClamped(score);
                _accuracyGridCells[i].SetScore(score, quality);
            }
        }

        private static Range GetScoresRange(float[] scores) {
            var max = float.MinValue;
            var min = float.MaxValue;

            foreach (var score in scores) {
                if (score == 0) continue;
                if (score > max) max = score;
                if (score < min) min = score;
            }

            return new Range(min, max);
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