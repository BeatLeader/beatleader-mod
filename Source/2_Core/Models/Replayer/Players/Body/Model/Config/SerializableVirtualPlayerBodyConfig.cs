using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BeatLeader.Utils;
using IPA.Utilities;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BeatLeader.Models {
    internal class SerializableVirtualPlayerBodyConfig : IVirtualPlayerBodyConfig {
        #region Construction

        [JsonConstructor, UsedImplicitly]
        private SerializableVirtualPlayerBodyConfig() { }

        [OnDeserialized]
        private void OnDeserialize(StreamingContext context) {
            LoadSerializedParts();
            BindConfigs();
        }

        public SerializableVirtualPlayerBodyConfig(
            IVirtualPlayerBodyModel model,
            IVirtualPlayerBodyConfig config
        ) : this(model) {
            AnchorMask = config.AnchorMask;
            foreach (var (id, (_, conf)) in _sourceBodyParts) {
                var part = config.BodyParts[id];
                conf.Alpha = part.Alpha;
                conf.PotentiallyActive = part.PotentiallyActive;
            }
        }
        
        public SerializableVirtualPlayerBodyConfig(IVirtualPlayerBodyModel model) {
            _sourceBodyParts = model.Parts.ToDictionary(
                static x => x.Id,
                x => (x.AnchorNode, new SerializableVirtualPlayerBodyPartConfig())
            );
            LoadSerializedParts();
            BindConfigs();
        }

        #endregion

        #region Impl

        [JsonIgnore]
        public IReadOnlyDictionary<string, IVirtualPlayerBodyPartConfig> BodyParts { get; private set; } = null!;

        [JsonIgnore]
        public BodyNode AnchorMask {
            get => _anchorMask;
            set {
                _anchorMask = value;
                RefreshMask();
            }
        }

        public event Action<IVirtualPlayerBodyPartConfig?>? ConfigUpdatedEvent;
        
        private BodyNode _anchorMask = (BodyNode)int.MaxValue;

        #endregion

        #region Logic

        [JsonProperty("BodyParts"), UsedImplicitly]
        private Dictionary<string, (BodyNode, SerializableVirtualPlayerBodyPartConfig)> _sourceBodyParts = new();

        public void NotifyConfigUpdated(IVirtualPlayerBodyPartConfig? config) {
            ConfigUpdatedEvent?.Invoke(config);
        }

        private void LoadSerializedParts() {
            BodyParts = _sourceBodyParts.ToDictionary(
                static x => x.Key,
                static x => (IVirtualPlayerBodyPartConfig)x.Value.Item2
            );
        }

        private void RefreshMask() {
            foreach (var (_, (node, conf)) in _sourceBodyParts) {
                var state = (_anchorMask & node) == 0;
                conf.SetMaskEnabled(state ? true : null);
            }
        }

        private void BindConfigs() {
            _sourceBodyParts.ForEach((_, y) => y.Item2.SetBodyConfig(this));
        }

        #endregion
    }
}