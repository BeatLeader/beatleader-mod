using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Utils;
using Reactive;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class ReplaysListPanel : ReactiveComponent {
        #region Construct

        public ReplaysList ReplaysList => _replaysList;

        private ReplaysList _replaysList = null!;
        private ReplaysListSettingsPanel _settingsPanel = null!;

        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    new ReplaysList()
                        .AsFlexItem(flexGrow: 1f)
                        // ReSharper disable once InconsistentlySynchronizedField
                        .Bind(ref _replaysList),

                    new ReplaysListSettingsPanel()
                        .AsFlexItem(basis: 4f)
                        .Bind(ref _settingsPanel)
                }
            }.AsFlexGroup(direction: FlexDirection.Column).Use();
        }

        protected override void OnInitialize() {
            _settingsPanel.Setup(_replaysList);
            _headers.AddRange(ReplayManager.Headers);
            _replaysList.Items = _headers;

            ReplayManager.ReplayAddedEvent += HandleReplayAdded;
            ReplayManager.ReplayDeletedEvent += HandleReplayRemoved;
            ReplayManager.AllReplaysDeletedEvent += HandleAllReplaysRemoved;
            ReplayManager.LoadingFinishedEvent += HandleLoadingFinished;
        }

        protected override void OnDestroy() {
            ReplayManager.ReplayAddedEvent -= HandleReplayAdded;
            ReplayManager.ReplayDeletedEvent -= HandleReplayRemoved;
            ReplayManager.AllReplaysDeletedEvent -= HandleAllReplaysRemoved;
            ReplayManager.LoadingFinishedEvent -= HandleLoadingFinished;
        }

        #endregion

        #region List Update

        private readonly List<IReplayHeader> _headers = new();
        private IReplayHeader? _navigateHeader;
        private bool _listIsDirty;

        protected override void OnLateUpdate() {
            if (!_listIsDirty) {
                return;
            }

            _replaysList.Items = _headers;

            if (_navigateHeader != null) {
                _replaysList.Select(_navigateHeader);
                _replaysList.ScrollTo(_navigateHeader);
            }

            _listIsDirty = false;
        }

        private void SetListIsDirty(bool force = false) {
            const int multiplier = 30;

            if (force || _headers.Count < _replaysList.VisibleCells * multiplier) {
                _listIsDirty = true;
            }
        }

        public void QueueNavigation(IReplayHeader header) {
            _navigateHeader = header;
            SetListIsDirty();
        }

        #endregion

        #region Callbacks

        private void HandleReplayAdded(IReplayHeader header) {
            _headers.Add(header);
            SetListIsDirty();
        }

        private void HandleReplayRemoved(IReplayHeader header) {
            _headers.Remove(header);
            SetListIsDirty(true);
        }

        private void HandleAllReplaysRemoved() {
            _headers.Clear();
            SetListIsDirty(true);
        }

        private void HandleLoadingFinished(bool _) {
            SetListIsDirty(true);
        }

        #endregion
    }
}