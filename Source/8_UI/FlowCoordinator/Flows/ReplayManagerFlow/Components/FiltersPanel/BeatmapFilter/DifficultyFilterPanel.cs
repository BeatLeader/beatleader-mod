using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class DifficultyFilterPanel : ReactiveComponent, IPanelListFilter<IReplayHeader> {
        public DifficultyFilterPanel(params IPanelListFilter<IReplayHeader>[] dependsOn) {
            DependsOn = dependsOn;
        }

        #region Filter

        public IEnumerable<IPanelListFilter<IReplayHeader>>? DependsOn { get; }
        public string FilterName => "Difficulty Filter";
        public string FilterStatus { get; private set; } = null!;
        public BeatmapDifficulty? BeatmapDifficulty { get; private set; }

        public event Action? FilterUpdatedEvent;

        public bool Matches(IReplayHeader value) {
            var diff = BeatmapDifficulty;
            return !diff.HasValue || value.ReplayInfo.SongDifficulty == diff.Value.ToString();
        }

        private void RefreshFilterStatus() {
            FilterStatus = BeatmapDifficulty == null ? "No Diff" : BeatmapDifficulty.ToString();
        }

        #endregion

        #region Setup

        private BeatmapLevel? _beatmapLevel;
        private BeatmapCharacteristicSO? _characteristic;

        public void SetBeatmapLevel(BeatmapLevel? level) {
            _beatmapLevel = level;
            if (_characteristic != null) {
                SetCharacteristic(_characteristic);
            }
        }

        public void SetCharacteristic(BeatmapCharacteristicSO characteristic) {
            _characteristic = characteristic;
            var sets = _beatmapLevel?.GetDifficulties(characteristic).ToArray();
            _difficultyPanel.SetData(sets);
        }

        protected override void OnInitialize() {
            RefreshFilterStatus();
            _difficultyPanel.SetData(null);
            _difficultyPanel.DifficultySelectedEvent += HandleDifficultySelected;
        }

        #endregion

        #region Construct

        private BeatmapDifficultyPanel _difficultyPanel = null!;

        protected override GameObject Construct() {
            return new BeatmapDifficultyPanel().Bind(ref _difficultyPanel).Use();
        }

        #endregion

        #region Callbacks

        private void HandleDifficultySelected(BeatmapDifficulty difficulty) {
            BeatmapDifficulty = difficulty;
            RefreshFilterStatus();
            FilterUpdatedEvent?.Invoke();
        }

        #endregion
    }
}