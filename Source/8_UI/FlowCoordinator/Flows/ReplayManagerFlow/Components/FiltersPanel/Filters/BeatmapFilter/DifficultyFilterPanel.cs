using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class DifficultyFilterPanel : ReactiveComponent, IPanelListFilter<IReplayHeaderBase> {
        public DifficultyFilterPanel(params IPanelListFilter<IReplayHeaderBase>[] dependsOn) {
            DependsOn = dependsOn;
        }
        
        #region Filter

        public IEnumerable<IPanelListFilter<IReplayHeaderBase>>? DependsOn { get; }
        public string FilterName => "Difficulty Filter";
        public BeatmapDifficulty? BeatmapDifficulty { get; private set; }

        public event Action? FilterUpdatedEvent;

        public bool Matches(IReplayHeaderBase value) {
            var diff = BeatmapDifficulty;
            return !diff.HasValue || value.ReplayInfo.SongDifficulty == diff.Value.ToString();
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
            var sets = _beatmapLevel?.GetCharacteristics()
                .SelectMany(x => _beatmapLevel.GetDifficulties(x))
                .ToArray();
            _difficultyPanel.SetData(sets);
        }

        protected override void OnInitialize() {
            _difficultyPanel.SetData(null);
            _difficultyPanel.DifficultySelectedEvent += HandleDifficultySelected;
            this.AsFlexItem(size: new() { x = 52f, y = 8f });
        }

        #endregion

        #region Construct

        private BeatLeader.Components.BeatmapDifficultyPanel _difficultyPanel = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new ReeWrapperV2<BeatLeader.Components.BeatmapDifficultyPanel>()
                        .AsFlexItem(grow: 1f)
                        .BindRee(ref _difficultyPanel)
                }
            }.AsFlexGroup(padding: 1f).Use();
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