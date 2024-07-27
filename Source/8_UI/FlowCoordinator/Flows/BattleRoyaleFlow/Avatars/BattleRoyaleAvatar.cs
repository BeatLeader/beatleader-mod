using BeatLeader.Replayer.Emulation;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleAvatar : MonoBehaviour {
        #region Pool

        public class Pool : MonoMemoryPool<IBattleRoyaleReplay, BattleRoyaleAvatar> {
            protected override void OnSpawned(BattleRoyaleAvatar item) {
                item.PresentAvatar();
            }

            protected override void OnDespawned(BattleRoyaleAvatar item) {
                item.HideAvatar();
            }

            protected override void Reinitialize(IBattleRoyaleReplay replay, BattleRoyaleAvatar item) {
                item.Init(replay);
            }

            protected override void OnCreated(BattleRoyaleAvatar item) { }
        }

        #endregion

        #region Injection

        [Inject] private readonly AvatarLoader _avatarLoader = null!;
        [Inject] private readonly MainCamera _mainCamera = null!;

        #endregion

        #region Setup

        private AvatarController _avatarController = null!;
        private FloatingBattleRoyaleReplayBadge _badge = null!;
        private AvatarData? _avatarData;
        private IBattleRoyaleReplay? _replay;

        public async void Refresh(bool force = false) {
            if (_replay == null) return;
            _badge.SetData(_replay);
            if (_avatarData == null || force) {
                _avatarController.SetVisuals(null);
                _avatarController.SetLoading(true);
                var replayData = await _replay.GetReplayDataAsync(force);
                _avatarData = replayData.AvatarData;
                _avatarController.SetVisuals(_avatarData!, true);
                _avatarController.SetLoading(false);
            }
        }

        private void Init(IBattleRoyaleReplay replay) {
            if (_replay == replay) return;
            _avatarData = null;
            _replay = replay;
            Refresh();
        }

        private void Awake() {
            _avatarController = _avatarLoader.CreateAvatar(transform);
            _badge = new();
            _badge.Use(_avatarController.HeadTransform);
            _badge.Setup(_mainCamera.transform);
            _badge.ContentTransform.localPosition = new(0f, 0.45f, -0.13f);
        }

        #endregion

        #region Enable & Disable

        public void PresentAvatar() {
            _avatarController.gameObject.SetActive(true);
            _avatarController.Present();
            Refresh(true);
        }

        public void HideAvatar() {
            _avatarController.Hide();
        }

        #endregion
    }
}