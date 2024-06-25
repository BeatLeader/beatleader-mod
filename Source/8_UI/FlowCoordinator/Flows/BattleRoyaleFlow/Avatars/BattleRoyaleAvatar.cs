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

        private AvatarTweenController AvatarTweenController => _avatarController.TweenController;
        private AvatarVisualController AvatarVisualController => _avatarController.VisualController;

        private AvatarController _avatarController = null!;
        private FloatingBattleRoyaleReplayBadge _badge = null!;
        private AvatarData? _avatarData;
        private IBattleRoyaleReplay? _replay;

        public void Refresh() {
            if (_replay == null) return;
            _avatarData = _replay.ReplayData.AvatarData;
            AvatarVisualController.UpdateAvatarVisual(_avatarData);
            _badge.SetData(_replay);
        }

        private void Init(IBattleRoyaleReplay replay) {
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
            AvatarTweenController.gameObject.SetActive(true);
            AvatarTweenController.PresentAvatar();
        }

        public void HideAvatar() {
            AvatarTweenController.HideAvatar();
        }

        private void OnEnable() {
            PresentAvatar();
        }

        #endregion
    }
}