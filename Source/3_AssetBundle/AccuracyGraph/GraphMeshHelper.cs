using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader {
    internal class GraphMeshHelper {
        #region Data

        private readonly int[] _triangles;
        private readonly Vector2[] _uv0;

        private readonly int _verticalResolution;
        private readonly int _rowsCount;
        private readonly int _columnsCount;
        private readonly int _vertexCount;
        private readonly float _lineThickness;

        private readonly GraphPoint[] _pointsArray;

        #endregion

        #region Constructor

        public GraphMeshHelper(int horizontalResolution, int verticalResolution, float lineThickness) {
            _verticalResolution = verticalResolution;
            _rowsCount = verticalResolution + 1;
            _columnsCount = horizontalResolution + 1;
            _lineThickness = lineThickness;

            _vertexCount = _rowsCount * _columnsCount;
            _triangles = CreateTrianglesArray(verticalResolution, horizontalResolution);
            _uv0 = CreateUvArray();
            _pointsArray = new GraphPoint[_columnsCount];
        }

        #endregion

        #region SetPoints

        private GraphSpline _spline;

        public void SetPoints(List<Vector2> points) {
            if (points.Count <= 1) {
                _spline = null;
                return;
            }

            _spline = new GraphSpline(points.Count);
            _spline.Add(points.First());
            _spline.Add(points.First());
            foreach (var t in points) {
                _spline.Add(t);
            }
            _spline.Add(points.Last());
        }

        #endregion

        #region PopulateMesh

        public void PopulateMesh(VertexHelper vh, ScreenViewTransform svt, float canvasRadius) {
            vh.Clear();
            
            if (_spline == null) return;
            _spline.FillArray(_pointsArray);

            for (var columnIndex = 0; columnIndex < _columnsCount; columnIndex++) {
                var node = _pointsArray[columnIndex];

                var screenNodePosition = svt.InverseTransformPoint(node.Position);
                var screenNodeTangent = svt.InverseTransformDirection(node.Tangent);
                var screenNodeNormal = new Vector2(screenNodeTangent.y, -screenNodeTangent.x);

                for (var rowIndex = 0; rowIndex < _rowsCount; rowIndex++) {
                    var verticalRatio = ((float) rowIndex / _verticalResolution - 0.5f) * 2.0f;
                    var vertexIndex = GetVertexIndex(rowIndex, columnIndex);

                    var widthOffset = verticalRatio * _lineThickness * screenNodeNormal;
                    var screenVertexPosition = screenNodePosition + widthOffset;
                    var screenNormalizedPosition = svt.NormalizeScreenPosition(screenVertexPosition);

                    vh.AddVert(
                        screenVertexPosition,
                        Color.white,
                        _uv0[vertexIndex],
                        screenNormalizedPosition,
                        new Vector2(canvasRadius, 0),
                        node.Position,
                        Vector3.one,
                        Vector3.one
                    );
                }
            }

            for (var i = 0; i < _triangles.Length; i += 3) vh.AddTriangle(_triangles[i], _triangles[i + 1], _triangles[i + 2]);
        }

        #endregion

        #region Utils

        private int GetVertexIndex(int columnIndex, int rowIndex) {
            return rowIndex * _rowsCount + columnIndex;
        }

        #endregion

        #region CreateArrays

        private Vector2[] CreateUvArray() {
            var tmp = new Vector2[_vertexCount];

            for (var rowIndex = 0; rowIndex < _columnsCount; rowIndex++) {
                var horizontalRatio = (float) rowIndex / (_columnsCount - 1);
                for (var columnIndex = 0; columnIndex < _rowsCount; columnIndex++) {
                    var verticalRatio = (float) columnIndex / (_rowsCount - 1);
                    var vertexIndex = GetVertexIndex(columnIndex, rowIndex);
                    tmp[vertexIndex] = new Vector2(verticalRatio, horizontalRatio);
                }
            }

            return tmp;
        }

        private int[] CreateTrianglesArray(int horizontalResolution, int verticalResolution) {
            var quadCount = horizontalResolution * verticalResolution;
            var tmp = new int[quadCount * 6];

            for (var i = 0; i < verticalResolution; i++)
            for (var j = 0; j < horizontalResolution; j++) {
                var quadIndex = i * horizontalResolution + j;

                var topLeftVertexIndex = GetVertexIndex(j, i);
                var topRightVertexIndex = GetVertexIndex(j + 1, i);
                var bottomLeftVertexIndex = GetVertexIndex(j, i + 1);
                var bottomRightVertexIndex = GetVertexIndex(j + 1, i + 1);

                var leftTriangleIndex = quadIndex * 6;
                tmp[leftTriangleIndex + 0] = topLeftVertexIndex;
                tmp[leftTriangleIndex + 1] = bottomLeftVertexIndex;
                tmp[leftTriangleIndex + 2] = bottomRightVertexIndex;

                var rightTriangleIndex = leftTriangleIndex + 3;
                tmp[rightTriangleIndex + 0] = bottomRightVertexIndex;
                tmp[rightTriangleIndex + 1] = topRightVertexIndex;
                tmp[rightTriangleIndex + 2] = topLeftVertexIndex;
            }

            return tmp;
        }

        #endregion
    }
}