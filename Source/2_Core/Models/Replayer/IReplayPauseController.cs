using System;

namespace BeatLeader.Models {
    public interface IReplayPauseController {
        bool LockUnpause { get; set; }
        bool IsPaused { get; }

        event Action<bool> PauseStateChangedEvent;

        void Pause(bool notifyListeners = true, bool forcePause = false);
        void Resume(bool notifyListeners = true, bool forceResume = false);
    }
}
