using UnityEngine;

namespace BeatLeader.Replayer.Binding {
    public abstract class GameHotkey {
        public abstract KeyCode Key { get; }

        public virtual void OnKeyDown() { }
        public virtual void OnKeyUp() { }
    }
}
