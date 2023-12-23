﻿using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;
using static BeatLeader.Components.BeatmapReplayLaunchPanel;
using static BeatLeader.Components.ReplaysList;

namespace BeatLeader.Components {
    internal class ReplaysListSettingsPanel : ReeUIComponentV3<ReplaysListSettingsPanel> {
        #region Events

        [ExternalProperty, UsedImplicitly]
        public event Action<Sorter, SortOrder>? SorterChangedEvent;

        [ExternalProperty, UsedImplicitly]
        public event Action<ReplayMode>? ReplayModeChangedEvent;

        [ExternalProperty, UsedImplicitly]
        public event Action? ReloadDataEvent;

        #endregion

        #region UI Components

        [UIComponent("tab-control")]
        private readonly TextTabSelector _textTabControl = null!;

        #endregion

        #region Sort & Order

        public SortOrder SortOrder {
            get => _sortOrder;
            set {
                _sortOrder = value;
                _textTabControl.SelectTab(_sorter.ToString());
                RefreshSorters();
            }
        }

        public Sorter Sorter {
            get => _sorter;
            set {
                _sorter = value;
                NotifyPropertyChanged(nameof(StringSorter));
                RefreshSorters();
            }
        }

        [UIValue("sorter"), UsedImplicitly]
        private string StringSorter {
            get => _sorter.ToString();
            set {
                _sorter = StringConverter.Convert<Sorter>(value);
                RefreshSorters();
            }
        }

        [UIValue("sorters"), UsedImplicitly]
        private readonly List<object> _localSorters = sorters;

        [UIValue("orders"), UsedImplicitly]
        private readonly List<string> _localOrders = orders;

        private static readonly List<object> sorters = Enum.GetNames(typeof(Sorter)).ToList<object>();

        private static readonly List<string> orders = Enum.GetNames(typeof(SortOrder)).ToList();

        private SortOrder _sortOrder;
        private Sorter _sorter;

        private void RefreshSorters() {
            SorterChangedEvent?.Invoke(_sorter, _sortOrder);
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

        protected override void OnInitialize() {
            _settingsModal.SetField("_animateParentCanvas", false);
            Sorter = Sorter.Date;
            RefreshSorters();
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
            ReloadDataEvent?.Invoke();
        }

        [UIAction("settings-click"), UsedImplicitly]
        private void HandleSettingsButtonClicked(bool state) {
            ShowModal();
        }

        [UIAction("battle-royale-click"), UsedImplicitly]
        private void HandleBattleRoyaleButtonClicked(bool state) {
            ReplayModeChangedEvent?.Invoke(state ? ReplayMode.BattleRoyale : ReplayMode.Standard);
        }

        #endregion
    }
}