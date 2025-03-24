using JetBrains.Annotations;
using Reactive;

namespace BeatLeader.Models {
    [PublicAPI]
    public interface IVirtualPlayerBodySpawner {
        /// <summary>
        /// Used for features like FPV camera so we can see clearly.
        /// </summary>
        bool BodyHeadsVisible { get; set; }
        
        /// <summary>
        /// Spawns a body for the specified player.
        /// </summary>
        IVirtualPlayerBody SpawnBody(IVirtualPlayer player);

        /// <summary>
        /// Despawns the specified body.
        /// </summary>
        void DespawnBody(IVirtualPlayerBody body);
    }
}