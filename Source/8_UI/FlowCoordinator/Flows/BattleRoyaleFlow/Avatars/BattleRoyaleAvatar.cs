using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Replayer.Emulation;
using BeatLeader.Utils;
using BeatSaber.BeatAvatarSDK;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleAvatar : MonoBehaviour {
        #region Pool

        public class Pool : MonoMemoryPool<BattleRoyaleReplay, BattleRoyaleAvatar> {
            public override void OnSpawned(BattleRoyaleAvatar item) {
                item.PresentAvatar();
            }

            public override void OnDespawned(BattleRoyaleAvatar item) {
                item.HideAvatar();
            }

            public override void Reinitialize(BattleRoyaleReplay replay, BattleRoyaleAvatar item) {
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
        private BattleRoyaleReplay? _replay;
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

                var player = await _replay.ReplayHeader.LoadPlayerAsync(false, token);
                _avatarData = await player.GetBeatAvatarAsync(false, token);

                if (token.IsCancellationRequested) {
                    return;
                }

                _avatarController.SetLoading(false);
                _avatarController.SetVisuals(_avatarData!, true);
            }
            
            _refreshTask = null;
        }

        private void Init(BattleRoyaleReplay replay) {
            if (_replay == replay) {
                return;
            }
            
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

        #endregion

        #region Enable & Disable

        public void PresentAvatar(bool animated = true) {
            _avatarController.gameObject.SetActive(true);
            _avatarController.Present(animated);
            Refresh(true);
        }

        public void HideAvatar() {
            _avatarController.Hide();
        }

        #endregion
    }
}