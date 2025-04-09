using Newtonsoft.Json;
using UnityEngine;

namespace BeatLeader.Models {
    public class ReplayTag {
        [JsonConstructor]
        internal ReplayTag(string name, Color color) {
            Name = name;
            Color = color;
        }

        public string Name { get; internal set; }

        [JsonIgnore]
        public Color Color {
            get => _color;
            internal set => _color = value;
        }

        [JsonProperty("Color")]
        private SerializableColor _color;
    }
}