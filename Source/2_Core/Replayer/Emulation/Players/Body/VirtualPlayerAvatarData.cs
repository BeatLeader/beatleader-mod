using BeatLeader.Models;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayerAvatarData {
        public VirtualPlayerAvatarData(IVirtualPlayerBase player) {
            Player = player;
        }

        public IVirtualPlayerBodyConfig DefaultConfig {
            get => _defaultConfig ?? throw new UninitializedComponentException();
            set => _defaultConfig = value;
        }

        public IVirtualPlayerBodyConfig PrimaryConfig {
            get => _primaryConfig ?? throw new UninitializedComponentException();
            set => _primaryConfig = value;
        }

        public readonly IVirtualPlayerBase Player;
        
        private IVirtualPlayerBodyConfig? _defaultConfig;
        private IVirtualPlayerBodyConfig? _primaryConfig;
    }
}