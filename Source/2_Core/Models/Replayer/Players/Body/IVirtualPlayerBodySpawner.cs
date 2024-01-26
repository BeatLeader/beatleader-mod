using System.Collections.Generic;

namespace BeatLeader.Models {
    public interface IVirtualPlayerBodySpawner {
        IReadOnlyList<IVirtualPlayerBodyModel> BodyModels { get; }

        void ApplyModelConfig(
            IVirtualPlayerBodyModel model,
            VirtualPlayerBodyConfig config
        );

        IControllableVirtualPlayerBody SpawnBody(
            IVirtualPlayersManager playersManager,
            IVirtualPlayerBase player
        );

        void DespawnBody(IVirtualPlayerBody body);
    }
}