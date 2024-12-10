using BeatLeader.Models;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    [RequireComponent(typeof(RectTransform))]
    internal class OrnamentStorePanel : MonoBehaviour {
        private const int VerticalCells = 3;
        private const int HorizontalCells = 4;
        private const int CellSize = 10;
        private const int GapSize = 1;

        private readonly OrnamentStoreCell[] _cells = new OrnamentStoreCell[VerticalCells * HorizontalCells];
        private StaticScreen _screen = null!;
        private ChristmasTree _tree = null!;
        private bool _initialized;

        public void Setup(ChristmasTree tree) {
            _tree = tree;
        }
        
        public void Present() {
            _screen.Present();
        }

        public void Dismiss() {
            _screen.Dismiss();
        }
        
        public void Reload(ChristmasTreeOrnamentSettings[] ornaments) {
            if (_initialized) {
                return;
            }
            var size = ornaments.Length;
            for (var i = 0; i < size; i++) {
                
                var ornament = ornaments[i];
                var cell = _cells[i];
                
                if (i <= size - 1) {
                    cell.Setup(_tree, ornament.bundleId);
                    cell.gameObject.SetActive(true);
                } else {
                    cell.gameObject.SetActive(false);
                }
            }
        }

        private void Awake() {
            _screen = gameObject.AddComponent<StaticScreen>();
            
            var group = gameObject.AddComponent<GridLayoutGroup>();
            group.cellSize = Vector2.one * CellSize;
            group.spacing = Vector2.one * GapSize;
            group.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            group.constraintCount = HorizontalCells;
            group.startCorner = GridLayoutGroup.Corner.UpperLeft;
            
            var rect = GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(CalcSize(HorizontalCells), CalcSize(VerticalCells));

            for (var i = 0; i < VerticalCells * HorizontalCells; i++) {
                var cell = new GameObject("OrnamentStoreCell").AddComponent<OrnamentStoreCell>();
                cell.transform.SetParent(transform, false);
                _cells[i] = cell;
            } 
        }

        private static int CalcSize(int count) {
            return CellSize * count + GapSize * (count - 1);
        }
    }
}