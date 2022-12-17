using System;

namespace BeatLeader.Models {
    public interface IReplayFinishController {
        bool ExitAutomatically { get; }

        event Action ReplayWasExitedEvent;
        event Action ReplayWasFinishedEvent;

        void Exit();
    }
}
