using UnityEngine;

namespace BeatLeader.Models {
    public class ReplayTag {
        internal ReplayTag(string name, Color color) {
            Name = name;
            Color = color;
        }

        public string Name { get; internal set; }
        public Color Color { get; internal set; }
    }
}