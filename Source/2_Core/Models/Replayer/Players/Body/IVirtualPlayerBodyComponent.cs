namespace BeatLeader.Models {
    public interface IVirtualPlayerBodyComponent : IControllableVirtualPlayerBody {
        bool UsesPrimaryModel { get; }
        
        void ApplyConfig(IVirtualPlayerBodyConfig config);
    }
}