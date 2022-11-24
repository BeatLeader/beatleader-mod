using BeatLeader.Models;

namespace BeatLeader.Replayer.Tweaking
{
    public abstract class GameTweak : ZenjectComponentModel
    {
        public virtual bool CanBeInstalled => true;
    }
}
