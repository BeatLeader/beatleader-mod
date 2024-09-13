using BeatSaber.AvatarCore;
using BeatSaber.BeatAvatarAdapter;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    public class BeatAvatarLoader : MonoBehaviour {
        [Inject] private readonly AvatarSystemCollection _avatarSystemCollection = null!;

        public IAvatarSystem AvatarSystem { get; private set; } = null!;

        private void Awake() {
            var meta = _avatarSystemCollection.availableAvatarSystems[0];
            AvatarSystem = _avatarSystemCollection.GetAvatarSystem(meta);
        }

        public BeatAvatarController CreateAvatar(AvatarDisplayContext context, Transform? parent = null) {
            var task = AvatarSystem.InstantiateAvatar(context, 0);
            task.Wait();
            var avatar = (BeatAvatar)task.Result;
            avatar.transform.SetParent(parent, false);
            return avatar.gameObject.AddComponent<BeatAvatarController>();
        }
        
        public BeatAvatarEditorFlowCoordinator CreateEditorFlowCoordinator() {
            var task = AvatarSystem.InstantiateAvatarEditorUI();
            task.Wait();
            return (BeatAvatarEditorFlowCoordinator)task.Result;
        }
    }
}