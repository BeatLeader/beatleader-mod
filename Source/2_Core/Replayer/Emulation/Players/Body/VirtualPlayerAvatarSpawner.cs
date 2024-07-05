using BeatLeader.Models;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayerAvatarSpawner : VirtualPlayerAvatarSpawnerBase {
        [Inject] private readonly VirtualPlayerAvatarBody.Pool _avatarPool = null!;

        public override IVirtualPlayerBodyModel PrimaryModel => Model;
        public override IVirtualPlayerBodyModel Model => VirtualPlayerAvatarBody.BodyModel;
        
        protected override IVirtualPlayerBodyComponent SpawnInternal(IVirtualPlayersManager playersManager, IVirtualPlayerBase player) {
            return _avatarPool.Spawn(player);
        }

        protected override void DespawnInternal(IVirtualPlayerBodyComponent body) {
            _avatarPool.Despawn((VirtualPlayerAvatarBody)body);
        }
    }
}