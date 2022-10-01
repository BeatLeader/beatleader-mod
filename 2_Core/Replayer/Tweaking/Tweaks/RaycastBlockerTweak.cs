namespace BeatLeader.Replayer.Tweaking
{
    internal class RaycastBlockerTweak : GameTweak
    {
        public override void Initialize()
        {
            RaycastBlocker.EnableBlocker = true;
        }
        public override void Dispose()
        {
            RaycastBlocker.EnableBlocker = false;
        }
    }
}
