using System;

namespace BeatLeader.Models {
    internal interface IReplayTimeline {
        event Action MarkersWereGeneratedEvent;

        void ShowMarkers(string name, bool show = true);
    }
}
