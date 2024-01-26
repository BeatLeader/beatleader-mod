using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    public class AvatarController {
        public AvatarController(
            GameObject container,
            AvatarTweenController tweenController,
            AvatarVisualController visualController,
            AvatarPoseController poseController,
            Animator animator
        ) {
            containerObject = container;
            containerTransform = container.transform;
            this.tweenController = tweenController;
            this.visualController = visualController;
            this.poseController = poseController;
            _animator = animator;
        }

        public bool PlayAnimation {
            get => _animator.enabled;
            set {
                _animator.enabled = value;
                if (value) return;
                containerTransform.localPosition = Vector3.zero;
                containerTransform.localEulerAngles = Vector3.zero;
            }
        }

        public readonly GameObject containerObject;
        public readonly Transform containerTransform;
        public readonly AvatarTweenController tweenController;
        public readonly AvatarVisualController visualController;
        public readonly AvatarPoseController poseController;

        private readonly Animator _animator;
    }

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
        }

        #endregion

        #region CreateAvatar

        public AvatarController CreateAvatar(Transform? parent = null) {
            var go = InstantiateAvatar(parent);
            var tweenController = go.GetComponent<AvatarTweenController>();
            var visualController = go.GetComponentInChildren<AvatarVisualController>();
            var poseController = go.GetComponentInChildren<AvatarPoseController>();
            var animator = go.GetComponent<Animator>();
            return new(go, tweenController, visualController, poseController, animator);
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
            return instance;
        }

        #endregion
    }
}