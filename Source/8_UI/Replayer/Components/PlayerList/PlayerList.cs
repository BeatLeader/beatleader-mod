using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using Reactive;
using Reactive.BeatSaber.Components;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class PlayerList : ReactiveComponent {
        #region Setup

        private IBeatmapTimeController? _timeController;
        private IVirtualPlayersManager? _playersManager;

        public void Setup(IEnumerable<IVirtualPlayer> players, IBeatmapTimeController? timeController, IVirtualPlayersManager? playersManager) {
            if (_playersManager != null) {
                _playersManager.PrimaryPlayerWasChangedEvent -= HandlePrimaryPlayerChanged;
            }
            _timeController = timeController;
            _playersManager = playersManager;
            if (_playersManager != null) {
                _playersManager.PrimaryPlayerWasChangedEvent += HandlePrimaryPlayerChanged;
            }
            ReloadCells(players);
            RefreshHandle();
        }

        protected override void OnLateUpdate() {
            UpdateHandleAnimation();
            UpdateCellsAnimation();
        }

        protected override void OnStart() {
            PlaceCells(false);
            PlaceHandle();
        }

        #endregion

        #region Construct

        protected override void Construct(RectTransform rect) {
            rect.pivot = new(1f, 1f);
            // Handle
            new Image {
                ContentTransform = {
                    pivot = new(1f, 0.5f)
                },
                Sprite = BundleLoader.Sprites.triangleIcon,
                Material = BundleLoader.Materials.uiNoDepthMaterial,
                Color = new(0.1f, 0.1f, 0.1f)
            }.WithSizeDelta(4f, 6f).Bind(ref _handleTransform).Use(rect);
        }

        #endregion

        #region Handle

        private float HandlePos => _primaryCellPos - PlayerListCell.CELL_SIZE / 2f;
        private RectTransform _handleTransform = null!;

        private void RefreshHandle() {
            _handleTransform.gameObject.SetActive(_sortedCells.Count > 1);
        }

        private void PlaceHandle() {
            _handleTransform.localPosition = new(0f, HandlePos);
        }

        private void UpdateHandleAnimation() {
            _handleTransform.localPosition = Vector3.Lerp(
                _handleTransform.localPosition,
                new(0f, HandlePos),
                Time.deltaTime * 4f
            );
        }

        #endregion

        #region Cells

        private class CellComparator : IComparer<PlayerListCell> {
            public int Compare(PlayerListCell x, PlayerListCell y) {
                var xScore = GetScore(x.Player);
                var yScore = GetScore(y.Player);
                return Comparer<int>.Default.Compare(yScore, xScore);
            }

            private static int GetScore(IVirtualPlayer player) {
                return player.ReplayScoreEventsProcessor.CurrentScoreEvent?.Value.score ?? 0;
            }
        }

        private readonly ReactivePool<PlayerListCell> _cellsPool = new() { DetachOnDespawn = false };
        private readonly List<PlayerListCell> _sortedCells = new();
        private readonly CellComparator _cellComparator = new();
        private float _primaryCellPos;

        private void ReloadCells(IEnumerable<IVirtualPlayer> players) {
            DespawnCells();
            if (_timeController == null) {
                return;
            }
            
            foreach (var player in players) {
                var cell = _cellsPool.Spawn();
                var trans = cell.ContentTransform;
                
                trans.pivot = new(1f, 1f);
                trans.anchorMin = new(0f, 1f);
                trans.anchorMax = new(1f, 1f);
                
                cell.Setup(player, _timeController!, this);
                cell.Use(ContentTransform);
                cell.CellSelectedEvent += HandleCellSelected;
                
                _sortedCells.Add(cell);
            }
            
            PlaceCells(false);
        }

        private void DespawnCells() {
            foreach (var cell in _cellsPool.SpawnedComponents) {
                cell.CellSelectedEvent -= HandleCellSelected;
            }
            _cellsPool.DespawnAll();
        }

        private void PlaceCells(bool animated) {
            _sortedCells.Sort(_cellComparator);
            
            for (var i = 0; i < _sortedCells.Count; i++) {
                var cell = _sortedCells[i];
                var trans = cell.ContentTransform;
                var pos = -i * PlayerListCell.CELL_SIZE;
               
                trans.SetSiblingIndex(i);
                if (animated) {
                    cell.MoveTo(pos);
                } else {
                    trans.localPosition = new(0f, pos);
                }
                
                if (cell.Player == _playersManager!.PrimaryPlayer) {
                    _primaryCellPos = pos;
                }
            }
        }

        private int FindPlayerIndex(IVirtualPlayer player) {
            for (var i = 0; i < _sortedCells.Count; i++) {
                var cell = _sortedCells[i];
                
                if (cell.Player == player) {
                    return i;
                }
            }
            return -1;
        }

        #endregion

        #region Cell Animation

        private float _lastReportedTime;
        private bool _cellUpdateRequired;

        public void NotifyCellUpdateRequired() {
            _cellUpdateRequired = true;
        }

        private void UpdateCellsAnimation() {
            var time = Time.time;
            
            if (_cellUpdateRequired && time - _lastReportedTime > 0.5f) {
                PlaceCells(true);
                _lastReportedTime = time;
                _cellUpdateRequired = false;
            }
        }

        #endregion

        #region Callbacks

        private IVirtualPlayer? _selectedPlayer;
        
        private void HandleCellSelected(PlayerListCell cell) {
            if (cell.Player == _selectedPlayer) {
                return;
            }
            
            _playersManager?.SetPrimaryPlayer(cell.Player);
            _selectedPlayer = cell.Player;
        }

        private void HandlePrimaryPlayerChanged(IVirtualPlayer player) {
            var index = FindPlayerIndex(player);
            _primaryCellPos = -index * PlayerListCell.CELL_SIZE;
            RefreshHandle();
        }

        #endregion
    }
}