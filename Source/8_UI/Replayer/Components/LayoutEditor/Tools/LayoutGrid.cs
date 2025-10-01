using BeatLeader.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    [RequireComponent(typeof(Image))]
    internal class LayoutGrid : MonoBehaviour {
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
        }

        private void Start() {
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
            _cellPseudoSize = cellSize + lineThickness;
            _areaCells = Size / _cellPseudoSize;
        }

        #endregion

        #region TransformsHandler

        private Vector2 _areaCells;
        private float _cellPseudoSize;
        
        private float AlignByGrid(float value, float areaCells, float compCells) {
            value = MathUtils.RoundStepped(value, _cellPseudoSize);
            if (Odd(areaCells) ^ Odd(compCells)) {
                value += _cellPseudoSize / 2f;
            }
            return value;

            static bool Odd(float value) {
                return (int)value % 2 == 1;
            }
        }

        public Vector2 OnMove(ILayoutComponent component, Vector2 destination) {
            var compCells = (Vector2)component.LayoutData.size / _cellPseudoSize;
            //
            destination.x = AlignByGrid(destination.x, _areaCells.x, compCells.x);
            destination.y = AlignByGrid(destination.y, _areaCells.y, compCells.y);
            return destination;
        }

        public Vector2 OnResize(ILayoutComponent component, Vector2 destination) {
            var cellPseudoSize = cellSize + lineThickness;
            for (var i = 0; i < 2; i++) {
                destination[i] -= destination[i] % cellPseudoSize;
            }
            return destination;
        }

        #endregion
    }
}