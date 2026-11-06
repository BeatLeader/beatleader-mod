using System;
using System.Collections.Generic;
using BeatLeader.Models;
using Reactive;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class CharacteristicFilterPanel : ReactiveComponent, IPanelListFilter<IReplayHeader> {
        public CharacteristicFilterPanel(params IPanelListFilter<IReplayHeader>[] dependsOn) {
            DependsOn = dependsOn;
        }

        #region Filter

        public IEnumerable<IPanelListFilter<IReplayHeader>>? DependsOn { get; }
        public string FilterName => "Characteristic Filter";
        public string FilterStatus { get; private set; } = null!;
        public BeatmapCharacteristic? Characteristic { get; private set; }

        public event Action? FilterUpdatedEvent;

        public bool Matches(IReplayHeader value) {
            var characteristic = Characteristic?.SerializedName();
            return characteristic == null || value.ReplayInfo.SongMode == characteristic;
        }

        private void RefreshFilterStatus() {
            FilterStatus = Characteristic == null ? "No Mode" : Characteristic.Value.SerializedName();
        }

        #endregion

        #region Setup

        public event Action<BeatmapCharacteristic>? CharacteristicSelectedEvent {
            add => _characteristicPanel.CharacteristicSelectedEvent += value;
            remove => _characteristicPanel.CharacteristicSelectedEvent -= value;
        }

        public void SetBeatmapLevel(BeatmapLevel? level) {
            var characteristics = level?.GetCharacteristics();
            _characteristicPanel.SetData(characteristics);
        }

        protected override void OnInitialize() {
            RefreshFilterStatus();
            _characteristicPanel.SetData(null);
            _characteristicPanel.CharacteristicSelectedEvent += HandleBeatmapCharacteristicSelected;
        }

        #endregion

        #region Construct

        private BeatmapCharacteristicPanel _characteristicPanel = null!;

        protected override GameObject Construct() {
            return new BeatmapCharacteristicPanel().Bind(ref _characteristicPanel).Use();
        }

        #endregion

        #region Callbacks

        private void HandleBeatmapCharacteristicSelected(BeatmapCharacteristic characteristic) {
            Characteristic = characteristic;
            RefreshFilterStatus();
            FilterUpdatedEvent?.Invoke();
        }

        #endregion
    }
}