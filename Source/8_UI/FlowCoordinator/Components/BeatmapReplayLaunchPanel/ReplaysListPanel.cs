﻿using BeatLeader.Models;
using BeatLeader.UI.Hub.Models;
using Reactive;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class ReplaysListPanel : ReactiveComponent {
        #region Setup

        private IReplaysLoader? _replaysLoader;

        public void Setup(IReplaysLoader replaysLoader) {
            if (_replaysLoader is not null) {
                _replaysLoader.ReplayLoadedEvent -= HandleReplayAdded;
                _replaysLoader.ReplayRemovedEvent -= HandleReplayRemoved;
                _replaysLoader.AllReplaysRemovedEvent -= HandleAllReplaysRemoved;
            }
            lock (_locker) {
                _settingsPanel.Setup(_replaysList, replaysLoader);
            }
            _replaysLoader = replaysLoader;
            _replaysLoader.ReplayLoadedEvent += HandleReplayAdded;
            _replaysLoader.ReplayRemovedEvent += HandleReplayRemoved;
            _replaysLoader.AllReplaysRemovedEvent += HandleAllReplaysRemoved;
        }

        #endregion

        #region Construct

        public ReplaysList ReplaysList {
            get {
                lock (_locker) {
                    return _replaysList;
                }
            }
        }

        private ReplaysList _replaysList = null!;
        private ReplaysListSettingsPanel _settingsPanel = null!;

        protected override GameObject Construct() {
            var go = new Dummy {
                Children = {
                    new ReplaysList()
                        .AsFlexItem(grow: 1f)
                        // ReSharper disable once InconsistentlySynchronizedField
                        .Bind(ref _replaysList),
                    new ReplaysListSettingsPanel()
                        .AsFlexItem(basis: 4f)
                        .Bind(ref _settingsPanel)
                }
            }.AsFlexGroup(direction: FlexDirection.Column).Use();
            return go;
        }

        #endregion

        #region List Update

        private readonly object _locker = new();
        private bool _listIsDirty;

        protected override void OnLateUpdate() {
            if (!_listIsDirty) return;
            lock (_locker) {
                _replaysList.Refresh();
            }
            _listIsDirty = false;
        }

        private void SetListIsDirty() {
            _listIsDirty = true;
        }

        #endregion

        #region Callbacks

        private void HandleReplayAdded(IReplayHeader header) {
            lock (_locker) {
                _replaysList.Items.Add(header);
            }
            SetListIsDirty();
        }

        private void HandleReplayRemoved(IReplayHeader header) {
            lock (_locker) {
                _replaysList.Items.Remove(header);
            }
            SetListIsDirty();
        }

        private void HandleAllReplaysRemoved() {
            lock (_locker) {
                _replaysList.Items.Clear();
            }
            SetListIsDirty();
        }

        #endregion
    }
}