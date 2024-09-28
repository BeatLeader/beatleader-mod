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
        [Inject] private readonly BeatAvatarSystemSettings _beatAvatarSettings = null!;
        [Inject] private readonly DiContainer _container = null!;

        private IAvatarSystem _avatarSystem = null!;
        private GameObject _avatarPrefab = null!;

        private void Awake() {
            var meta = _avatarSystemCollection.availableAvatarSystems[0];
            _avatarSystem = _avatarSystemCollection.GetAvatarSystem(meta);
            _avatarPrefab = Resources
                .FindObjectsOfTypeAll<AvatarTweenController>()
                .First(static x => x.name == "AnimatedAvatar")
                .gameObject;
        }

        #endregion

        #region CreateAvatar

        public BeatAvatarController CreateGameplayAvatar(Transform? parent = null) {
            var prefab = _beatAvatarSettings.avatarSelectionViewPrefab.Asset;
            var avatar = CreateAvatar(prefab, parent, 1f);
            // For camera2 support
            foreach (var item in transform.GetChildren(false)) {
                item.gameObject.layer = 10;
            }
            return avatar.AddComponent<BeatAvatarController>();
        }

        public MenuBeatAvatarController CreateMenuAvatar(Transform? parent = null, float size = 1.2f) {
            var avatar = CreateAvatar(_avatarPrefab, parent, size);
            return avatar.AddComponent<MenuBeatAvatarController>();
        }

        private GameObject CreateAvatar(Object prefab, Transform? parent, float size) {
            var avatar = (GameObject)Instantiate(prefab, parent, false);
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