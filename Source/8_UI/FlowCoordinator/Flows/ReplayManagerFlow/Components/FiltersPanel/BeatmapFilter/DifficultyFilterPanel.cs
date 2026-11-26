using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using Reactive;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class DifficultyFilterPanel : ReactiveComponent, IPanelListFilter<IReplayHeader> {
        public DifficultyFilterPanel(params IPanelListFilter<IReplayHeader>[] dependsOn) {
            DependsOn = dependsOn;
        }

        #region Filter

        public IEnumerable<IPanelListFilter<IReplayHeader>>? DependsOn { get; }
        public string FilterName => "Difficulty";
        public BeatmapDifficulty? BeatmapDifficulty { get; private set; }
        public bool IsActive => BeatmapDifficulty.HasValue;

        public void Reset() {
            BeatmapLevel = null;
        }

        public event Action? FilterUpdatedEvent;

        public bool Matches(IReplayHeader value) {
            var diff = BeatmapDifficulty;
            return !diff.HasValue || value.ReplayInfo.SongDifficulty == diff.Value.ToString();
        }

        #endregion

        #region Setup
        
        public BeatmapLevel? BeatmapLevel {
            get;
            set {
                field = value;
                BeatmapCharacteristic = null;
            }
        }
        
        public BeatmapCharacteristicSO? BeatmapCharacteristic {
            get;
            set {
                field = value;

                if (IsActive) {
                    BeatmapDifficulty = null;
                    FilterUpdatedEvent?.Invoke();
                }

                _difficultyPanel.Difficulties = value != null ? BeatmapLevel?.GetDifficulties(value).ToArray() : null;
            }
        }

        #endregion

        #region Construct

        private BeatmapDifficultyPanel _difficultyPanel = null!;

        protected override GameObject Construct() {
            return new BeatmapDifficultyPanel {
                OnDifficultySelected = HandleDifficultySelected
            }.Bind(ref _difficultyPanel).Use();
        }

        #endregion

        #region Callbacks

        private void HandleDifficultySelected(BeatmapDifficulty difficulty) {
            BeatmapDifficulty = difficulty;
            FilterUpdatedEvent?.Invoke();
        }

        #endregion
    }
}