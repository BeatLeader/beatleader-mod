using System;
using System.Collections.Generic;
using System.Linq;
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
        public string FilterName => "Mode";
        public BeatmapCharacteristicSO? BeatmapCharacteristic { get; private set; }
        public bool IsActive => BeatmapCharacteristic != null;

        public void Reset() {
            BeatmapLevel = null;
        }

        public event Action? FilterUpdatedEvent;

        public bool Matches(IReplayHeader value) {
            var characteristic = BeatmapCharacteristic?.serializedName;
            return characteristic == null || value.ReplayInfo.SongMode == characteristic;
        }

        #endregion

        #region Setup
        
        public BeatmapLevel? BeatmapLevel {
            get;
            set {
                field = value;
                
                if (IsActive) {
                    BeatmapCharacteristic = null;
                    FilterUpdatedEvent?.Invoke();
                }

                _characteristicPanel.Characteristics = value?.GetCharacteristics().ToArray();
            }
        }

        public Action<BeatmapCharacteristicSO>? OnCharacteristicSelected { get; set; }

        #endregion

        #region Construct

        private BeatmapCharacteristicPanel _characteristicPanel = null!;

        protected override GameObject Construct() {
            return new BeatmapCharacteristicPanel {
                OnCharacteristicSelected = HandleBeatmapCharacteristicSelected
            }.Bind(ref _characteristicPanel).Use();
        }

        #endregion

        #region Callbacks

        private void HandleBeatmapCharacteristicSelected(BeatmapCharacteristicSO characteristic) {
            BeatmapCharacteristic = characteristic;
            FilterUpdatedEvent?.Invoke();
            OnCharacteristicSelected?.Invoke(characteristic);
        }

        #endregion
    }
}