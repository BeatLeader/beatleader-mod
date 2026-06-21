using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.Replayer.Binding {
    public abstract class GameHotkey {
        public abstract KeyCode Key { get; }

        public virtual IEnumerable<KeyCode> Keys {
            get {
                yield return Key;
            }
        }

        public virtual void OnKeyDown() { }
        public virtual void OnKeyUp() { }
    }
}
