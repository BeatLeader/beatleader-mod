using BeatLeader.Models;
using BeatLeader.Utils;
using Reactive;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class ReplaysListPanel : ReactiveComponent {
        #region Construct

        public ReplaysList ReplaysList => _replaysList;

        private ReplaysList _replaysList = null!;
        private ReplaysListSettingsPanel _settingsPanel = null!;

        protected override GameObject Construct() {
            var go = new Layout {
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
            return go;
        }

        protected override void OnInitialize() {
            _settingsPanel.Setup(_replaysList);
            CollectionExtensions.AddRange(_replaysList.Items, ReplayManager.Headers);

            _replaysList.Sort();
            _replaysList.Refresh();

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

        private IReplayHeader? _navigateHeader;
        private bool _listIsDirty;

        protected override void OnLateUpdate() {
            if (!_listIsDirty) {
                return;
            }
            
            _replaysList.Refresh();

            if (_navigateHeader != null) {
                _replaysList.Select(_navigateHeader);
                _replaysList.ScrollTo(_navigateHeader);
            }

            _listIsDirty = false;
        }

        private void SetListIsDirty(bool force = false) {
            const int multiplier = 30;

            if (force || _replaysList.Items.Count < _replaysList.VisibleCells * multiplier) {
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

        private void HandleLoadingFinished(bool _) {
            _replaysList.Sort();
            SetListIsDirty(true);
        }

        #endregion
    }
}