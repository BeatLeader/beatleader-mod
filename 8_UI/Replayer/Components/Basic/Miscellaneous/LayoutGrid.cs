using BeatLeader.Models;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    [RequireComponent(typeof(Image))]
    internal class LayoutGrid : MonoBehaviour, ILayoutGrid {
        public float LineThickness { get; set; } = 6;
        public float CellSize { get; set; } = 70;
        public Vector2 Size => _rect.sizeDelta;

        public Color color = Color.white;
        public Color bgColor = new(0.2f, 0.2f, 0.2f);

        private Material _gridMaterial = null!;
        private RectTransform _rect = null!;
        private Image _image = null!;

        private void Awake() {
            _image = GetComponent<Image>();
            _rect = GetComponent<RectTransform>();
            _image.material = _gridMaterial = Instantiate(BundleLoader.UIGridMaterial);
            Refresh();
        }
        private void OnDestroy() {
            Destroy(_gridMaterial);
        }

        private void OnEnable() {
            _image.enabled = true;
        }
        private void OnDisable() {
            _image.enabled = false;
        }

        public void Refresh() {
            _gridMaterial.SetColor("_Color", color);
            _gridMaterial.SetColor("_BgColor", bgColor);
            _gridMaterial.SetFloat("_LineThickness", LineThickness);
            _gridMaterial.SetFloat("_CellSize", CellSize);
            _gridMaterial.SetFloat("_Width", Size.x);
            _gridMaterial.SetFloat("_Height", Size.y);
        }
    }
}
