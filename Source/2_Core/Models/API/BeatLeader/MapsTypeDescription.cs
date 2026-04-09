using UnityEngine;

namespace BeatLeader.Models {
    public class MapsTypeDescription {
        public int Id { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public Sprite? Sprite { get; set; }
    }
}
