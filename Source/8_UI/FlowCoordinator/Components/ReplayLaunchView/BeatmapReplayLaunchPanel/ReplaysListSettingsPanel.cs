using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class ReplaysListSettingsPanel : ReeUIComponentV2 {
        #region Events

        public event Action<Sorters, bool>? SorterChangedEvent;
        public event Action<bool>? ShowCorruptedChangedEvent;
        public event Action? ReloadDataEvent;

        #endregion

        #region UI Components

        [UIComponent("tab-selector")]
        private readonly SegmentedControl _segmentedControl = null!;

        #endregion

        #region Sorters

        public enum Sorters {
            Difficulty,
            Player,
            Completion,
            Date
        }

        public bool AscendingSortOrder {
            get => _ascendingSortOrder;
            set {
                _ascendingSortOrder = value;
                _segmentedControl.cells[value ? 0 : 1].SetSelected(true,
                    SelectableCell.TransitionType.Instant, null, false);
                RefreshSorters();
            }
        }

        public Sorters Sorter {
            get => _sorter;
            set {
                _sorter = value;
                NotifyPropertyChanged(nameof(LocalSorter));
                RefreshSorters();
            }
        }

        [UIValue("sorter"), UsedImplicitly]
        private string LocalSorter {
            get => _sorter.ToString();
            set {
                _sorter = (Sorters)Enum.Parse(typeof(Sorters), value);
                RefreshSorters();
            }
        }

        [UIValue("sorters"), UsedImplicitly]
        private readonly List<object> _localSorters = sorters;
        private bool _ascendingSortOrder = true;
        private Sorters _sorter;

        private static readonly List<object> sorters = Enum.GetNames(typeof(Sorters)).ToList<object>();

        private void RefreshSorters() {
            SorterChangedEvent?.Invoke(_sorter, _ascendingSortOrder);
        }

        #endregion

        #region ShowCorrupted

        [UIValue("show-corrupted-interactable")]
        public bool ShowCorruptedInteractable {
            get => _showCorruptedInteractable;
            set {
                _showCorruptedInteractable = value;
                ShowCorrupted = value && ShowCorrupted;
                NotifyPropertyChanged();
            }
        }

        [UIValue("show-corrupted"), UsedImplicitly]
        private bool ShowCorruptedReplays {
            get => _showCorruptedReplays;
            set {
                _showCorruptedReplays = value;
                ShowCorruptedChangedEvent?.Invoke(value);
            }
        }

        public bool ShowCorrupted {
            get => ShowCorruptedReplays;
            set {
                ShowCorruptedReplays = value;
                NotifyPropertyChanged(nameof(ShowCorruptedReplays));
            }
        }

        private bool _showCorruptedInteractable = true;
        private bool _showCorruptedReplays;

        #endregion

        #region Modal

        [UIComponent("settings-modal")]
        private readonly ModalView _settingsModal = null!;

        protected override void OnInitialize() {
            _settingsModal.SetField("_animateParentCanvas", false);
            ShowCorruptedReplays = false;
            Sorter = Sorters.Date;
            RefreshSorters();
        }

        #endregion

        #region Callbacks

        [UIAction("select-order"), UsedImplicitly]
        private void HandleOrderTabSelected(SegmentedControl control, int cellIdx) {
            _ascendingSortOrder = cellIdx is 0;
            RefreshSorters();
        }

        [UIAction("reload-replays-list"), UsedImplicitly]
        private void HandleReloadButtonClicked() {
            ReloadDataEvent?.Invoke();
        }

        #endregion
    }
}