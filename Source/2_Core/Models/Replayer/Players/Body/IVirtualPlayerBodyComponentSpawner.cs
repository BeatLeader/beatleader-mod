namespace BeatLeader.Models {
    public interface IVirtualPlayerBodyComponentSpawner {
        IVirtualPlayerBodyModel PrimaryModel { get; }
        IVirtualPlayerBodyModel Model { get; }
        
        void ApplyModelConfig(
            bool applyToPrimaryModel,
            VirtualPlayerBodyConfig config
        );
    }
}