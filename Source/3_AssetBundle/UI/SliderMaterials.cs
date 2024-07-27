using UnityEngine;

#nullable disable

namespace BeatLeader {
    [CreateAssetMenu(fileName = "SliderMaterials", menuName = "SliderMaterials collection")]
    public class SliderMaterials : ScriptableObject {
        public Material normalized;
        public Material number;
        public Material value;
        public Material alpha;
    }
}