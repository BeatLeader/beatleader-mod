using BeatLeader.Models;
using BeatLeader.Replayer.Emulation;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleAvatar : MonoBehaviour {
        #region Pool

        public class Pool : MonoMemoryPool<IReplayHeaderBase, BattleRoyaleAvatar> {
            protected override void OnSpawned(BattleRoyaleAvatar item) {
                item.PresentAvatar();
            }

            protected override void OnDespawned(BattleRoyaleAvatar item) {
                item.HideAvatar();
            }

            protected override void Reinitialize(IReplayHeaderBase header, BattleRoyaleAvatar item) {
                item.Init(header);
            }
        }

        #endregion

        #region Injection
        
        [Inject] private readonly AvatarPartsModel _avatarPartsModel = null!;
        [Inject] private readonly AvatarLoader _avatarLoader = null!;

        #endregion

        #region Setup

        private AvatarTweenController AvatarTweenController => _avatarController.tweenController;
        private AvatarVisualController AvatarVisualController => _avatarController.visualController;
        
        private AvatarController _avatarController = null!;
        private readonly AvatarData _avatarData = new();

        private void Init(IReplayHeaderBase header) {
            var playerId = header.ReplayInfo!.PlayerID;
            AvatarUtils.RandomizeAvatarByPlayerId(playerId, _avatarData, _avatarPartsModel);
            AvatarVisualController.UpdateAvatarVisual(_avatarData);
        }

        private void Awake() {
            _avatarController = _avatarLoader.CreateAvatar(transform);
        }

        #endregion

        #region Enable & Disable

        public void PresentAvatar() {
            gameObject.SetActive(true);
            AvatarTweenController.gameObject.SetActive(true);
            AvatarTweenController.PresentAvatar();
        }

        public void HideAvatar() {
            AvatarTweenController.HideAvatar();
        }

        private void OnEnable() {
            PresentAvatar();
        }

        private void OnDisable() {
            HideAvatar();
        }

        #endregion
    }
}