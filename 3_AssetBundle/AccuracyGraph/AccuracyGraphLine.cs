using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader {
    public class AccuracyGraphLine : Graphic {
        #region Serialized

        [SerializeField] private int resolution = 500;
        [SerializeField] private float thickness = 0.2f;

        #endregion

        #region Start

        private GraphMeshHelper _graphMeshHelper;

        protected override void Start() {
            base.Start();
            _graphMeshHelper = new GraphMeshHelper(resolution, 1, thickness);
        }

        #endregion

        #region OnPopulateMesh

        protected override void OnPopulateMesh(VertexHelper vh) {
            var screenRect = RectTransformUtility.PixelAdjustRect(rectTransform, canvas);
            var screenViewTransform = new ScreenViewTransform(screenRect, _viewRect);

            _graphMeshHelper.SetPoints(_points);
            _graphMeshHelper.PopulateMesh(vh, screenViewTransform, _canvasRadius);
        }

        #endregion

        #region Setup

        private List<Vector2> _points;
        private float _canvasRadius;
        private Rect _viewRect = Rect.MinMaxRect(0, 0, 1, 1);

        public void Setup(List<Vector2> points, Rect viewRect, float canvasRadius) {
            _points = points;
            _viewRect = viewRect;
            _canvasRadius = canvasRadius;

            SetVerticesDirty();
        }

        #endregion
    }
}