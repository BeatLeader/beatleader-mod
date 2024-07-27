using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    public class AvatarLoader : MonoBehaviour {
        #region Injection

        [Inject] private readonly EditAvatarFlowCoordinator _editAvatarFlowCoordinator = null!;
        [Inject] private readonly DiContainer _diContainer = null!;

        #endregion

        #region Setup

        private void Awake() {
            //TODO: asm pub
            var tweenController = _editAvatarFlowCoordinator.GetField<AvatarTweenController, EditAvatarFlowCoordinator>("_avatarTweenController");
            _prefab = tweenController.gameObject;
            _prefab = InstantiateAvatar(null);
            _prefab.gameObject.SetActive(false);
        }

        #endregion

        #region CreateAvatar

        public AvatarController CreateAvatar(Transform? parent = null) {
            var go = InstantiateAvatar(parent);
            return go.AddComponent<AvatarController>();
        }

        #endregion

        #region InstantiateAvatar

        private GameObject _prefab = null!;

        private GameObject InstantiateAvatar(Transform? parent) {
            var instance = Instantiate(_prefab, parent, false);
            instance.SetActive(true);
            _diContainer.InjectGameObject(instance);
            var trans = instance.transform;
            trans.localPosition = Vector3.zero;
            trans.localEulerAngles = Vector3.zero;
            trans.localScale = Vector3.one * 1.35f;
            return instance;
        }

        #endregion
    }
}