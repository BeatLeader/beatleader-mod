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
        #region Factory

        public static SerializableVirtualPlayerBodyConfig Clone(IVirtualPlayerBodyModel model, IVirtualPlayerBodyConfig bodyConfig) {
            var config = Create(model);
            config.AnchorMask = config.AnchorMask;
            foreach (var (id, conf) in config.BodyParts) {
                var part = config.BodyParts[id];
                conf.Alpha = part.Alpha;
                conf.PotentiallyActive = part.PotentiallyActive;
            }
            return config;
        }

        public static SerializableVirtualPlayerBodyConfig Create(IVirtualPlayerBodyModel model) {
            var config = new SerializableVirtualPlayerBodyConfig {
                _serializedBodyParts = model.Parts.ToDictionary(
                    static x => x.Id,
                    _ => new SerializableVirtualPlayerBodyPartConfig()
                )
            };
            config.LoadSerializedParts();
            config.BindConfigs();
            return config;
        }

        public static SerializableVirtualPlayerBodyConfig CreateManual(
            Dictionary<string, SerializableVirtualPlayerBodyPartConfig> sourceBodyParts
        ) {
            var config = new SerializableVirtualPlayerBodyConfig {
                _serializedBodyParts = sourceBodyParts
            };
            config.LoadSerializedParts();
            config.BindConfigs();
            return config;
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

        private readonly Dictionary<string, BodyNode> _bodyNodes = new();

        public void SetBodyModel(IVirtualPlayerBodyModel model) {
            foreach (var item in model.Parts) {
                _bodyNodes[item.Id] = item.AnchorNode;
            }
        }

        private void RefreshMask() {
            foreach (var (name, conf) in _serializedBodyParts) {
                var state = (_anchorMask & _bodyNodes[name]) == 0;
                conf.SetMaskEnabled(state ? true : null);
            }
        }

        public void NotifyConfigUpdated(IVirtualPlayerBodyPartConfig? config) {
            ConfigUpdatedEvent?.Invoke(config);
        }

        #endregion

        #region Serialization

        [JsonConstructor, UsedImplicitly]
        private SerializableVirtualPlayerBodyConfig() { }

        [OnDeserialized]
        private void OnDeserialize(StreamingContext context) {
            LoadSerializedParts();
            BindConfigs();
        }

        [JsonProperty("BodyParts"), UsedImplicitly]
        private Dictionary<string, SerializableVirtualPlayerBodyPartConfig> _serializedBodyParts = new();

        private void LoadSerializedParts() {
            BodyParts = _serializedBodyParts.ToDictionary(
                static x => x.Key,
                static x => (IVirtualPlayerBodyPartConfig)x.Value
            );
        }

        private void BindConfigs() {
            _serializedBodyParts.ForEach((_, y) => y.SetBodyConfig(this));
        }

        #endregion
    }
}