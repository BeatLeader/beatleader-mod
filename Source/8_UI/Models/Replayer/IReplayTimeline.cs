using System.Collections.Generic;

namespace BeatLeader.UI.Replayer {
    internal interface IReplayTimeline {
        IReadOnlyCollection<string> AvailableMarkers { get; }
         
        void SetMarkersEnabled(string name, bool enable = true);
        bool GetMarkersEnabled(string name);
    }
}
