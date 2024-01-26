using System.Collections.Generic;

namespace BeatLeader.Models {
    public abstract class VirtualPlayerBodyComponentSpawnerBase : IVirtualPlayerBodyComponentSpawner {
        public abstract IVirtualPlayerBodyModel PrimaryModel { get; }
        public abstract IVirtualPlayerBodyModel Model { get; }

        protected abstract IEnumerable<IVirtualPlayerBodyComponent> SpawnedBodyComponents { get; }

        private VirtualPlayerBodyConfig? _primaryConfig;
        private VirtualPlayerBodyConfig? _config;

        public void ApplyModelConfig(bool applyToPrimaryModel, VirtualPlayerBodyConfig config) {
            if (applyToPrimaryModel) {
                _primaryConfig = config;
                ApplyPrimaryConfig(config);
            } else {
                _config = config;
                ApplyConfig(config);
            }
        }

        protected virtual void ApplyPrimaryConfig(VirtualPlayerBodyConfig config) {
            ApplyConfigToComponents(true, config);
        }

        protected virtual void ApplyConfig(VirtualPlayerBodyConfig config) {
            ApplyConfigToComponents(false, config);
        }

        private void ApplyConfigToComponents(bool applyToPrimaryModel, VirtualPlayerBodyConfig config) {
            foreach (var component in SpawnedBodyComponents) {
                if (applyToPrimaryModel && !component.UsesPrimaryModel) continue;
                component.ApplyConfig(config);
            }
        }

        protected void EnhanceComponent(
            IVirtualPlayerBodyComponent component,
            IVirtualPlayersManager manager,
            IVirtualPlayerBase player
        ) {
            var primary = manager.PrimaryPlayer == player;
            var config = primary ? _primaryConfig : _config;
            if (config is not null) component.ApplyConfig(config);
        }
    }
}