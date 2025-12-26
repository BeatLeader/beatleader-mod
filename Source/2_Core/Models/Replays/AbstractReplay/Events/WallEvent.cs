using System.Runtime.InteropServices;

namespace BeatLeader.Models.AbstractReplay {
    [StructLayout(LayoutKind.Auto)]
    public readonly struct WallEvent {
        public WallEvent(int wallId, float energy, float time, float spawnTime) {
            this.wallId = wallId;
            this.energy = energy;
            this.time = time;
            this.spawnTime = spawnTime;
        }
        
        public readonly int wallId;
        public readonly float energy;
        public readonly float time;
        public readonly float spawnTime;
    }
}
