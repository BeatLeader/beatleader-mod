using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Replayer.Emulation;
using BeatLeader.Utils;
using BeatSaber.AvatarCore;
using BeatSaber.BeatAvatarSDK;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleAvatar : MonoBehaviour {
        #region Pool

        public class Pool : MonoMemoryPool<IBattleRoyaleReplay, BattleRoyaleAvatar> {
            public override void OnSpawned(BattleRoyaleAvatar item) {
                item.PresentAvatar();
            }

            public override void OnDespawned(BattleRoyaleAvatar item) {
                item.HideAvatar();
            }

            public override void Reinitialize(IBattleRoyaleReplay replay, BattleRoyaleAvatar item) {
                item.Init(replay);
            }

            public override void OnCreated(BattleRoyaleAvatar item) { }
        }

        #endregion

        #region Injection

        [Inject] private readonly BeatAvatarLoader _beatAvatarLoader = null!;
        [Inject] private readonly MainCamera _mainCamera = null!;

        #endregion

        #region Setup

        private MenuBeatAvatarController _avatarController = null!;
        private FloatingBattleRoyaleReplayBadge _badge = null!;
        private AvatarData? _avatarData;
        private IBattleRoyaleReplay? _replay;
        private CancellationTokenSource _tokenSource = new();
        private Task? _refreshTask;

        public void Refresh(bool force = false) {
            if (_refreshTask != null) {
                _tokenSource.Cancel();
                _tokenSource = new();
            }
            _refreshTask = RefreshInternal(force, _tokenSource.Token).RunCatching();
        }

        private async Task RefreshInternal(bool force, CancellationToken token) {
            if (_replay == null) {
                _refreshTask = null;
                return;
            }
            _badge.SetData(_replay);
            if (_avatarData == null || force) {
                _avatarController.SetVisuals(null);
                _avatarController.SetLoading(true);
                var replayData = await _replay.GetReplayDataAsync(force);
                _avatarData = replayData.AvatarData;
                if (token.IsCancellationRequested) return;
                _avatarController.SetLoading(false);
                _avatarController.SetVisuals(_avatarData!, true);
            }
            _refreshTask = null;
        }

        private void Init(IBattleRoyaleReplay replay) {
            if (_replay == replay) return;
            _avatarData = null;
            _replay = replay;
            Refresh();
        }

        private void Awake() {
            _avatarController = _beatAvatarLoader.CreateMenuAvatar(transform);
            _badge = new();
            _badge.Use(_avatarController.PoseController._headTransform);
            _badge.Setup(_mainCamera.transform);
            _badge.ContentTransform.localPosition = new(0f, 0.45f, -0.13f);
        }

        private void OnEnable() {
            PresentAvatar();
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