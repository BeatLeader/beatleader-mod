using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class CharacteristicFilterPanel : ReactiveComponent, IPanelListFilter<IReplayHeaderBase> {
        public CharacteristicFilterPanel(params IPanelListFilter<IReplayHeaderBase>[] dependsOn) {
            DependsOn = dependsOn;
        }

        public CharacteristicFilterPanel() { }

        #region Filter

        public IEnumerable<IPanelListFilter<IReplayHeaderBase>>? DependsOn { get; }
        public string FilterName => "Characteristic Filter";
        public BeatmapCharacteristicSO? BeatmapCharacteristic { get; private set; }

        public event Action? FilterUpdatedEvent;

        public bool Matches(IReplayHeaderBase value) {
            var characteristic = BeatmapCharacteristic?.serializedName;
            return characteristic == null || value.ReplayInfo.SongMode == characteristic;
        }

        #endregion

        #region Setup

        public event Action<BeatmapCharacteristicSO>? CharacteristicSelectedEvent {
            add => _characteristicPanel.CharacteristicSelectedEvent += value;
            remove => _characteristicPanel.CharacteristicSelectedEvent -= value;
        }

        public void SetBeatmapLevel(IPreviewBeatmapLevel? level) {
            var sets = level?.previewDifficultyBeatmapSets
                .Select(
                    static x => new DifficultyBeatmapSet(
                        x.beatmapCharacteristic,
                        Array.Empty<IDifficultyBeatmap>()
                    )
                )
                .ToArray();
            _characteristicPanel.SetData(sets);
        }

        protected override void OnInitialize() {
            _characteristicPanel.SetData(null);
            _characteristicPanel.CharacteristicSelectedEvent += HandleBeatmapCharacteristicSelected;
            this.AsFlexItem(size: new() { x = 52f, y = 8f });
        }

        #endregion

        #region Construct

        private BeatLeader.Components.BeatmapCharacteristicPanel _characteristicPanel = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new ReeWrapperV2<BeatLeader.Components.BeatmapCharacteristicPanel>()
                        .AsFlexItem(grow: 1f)
                        .BindRee(ref _characteristicPanel)
                }
            }.AsFlexGroup(padding: 1f).Use();
        }

        #endregion

        #region Callbacks

        private void HandleBeatmapCharacteristicSelected(BeatmapCharacteristicSO characteristic) {
            BeatmapCharacteristic = characteristic;
            FilterUpdatedEvent?.Invoke();
        }

        #endregion
    }
}