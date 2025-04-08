using Newtonsoft.Json;
using UnityEngine;

namespace BeatLeader.Models {
    public class ReplayTag {
        [JsonConstructor]
        private ReplayTag() { }
        
        internal ReplayTag(string name, Color color) {
            Name = name;
            Color = color;
        }

        public string Name { get; internal set; } = null!;
        public Color Color { get; internal set; }
    }
}