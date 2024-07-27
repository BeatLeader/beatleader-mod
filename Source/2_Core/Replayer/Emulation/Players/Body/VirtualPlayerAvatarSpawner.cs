using System.Collections.Generic;
using BeatLeader.Models;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayerAvatarSpawner : VirtualPlayerAvatarSpawnerBase {
        [Inject] private readonly VirtualPlayerAvatarBody.Pool _avatarPool = null!;

        public override IVirtualPlayerBodyModel PrimaryModel => Model;
        public override IVirtualPlayerBodyModel Model => VirtualPlayerAvatarBody.BodyModel;

        private readonly Dictionary<VirtualPlayerAvatarBody, VirtualPlayerAvatarData> _avatarDatas = new();

        protected override IVirtualPlayerBodyComponent SpawnInternal(IVirtualPlayersManager playersManager, IVirtualPlayerBase player) {
            var data = new VirtualPlayerAvatarData(player) {
                DefaultConfig = Config!,
                PrimaryConfig = PrimaryConfig!
            };
            var avatar = _avatarPool.Spawn(data);
            _avatarDatas.Add(avatar, data);
            return avatar;
        }

        public override void ApplyModelConfig(bool applyToPrimaryModel, IVirtualPlayerBodyConfig config) {
            foreach (var data in _avatarDatas.Values) {
                if (applyToPrimaryModel) {
                    data.PrimaryConfig = config;
                } else {
                    data.DefaultConfig = config;
                }
            }
            base.ApplyModelConfig(applyToPrimaryModel, config);
        }

        protected override void DespawnInternal(IVirtualPlayerBodyComponent body) {
            var avatar = (VirtualPlayerAvatarBody)body;
            _avatarPool.Despawn(avatar);
            _avatarDatas.Remove(avatar);
        }
    }
}