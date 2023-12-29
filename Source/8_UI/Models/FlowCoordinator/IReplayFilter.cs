using System;
using BeatLeader.Models;

namespace BeatLeader.UI.Hub.Models {
    internal interface IReplayFilter {
        event Action FilterUpdatedEvent;

        bool MatchesFilter(IReplayInfo? info);
    }
}