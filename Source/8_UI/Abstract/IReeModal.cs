using System;

namespace BeatLeader {
    public interface IReeModal {
        public void Resume(object state, Action closeAction);
        public void Pause();
        public void Interrupt();
        public void Close();
        public void HandleOffClick();
    }
}