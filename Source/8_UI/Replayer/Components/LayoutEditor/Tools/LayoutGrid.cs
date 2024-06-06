using BeatLeader.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    [RequireComponent(typeof(Image))]
    internal class LayoutGrid : MonoBehaviour, ILayoutComponentTransformsHandler {
        #region Setup

        private Vector2 Size => _rect.rect.size;

        public int lineThickness = 1;
        public int cellSize = 5;

        public Color color = Color.white;
        public Color backgroundColor = new(0.2f, 0.2f, 0.2f);

        private Material _gridMaterial = null!;
        private RectTransform _rect = null!;
        private Image _image = null!;

        private void Awake() {
            _image = GetComponent<Image>();
            _rect = GetComponent<RectTransform>();
            _gridMaterial = Instantiate(BundleLoader.UIGridMaterial);
            _image.material = _gridMaterial;
            Refresh();
        }

        private void OnDestroy() {
            Destroy(_gridMaterial);
        }

        private void OnEnable() {
            _image.enabled = true;
            Refresh();
        }

        private void OnDisable() {
            _image.enabled = false;
        }

        #endregion

        #region Refresh

        private static readonly int colorShaderProp = Shader.PropertyToID("_Color");
        private static readonly int bgColorShaderProp = Shader.PropertyToID("_BgColor");
        private static readonly int thicknessShaderProp = Shader.PropertyToID("_LineThickness");
        private static readonly int sizeShaderProp = Shader.PropertyToID("_CellSize");
        private static readonly int widthShaderProp = Shader.PropertyToID("_Width");
        private static readonly int heightShaderProp = Shader.PropertyToID("_Height");

        public void Refresh() {
            _gridMaterial.SetColor(colorShaderProp, color);
            _gridMaterial.SetColor(bgColorShaderProp, backgroundColor);
            _gridMaterial.SetFloat(thicknessShaderProp, lineThickness);
            _gridMaterial.SetFloat(sizeShaderProp, cellSize);
            _gridMaterial.SetFloat(widthShaderProp, Size.x);
            _gridMaterial.SetFloat(heightShaderProp, Size.y);
        }

        #endregion

        #region TransformsHandler

        private static float AlignByGrid(float value, float cellPseudoSize, int elementCells, bool odd) {
            value = MathUtils.RoundStepped(value, cellPseudoSize);
            if (elementCells % 2 == (odd ? 0 : 1)) {
                value += cellPseudoSize / 2f;
            }
            return value;
        }

        public Vector2 OnMove(ILayoutComponent component, Vector2 origin, Vector2 destination) {
            var cellPseudoSize = cellSize + lineThickness;
            var elementCells = component.ComponentController.ComponentSize / cellPseudoSize;
            destination.x = AlignByGrid(destination.x, cellPseudoSize, (int)elementCells.x, true);
            destination.y = AlignByGrid(destination.y, cellPseudoSize, (int)elementCells.y, false);
            return destination;
        }

        public Vector2 OnResize(ILayoutComponent component, Vector2 origin, Vector2 destination) {
            var cellPseudoSize = cellSize + lineThickness;
            for (var i = 0; i < 2; i++) destination[i] -= destination[i] % cellPseudoSize;
            return destination;
        }

        #endregion
    }
}