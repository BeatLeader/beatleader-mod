using System.Collections.Generic;

namespace BeatLeader.Models {
    public interface IVirtualPlayerBodySpawner {
        IReadOnlyList<IVirtualPlayerBodyModel> BodyModels { get; }
        IReadOnlyDictionary<IVirtualPlayerBodyModel, IVirtualPlayerBodyConfig> BodyConfigs { get; }

        IControllableVirtualPlayerBody SpawnBody(
            IVirtualPlayersManager playersManager,
            IVirtualPlayerBase player
        );

        void DespawnBody(IVirtualPlayerBody body);
    }
}