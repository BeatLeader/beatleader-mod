using System;

namespace BeatLeader.Models {
    internal interface IReplayTimeline {
        event Action MarkersWasGeneratedEvent;

        void ShowMarkers(string name, bool show = true);
        void ClearMarkers(string name);
    }
}
