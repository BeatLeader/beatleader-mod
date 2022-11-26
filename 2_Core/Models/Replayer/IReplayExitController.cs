using System;

namespace BeatLeader.Models {
    public interface IReplayExitController {
        event Action ReplayExitEvent;

        void Exit();
    }
}
