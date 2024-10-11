using System.Linq;
using System.Threading.Tasks;
using BeatLeader.Utils;
using BeatSaber.AvatarCore;
using BeatSaber.BeatAvatarAdapter;
using BeatSaber.BeatAvatarAdapter.AvatarEditor;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    public class BeatAvatarLoader : MonoBehaviour {
        #region Setup

        [Inject] private readonly AvatarSystemCollection _avatarSystemCollection = null!;
        [Inject] private readonly DiContainer _container = null!;

        private GameObject AvatarPrefab {
            get {
                if (!_avatarPrefab) {
                    _avatarPrefab = Resources
                        .FindObjectsOfTypeAll<AvatarTweenController>()
                        .First(static x => x.name == "AnimatedAvatar")
                        .gameObject;
                }
                return _avatarPrefab;
            }
        }
        
        private IAvatarSystem _avatarSystem = null!;
        private GameObject _avatarPrefab = null!;

        private void Awake() {
            var meta = _avatarSystemCollection.availableAvatarSystems[0];
            _avatarSystem = _avatarSystemCollection.GetAvatarSystem(meta);
        }

        #endregion

        #region CreateAvatar

        public BeatAvatarController CreateGameplayAvatar(Transform? parent = null) {
            var avatar = CreateAvatar(parent, 1f);
            // For camera2 support
            foreach (var item in avatar.transform.GetChildren(false)) {
                item.gameObject.layer = 10;
            }
            // Forcibly enable to initiate Awake
            avatar.gameObject.SetActive(true);
            return avatar.AddComponent<BeatAvatarController>();
        }

        public MenuBeatAvatarController CreateMenuAvatar(Transform? parent = null, float size = 1.2f) {
            var avatar = CreateAvatar(parent, size);
            return avatar.AddComponent<MenuBeatAvatarController>();
        }

        private GameObject CreateAvatar(Transform? parent, float size) {
            var avatar = Instantiate(AvatarPrefab, parent, false);
            var trans = avatar.transform;
            trans.localPosition = Vector3.zero;
            trans.localScale = size * Vector3.one;
            trans.localRotation = Quaternion.identity;
            _container.InjectGameObject(avatar);
            return avatar;
        }

        #endregion

        #region CreateEditor

        public async Task<BeatAvatarEditorFlowCoordinator> CreateEditorFlowCoordinator() {
            return (BeatAvatarEditorFlowCoordinator)await _avatarSystem.InstantiateAvatarEditorUI(_container);
        }

        #endregion
    }
}