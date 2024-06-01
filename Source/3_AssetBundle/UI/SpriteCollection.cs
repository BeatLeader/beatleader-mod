using UnityEngine;

#nullable disable

namespace BeatLeader {
    [CreateAssetMenu(fileName = "SpriteCollection", menuName = "SpriteCollection")]
    public class SpriteCollection : ScriptableObject {
        public Sprite rectangle;
        public Sprite background;
        public Sprite backgroundBottom;
        public Sprite backgroundTop;
        /* Icons */
        public Sprite plusIcon;
        public Sprite minusIcon;
        public Sprite ascendingIcon;
        public Sprite descendingIcon;
        /* Other */
        public Sprite glare;
    }
}