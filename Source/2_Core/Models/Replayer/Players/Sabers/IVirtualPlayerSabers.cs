namespace BeatLeader.Models {
    public interface IVirtualPlayerSabers {
        IHandVRControllersProvider ControllersProvider { get; }
        bool HasAlphaSupport { get; }

        void ApplyConfig(VirtualPlayerSabersConfig config);
    }
}