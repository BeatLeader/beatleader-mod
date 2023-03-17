using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeatLeader {
    [RequireComponent(typeof(RectTransform))]
    public class AccuracyGraph: UIBehaviour {
        #region Serialized

        [SerializeField] private AccuracyGraphLine graphLine;
        [SerializeField] private Material backgroundMaterial;

        public Canvas Canvas => graphLine.canvas;

        #endregion

        #region Construct

        private Material _backgroundMaterialInstance;

        public void Construct(Image backgroundImage) {
            _backgroundMaterialInstance = Instantiate(backgroundMaterial);
            backgroundImage.material = _backgroundMaterialInstance;
        }

        #endregion

        #region Setup

        private Rect _viewRect = Rect.MinMaxRect(0, 0, 1, 1);
        private float _songDuration = 1.0f;

        public void Setup(List<Vector2> positions, Rect viewRect, float canvasRadius, float songDuration) {
            _songDuration = songDuration;
            _viewRect = viewRect;
            
            graphLine.Setup(positions, _viewRect, canvasRadius);
            UpdateBackground();
        }

        #endregion

        #region Shader

        private static readonly int ViewRectPropertyId = Shader.PropertyToID("_ViewRect");
        private static readonly int SongDurationPropertyId = Shader.PropertyToID("_SongDuration");
        private static readonly int CursorPositionPropertyId = Shader.PropertyToID("_CursorPosition");

        private void UpdateBackground() {
            var viewRectVector = new Vector4(_viewRect.xMin, _viewRect.yMin, _viewRect.xMax, _viewRect.yMax);
            _backgroundMaterialInstance.SetVector(ViewRectPropertyId, viewRectVector);
            _backgroundMaterialInstance.SetFloat(SongDurationPropertyId, _songDuration);
        }

        public void SetCursor(float viewTime) {
            _backgroundMaterialInstance.SetFloat(CursorPositionPropertyId, viewTime);
        }

        #endregion
    }
}