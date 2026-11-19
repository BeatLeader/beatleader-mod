using Reactive;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleMenuStuffController : MonoBehaviour {
        #region Injection

        [Inject] private readonly IBattleRoyaleHost _battleRoyaleHost = null!;

        #endregion

        #region Setup

        private int _replaysCount;
        private bool _hostEnabled;

        private void Awake() {
            CreateFloatingText();
            transform.localPosition = new(0f, 0f, 4f);
            //
            _battleRoyaleHost.HostStateChangedEvent += HandleHostStateChanged;
            _battleRoyaleHost.ReplayAddedEvent += HandleReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent += HandleReplayRemoved;
        }

        private void OnDestroy() {
            _battleRoyaleHost.HostStateChangedEvent -= HandleHostStateChanged;
            _battleRoyaleHost.ReplayAddedEvent -= HandleReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent -= HandleReplayRemoved;
        }

        private void RefreshVisibility() {
            _floatingText.IsPresent = _hostEnabled && _replaysCount == 0;
        }

        #endregion

        #region FloatingText

        private BattleRoyaleFloatingMenuText _floatingText = null!;

        private void CreateFloatingText() {
            new BattleRoyaleFloatingMenuText {
                ContentTransform = {
                    localPosition = new(0f, 1.5f, 0f)
                }
            }.Bind(ref _floatingText).Use(transform);
        }

        #endregion

        #region Callbacks

        private void HandleHostStateChanged(bool state) {
            _hostEnabled = state;
            _replaysCount = _battleRoyaleHost.PendingReplays.Count;
            RefreshVisibility();
        }

        private void HandleReplayAdded(BattleRoyaleReplay replay, object caller) {
            if (!_hostEnabled) return;
            _replaysCount++;
            RefreshVisibility();
        }

        private void HandleReplayRemoved(BattleRoyaleReplay replay, object caller) {
            if (!_hostEnabled) return;
            _replaysCount--;
            RefreshVisibility();
        }

        #endregion
    }
}