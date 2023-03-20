using System;

namespace BeatLeader.Models {
    public interface IReplayFinishController {
        bool ExitAutomatically { get; }

        event Action ReplayWasLeftEvent;
        event Action ReplayWasFinishedEvent;

        void Exit();
    }
}
