using BeatLeader.Interop;
using BeatLeader.Replayer.Emulation;
using Zenject;

namespace BeatLeader.Replayer.Tweaking
{
    internal class InteropsLoaderTweak : GameTweak
    {
        [Inject] private readonly VRControllersProvider _controllersProvider;

        public override void Initialize()
        {
            RaycastBlocker.EnableBlocker = true;
            Cam2Interop.SetHeadTransform(_controllersProvider.Head.transform);
        }
        public override void Dispose()
        {
            RaycastBlocker.EnableBlocker = false;
            Cam2Interop.SetHeadTransform(null);
        }
    }
}
