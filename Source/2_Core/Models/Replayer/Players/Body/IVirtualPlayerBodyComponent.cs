namespace BeatLeader.Models {
    public interface IVirtualPlayerBodyComponent {
        bool UsesPrimaryModel { get; }
        
        void ApplyConfig(IVirtualPlayerBodyConfig config);
    }
}