using UnityEngine;

#nullable disable

namespace BeatLeader {
    [CreateAssetMenu(fileName = "SpriteCollection", menuName = "SpriteCollection")]
    public class SpriteCollection : ScriptableObject {
        [Space] [Header("Backgrounds")]
        public Sprite rectangle;
        public Sprite background;
        public Sprite backgroundLeft;
        public Sprite backgroundRight;
        public Sprite backgroundBottom;
        public Sprite backgroundTop;
        public Sprite backgroundRightBottom;
        public Sprite backgroundUnderline;
        /* Icons */
        [Space] [Header("Icons")]
        public Sprite homeIcon;
        public Sprite spinnerIcon;
        public Sprite trashIcon;
        public Sprite editIcon;
        public Sprite curvatureIcon;
        public Sprite snapIcon;
        public Sprite pinIcon;
        public Sprite crossIcon;
        public Sprite rightArrowIcon;
        public Sprite plusIcon;
        public Sprite minusIcon;
        public Sprite ascendingIcon;
        public Sprite descendingIcon;
        public Sprite triangleIcon;
        public Sprite checkIcon;
        /* Other */
        [Space] [Header("Misc")]
        public Sprite glare;
        public Sprite transparentPixel;
    }
}