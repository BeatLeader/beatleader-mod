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

        public Vector2 OnMove(ILayoutComponent component, Vector2 origin, Vector2 destination) {
            var elementSize = component.ComponentController.ComponentSize;
            var elementOffset = elementSize / 2;
            var cellPseudoSize = cellSize + lineThickness;
            var areaSize = component.ComponentHandler!.AreaTransform!.rect.size;
            var areaOffset = areaSize / cellPseudoSize;

            for (var i = 0; i < 2; i++) {
                //works only for the area with pivot at the center
                var areaOffsetAxis = (int)areaOffset[i] % 2 is 0 ? 0 : cellPseudoSize / 2;
                var pos = destination[i] - elementOffset[i];
                var offset = pos - pos % cellPseudoSize;
                destination[i] = offset + elementOffset[i] + areaOffsetAxis;
            }

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