namespace BeatLeader.Models {
    public interface IVirtualPlayerBody {
        IVRControllersProvider ControllersProvider { get; }
        
        void ApplyConfig(VirtualPlayerBodyConfig config);
    }
}