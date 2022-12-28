using BeatLeader.Models;

namespace BeatLeader.Replayer.Tweaking {
    public abstract class GameTweak : ZenjectComponentModel {
        /// <summary>
        /// Returns a boolean that describes, can tweak be installed or not
        /// </summary>
        public virtual bool CanBeInstalled { get; } = true;
    }
}
