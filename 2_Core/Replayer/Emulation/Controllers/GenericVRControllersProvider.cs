using BeatLeader.Models;

namespace BeatLeader.Replayer.Emulation {
    internal class GenericVRControllersProvider : IVRControllersProvider {
        public GenericVRControllersProvider(VRController left, VRController right, VRController head) {
            LeftSaber = left;
            RightSaber = right;
            Head = head;
        }

        public VRController LeftSaber { get; }
        public VRController RightSaber { get; }
        public VRController Head { get; }
    }
}
