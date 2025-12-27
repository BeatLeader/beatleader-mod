using System.Runtime.InteropServices;

namespace BeatLeader.Models.AbstractReplay {
    [StructLayout(LayoutKind.Auto)]
    public readonly struct PauseEvent {
        public PauseEvent(float time, long duration) {
            this.time = time;
            this.duration = duration;
        }
        
        public readonly float time;
        public readonly long duration;
    }
}
