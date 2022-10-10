using BeatLeader.Models;

namespace BeatLeader.Replayer.Tweaking
{
    internal abstract class GameTweak : ZenjectComponentModel
    {
        public virtual bool CanBeInstalled => true;
    }
}
