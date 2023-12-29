using System.Linq;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Hub.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.UI.Hub {
    internal class ReplaysListPanel : ReeUIComponentV3<ReplaysListPanel> {
        #region UI Components

        public ReplaysList ReplaysList => _replaysList;
        
        [UIComponent("replays-list"), UsedImplicitly]
        private ReplaysList _replaysList = null!;

        [UIComponent("settings-panel"), UsedImplicitly]
        private ReplaysListSettingsPanel _settingsPanel = null!;

        #endregion

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

        protected override void OnInitialize() { }

        protected override bool OnValidation() {
            return _replaysLoader is not null;
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
            var items = _replaysList.items;
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
            _replaysList.items.Add(header);
            SetListIsDirty();
        }

        private void HandleReplayRemoved(IReplayHeader header) {
            _replaysList.items.Remove(header);
            SetListIsDirty();
        }

        private void HandleAllReplaysRemoved() {
            _replaysList.items.Clear();
            SetListIsDirty();
        }

        #endregion
    }
}