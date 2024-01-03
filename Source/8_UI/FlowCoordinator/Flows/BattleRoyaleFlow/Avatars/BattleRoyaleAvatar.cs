using BeatLeader.Models;
using BeatLeader.Utils;
using IPA.Utilities;
using UnityEngine;
using Zenject;
using Random = System.Random;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleAvatar : MonoBehaviour {
        #region Pool

        public class Pool : MonoMemoryPool<BattleRoyaleAvatar> {
            protected override void OnSpawned(BattleRoyaleAvatar item) {
                item.PresentAvatar();
            }

            protected override void OnDespawned(BattleRoyaleAvatar item) {
                item.HideAvatar();
            }
        }

        #endregion

        #region Injection

        [Inject] private readonly EditAvatarFlowCoordinator _editAvatarFlowCoordinator = null!;
        [Inject] private readonly AvatarPartsModel _avatarPartsModel = null!;
        [Inject] private readonly DiContainer _diContainer = null!;

        #endregion

        #region Setup

        private AvatarTweenController _avatarTweenController = null!;
        private AvatarVisualController _avatarVisualController = null!;
        private readonly AvatarData _avatarData = new();

        public void Init(IReplayHeaderBase header) {
            var playerId = header.ReplayInfo!.PlayerID;
            AvatarUtils.RandomizeAvatarByPlayerId(playerId, _avatarData, _avatarPartsModel);
            _avatarVisualController.UpdateAvatarVisual(_avatarData);
        }

        private void Awake() {
            _avatarTweenController = InstantiateAvatar();
            _avatarVisualController = _avatarTweenController.GetComponentInChildren<AvatarVisualController>();
        }

        private AvatarTweenController InstantiateAvatar() {
            //TODO: asm pub
            var prefab = _editAvatarFlowCoordinator.GetField<AvatarTweenController, EditAvatarFlowCoordinator>("_avatarTweenController");
            var instance = Instantiate(prefab, transform, false);
            _diContainer.InjectGameObject(instance.gameObject);
            var trans = instance.transform;
            trans.localPosition = Vector3.zero;
            trans.localEulerAngles = Vector3.zero;
            return instance;
        }

        #endregion

        #region Enable & Disable

        public void PresentAvatar() {
            gameObject.SetActive(true);
            _avatarTweenController.gameObject.SetActive(true);
            _avatarTweenController.PresentAvatar();
        }

        public void HideAvatar() {
            _avatarTweenController.HideAvatar();
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