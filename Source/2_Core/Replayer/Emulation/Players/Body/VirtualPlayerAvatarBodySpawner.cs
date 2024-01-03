using System;
using BeatLeader.Models;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayerAvatarBodySpawner : IVirtualPlayerBodySpawner {
        [Inject] private readonly VirtualPlayerAvatarBody.Pool _avatarBodyPool = null!;
        
        public IVirtualPlayerBodyModel BodyModel => VirtualPlayerAvatarBody.BodyModel;

        public IVirtualPlayerBody SpawnBody(IVirtualPlayerBase player) {
            return _avatarBodyPool.Spawn(player);
        }

        public void DespawnBody(IVirtualPlayerBody body) {
            if (body is not VirtualPlayerAvatarBody avatarBody) {
                throw new InvalidOperationException("Unable to despawn the body which does not belong to the pool");
            }
            _avatarBodyPool.Despawn(avatarBody);
        }
    }
}