using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Hub.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;

namespace BeatLeader.UI.Hub {
    internal class ReplaysListSettingsPanel : ReeUIComponentV3<ReplaysListSettingsPanel> {
        #region Sort & Order

        private ReplaysListSorter ReplaysListSorter {
            get => _replaysListSorter;
            set {
                _replaysListSorter = value;
                NotifyPropertyChanged(nameof(StringSorter));
                RefreshSorters();
            }
        }

        [UIValue("sorter"), UsedImplicitly]
        private string StringSorter {
            get => _replaysListSorter.ToString();
            set {
                _replaysListSorter = StringConverter.Convert<ReplaysListSorter>(value);
                RefreshSorters();
            }
        }

        [UIValue("sorters"), UsedImplicitly]
        private readonly List<object> _localSorters = sorters;

        [UIValue("orders"), UsedImplicitly]
        private readonly List<string> _localOrders = orders;

        private static readonly List<object> sorters = Enum.GetNames(typeof(ReplaysListSorter)).ToList<object>();

        private static readonly List<string> orders = Enum.GetNames(typeof(SortOrder)).ToList();

        private SortOrder _sortOrder;
        private ReplaysListSorter _replaysListSorter;

        private void RefreshSorters() {
            ValidateAndThrow();
            _replaysList!.Sorter = _replaysListSorter;
            _replaysList.SortOrder = _sortOrder;
        }

        #endregion

        #region Modal

        [UIComponent("settings-modal")]
        private readonly ModalView _settingsModal = null!;
        
        private void ShowModal() {
            _settingsModal.Show(true);
        }

        #endregion

        #region Setup

        private IReplaysList? _replaysList;
        private IReplaysLoader? _replaysLoader;
        
        public void Setup(IReplaysList replaysList, IReplaysLoader replaysLoader) {
            _replaysList = replaysList;
            _replaysLoader = replaysLoader;
            RefreshSorters();
        }

        protected override bool OnValidation() {
            return _replaysList is not null && _replaysLoader is not null;
        }

        protected override void OnInitialize() {
            _settingsModal.SetField("_animateParentCanvas", false);
            _replaysListSorter = ReplaysListSorter.Date;
        }

        #endregion

        #region Callbacks

        [UIAction("order-tab-select"), UsedImplicitly]
        private void HandleOrderTabSelected(string tabName) {
            _sortOrder = StringConverter.Convert<SortOrder>(tabName);
            RefreshSorters();
        }

        [UIAction("reload-click"), UsedImplicitly]
        private void HandleReloadButtonClicked(bool state) {
            ValidateAndThrow();
            _replaysLoader!.StartReplaysLoad();
        }

        [UIAction("settings-click"), UsedImplicitly]
        private void HandleSettingsButtonClicked(bool state) {
            ShowModal();
        }

        #endregion
    }
}