using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public abstract class VirtualPlayerBodyComponentSpawnerBase {
        #region Configs

        public abstract IVirtualPlayerBodyModel PrimaryModel { get; }
        public abstract IVirtualPlayerBodyModel Model { get; }

        private IVirtualPlayerBodyConfig? _primaryConfig;
        private IVirtualPlayerBodyConfig? _config;

        public void ApplyModelConfig(bool applyToPrimaryModel, IVirtualPlayerBodyConfig config) {
            if (applyToPrimaryModel) {
                _primaryConfig = config;
                ApplyPrimaryConfig(config);
            } else {
                _config = config;
                ApplyConfig(config);
            }
        }

        protected virtual void ApplyPrimaryConfig(IVirtualPlayerBodyConfig config) {
            ApplyConfigToComponents(true, config);
        }

        protected virtual void ApplyConfig(IVirtualPlayerBodyConfig config) {
            ApplyConfigToComponents(false, config);
        }

        private void ApplyConfigToComponents(bool applyToPrimaryModel, IVirtualPlayerBodyConfig config) {
            foreach (var component in SpawnedComponents) {
                if (applyToPrimaryModel != component.UsesPrimaryModel) continue;
                component.ApplyConfig(config);
            }
        }

        protected void ApplyConfigToComponent(
            IVirtualPlayerBodyComponent component,
            IVirtualPlayersManager manager,
            IVirtualPlayerBase player
        ) {
            var primary = manager.PrimaryPlayer == player;
            var config = primary ? _primaryConfig : _config;
            if (config != null) component.ApplyConfig(config);
        }

        #endregion

        #region Spawning

        protected IReadOnlyCollection<IVirtualPlayerBodyComponent> SpawnedComponents => _components;

        private readonly HashSet<IVirtualPlayerBodyComponent> _components = new();

        public IControllableVirtualPlayerBody Spawn(IVirtualPlayersManager playersManager, IVirtualPlayerBase player) {
            var body = SpawnInternal(playersManager, player);
            body.RefreshVisuals();
            ApplyConfigToComponent(body, playersManager, player);
            _components.Add(body);
            return body;
        }

        public void Despawn(IControllableVirtualPlayerBody body) {
            if (body is not IVirtualPlayerBodyComponent poolBody || !_components.Contains(body)) {
                throw new InvalidOperationException("Unable to despawn the body which does not belong to the pool");
            }
            DespawnInternal(poolBody);
            _components.Remove(poolBody);
        }

        protected abstract IVirtualPlayerBodyComponent SpawnInternal(IVirtualPlayersManager playersManager, IVirtualPlayerBase player);
        protected virtual void DespawnInternal(IVirtualPlayerBodyComponent body) { }

        #endregion
    }
}