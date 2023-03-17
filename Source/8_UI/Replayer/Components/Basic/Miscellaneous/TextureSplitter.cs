using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class TextureSplitter : MonoBehaviour {
        public Image image;
        public Color color = Color.white;
        public Vector2 point1 = Vector2.zero;
        public Vector2 point2 = new(0.5f, 0.5f);

        private Material _material;

        private void Awake() {
            _material = Instantiate(BundleLoader.TextureSplitterMaterial);
        }
        private void OnDestroy() {
            Destroy(_material);
        }

        public void Refresh() {
            if (image == null) return;
            image.material = _material;
            _material.SetVector("_Zone", new(point1.x, point1.y, point2.x, point2.y));
            _material.SetColor("_Color", color);
        }
    }
}
