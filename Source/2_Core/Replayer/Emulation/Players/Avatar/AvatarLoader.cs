using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    public class AvatarController : MonoBehaviour {
        public bool PlayAnimation {
            get => _animator.enabled;
            set {
                _animator.enabled = value;
                if (value) return;
                var trans = transform;
                trans.localPosition = Vector3.zero;
                trans.localEulerAngles = Vector3.zero;
            }
        }

        private void Awake() {
            TweenController = GetComponent<AvatarTweenController>();
            VisualController = GetComponentInChildren<AvatarVisualController>();
            PoseController = GetComponentInChildren<AvatarPoseController>();
            //TODO: asm pub
            HeadTransform = PoseController.GetField<Transform, AvatarPoseController>("_headTransform");
            _animator = GetComponent<Animator>();
        }

        public AvatarTweenController TweenController { get; private set; } = null!;
        public AvatarVisualController VisualController { get; private set; } = null!;
        public AvatarPoseController PoseController { get; private set; } = null!;
        public Transform HeadTransform { get; private set; } = null!;

        private Animator _animator = null!;
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
            return instance;
        }

        #endregion
    }
}