using System;
using System.Collections.Generic;
using BeatLeader.Models;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayerAvatarSpawner : VirtualPlayerBodyComponentSpawnerBase, IVirtualPlayerAvatarSpawner {
        [Inject] private readonly VirtualPlayerAvatarBody.Pool _avatarPool = null!;

        public override IVirtualPlayerBodyModel PrimaryModel => Model;
        public override IVirtualPlayerBodyModel Model => VirtualPlayerAvatarBody.BodyModel;

        protected override IEnumerable<IVirtualPlayerBodyComponent> SpawnedBodyComponents => _spawnedAvatars;

        private readonly List<VirtualPlayerAvatarBody> _spawnedAvatars = new();
        
        public IVirtualPlayerAvatar SpawnAvatar(IVirtualPlayersManager playersManager, IVirtualPlayerBase player) {
            var avatar = _avatarPool.Spawn(player);
            EnhanceComponent(avatar, playersManager, player);
            _spawnedAvatars.Add(avatar);
            return avatar;
        }

        public void DespawnAvatar(IVirtualPlayerAvatar avatar) {
            if (avatar is not VirtualPlayerAvatarBody avatarBody) {
                throw new InvalidOperationException("Unable to despawn the body which does not belong to the pool");
            }
            _spawnedAvatars.Remove(avatarBody);
            _avatarPool.Despawn(avatarBody);
        }

        protected override void ApplyConfig(IVirtualPlayerBodyConfig config) {
            ApplyPrimaryConfig(config);
        }
    }
}