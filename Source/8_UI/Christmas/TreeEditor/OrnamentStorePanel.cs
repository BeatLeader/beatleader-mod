using System;
using System.Linq;
using BeatLeader.API.Methods;
using BeatLeader.Models;
using ModestTree;
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
        private TreeStatus? _treeStatus;
        private StaticScreen _screen = null!;
        private bool _initialized;

        public void Setup(ChristmasOrnamentPool pool) {
            foreach (var cell in _cells) {
                cell.Setup(pool);
            }
        }

        public void Present() {
            _screen.Present();
        }

        public void Dismiss() {
            _screen.Dismiss();
        }

        public void Reload() {
            if (_initialized || _treeStatus == null) {
                return;
            }
            var ornaments = _treeStatus.previousDays;
            var size = ornaments.Length;
            for (var i = 0; i < VerticalCells * HorizontalCells; i++) {
                var ornament = i == size ? _treeStatus.today : ornaments[i];
                var cell = _cells[i];

                if (i <= size - 1) {
                    cell.SetBundleId(ornament.bundleId);
                } else {
                    cell.SetOpeningDay(i + 1);
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

            TreeMapRequest.AddStateListener(HandleTreeStatusRequestState);
            TreeMapRequest.SendRequest();
        }

        private void OnDestroy() {
            TreeMapRequest.RemoveStateListener(HandleTreeStatusRequestState);
        }

        private static int CalcSize(int count) {
            return CellSize * count + GapSize * (count - 1);
        }

        private void HandleTreeStatusRequestState(API.RequestState state, TreeStatus status, string? failReason) {
            if (state != API.RequestState.Finished) {
                return;
            }
            _treeStatus = status;
            Reload();
        }
    }
}