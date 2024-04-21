using System.Linq;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Hub.Models;
using BeatLeader.UI.Reactive;
using UnityEngine;
using FlexDirection = BeatLeader.UI.Reactive.Yoga.FlexDirection;

namespace BeatLeader.UI.Hub {
    internal class ReplaysListPanel : ReeUIComponentV3<ReplaysListPanel> {
        #region Setup

        private IReplaysLoader? _replaysLoader;

        public void Setup(IReplaysLoader replaysLoader) {
            if (_replaysLoader is not null) {
                _replaysLoader.ReplayLoadedEvent -= HandleReplayAdded;
                _replaysLoader.ReplayRemovedEvent -= HandleReplayRemoved;
                _replaysLoader.AllReplaysRemovedEvent -= HandleAllReplaysRemoved;
            }
            _settingsPanel.Setup(_replaysList, replaysLoader);
            _replaysLoader = replaysLoader;
            _replaysLoader.ReplayLoadedEvent += HandleReplayAdded;
            _replaysLoader.ReplayRemovedEvent += HandleReplayRemoved;
            _replaysLoader.AllReplaysRemovedEvent += HandleAllReplaysRemoved;
        }

        protected override bool OnValidation() {
            return _replaysLoader is not null;
        }

        #endregion

        #region Construct

        public ReplaysList ReplaysList => _replaysList;

        private ReplaysList _replaysList = null!;
        private ReplaysListSettingsPanel _settingsPanel = null!;

        protected override GameObject Construct(Transform parent) {
            ReactiveComponentBase componentBase = null!;
            var go = new UI.Reactive.Components.Dummy {
                Children = {
                    new ReplaysList()
                        .AsFlexItem(grow: 1f)
                        .Bind(ref _replaysList),
                    new UI.Reactive.Components.Dummy()
                        .AsFlexItem(basis: 5f)
                        .Bind(ref componentBase)
                }
            }.AsFlexGroup(direction: FlexDirection.Column).Use(parent);
            _settingsPanel = ReplaysListSettingsPanel.Instantiate(componentBase.ContentTransform);
            return go;
        }

        #endregion

        #region Filter

        public IReplayFilter? Filter {
            get => _replayFilter;
            set {
                if (_replayFilter is not null) {
                    _replayFilter.FilterUpdatedEvent -= HandleFilterUpdated;
                }
                _replayFilter = value;
                if (_replayFilter is not null) {
                    _replayFilter.FilterUpdatedEvent += HandleFilterUpdated;
                }
                RefreshFilter();
            }
        }

        private IReplayFilter? _replayFilter;

        private void RefreshFilter() {
            ValidateAndThrow();
            var loadedReplays = _replaysLoader!.LoadedReplays;
            var items = _replaysList.Items;
            items.Clear();
            SetListIsDirty();
            if (_replayFilter is null) {
                items.AddRange(loadedReplays);
                return;
            }
            items.AddRange(loadedReplays.Where(FilterItem));
        }

        private bool FilterItemOrTrue(IReplayHeaderBase header) {
            return _replayFilter is null || FilterItem(header);
        }

        private bool FilterItem(IReplayHeaderBase header) {
            return _replayFilter!.MatchesFilter(header.ReplayInfo);
        }

        #endregion

        #region List Update

        private bool _listIsDirty;

        private void LateUpdate() {
            if (!_listIsDirty) return;
            _replaysList.Refresh();
            _listIsDirty = false;
        }

        private void SetListIsDirty() {
            _listIsDirty = true;
        }

        #endregion

        #region Callbacks

        private void HandleFilterUpdated() {
            RefreshFilter();
        }

        private void HandleReplayAdded(IReplayHeader header) {
            if (!FilterItemOrTrue(header)) return;
            _replaysList.Items.Add(header);
            SetListIsDirty();
        }

        private void HandleReplayRemoved(IReplayHeader header) {
            _replaysList.Items.Remove(header);
            SetListIsDirty();
        }

        private void HandleAllReplaysRemoved() {
            _replaysList.Items.Clear();
            SetListIsDirty();
        }

        #endregion
    }
}