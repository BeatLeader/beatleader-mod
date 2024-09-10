using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class EditorTableView : ReeUIComponentV2, TableView.IDataSource {
        #region EditablesComparer

        private class CellsComparer : IComparer<EditorTableCell> {
            public bool invert;

            public int Compare(EditorTableCell left, EditorTableCell right) {
                var calc = left.Layer > right.Layer ? 1 : left.Layer < right.Layer ? -1 : 0;
                return invert ? -calc : calc;
            }
        }

        #endregion

        #region Events

        public event Action<EditorTableCell> CellSelectedEvent;

        #endregion

        #region Setup

        [UIComponent("elements-list")]
        private readonly CustomCellListTableData _customList;

        private TableView _tableView;

        private readonly CellsComparer _comparer = new() { invert = true };
        private readonly List<EditorTableCell> _models = new();

        public void ReloadTable() {
            _models.ForEach(x => x.RefreshTexts());
            _models.Sort(_comparer);
            _tableView.ReloadData();
            RefreshLayers();
        }

        public void SelectCell(EditableElement element) {
            if (element == null) {
                _tableView.ClearSelection();
                return;
            }
            var idx = _models.FindIndex(x => x.Element == element);
            if (idx == -1) return;
            _tableView.SelectCellWithIdx(idx);
        }

        public void Add(EditableElement element) {
            var cell = InstantiateOnSceneRoot<EditorTableCell>();
            cell.Setup(element);
            cell.ManualInit(null!);
            _models.Add(cell);
        }

        public bool Remove(EditableElement element) {
            var model = _models.FirstOrDefault(x => x.Element == element);
            if (model == null) return false;
            return _models.Remove(model);
        }

        private void RefreshLayers() {
            foreach (var item in _models) {
                var editable = item.Element;
                editable.TempLayoutMap.layer = editable.Layer;
            }
        }

        protected override void OnInitialize() {
            _tableView = _customList.TableView;
            _tableView.SetDataSource(this, false);
            _tableView.didSelectCellWithIdxEvent += HandleCellSelected;
            var list = Content.Find("BSMLCustomList").gameObject;
            list.GetComponent<BaseRaycaster>().TryDestroy();
            list.AddComponent<GraphicRaycaster>();
        }

        #endregion

        #region Callbacks

        private void HandleCellSelected(TableView table, int idx) {
            CellSelectedEvent?.Invoke(_models[idx]);
        }

        #endregion

        #region TableSource

        private const float kCellSize = 9.5f;

        public float CellSize(int idx) {
            return kCellSize;
        }

        public int NumberOfCells() {
            return _models.Count;
        }

        public TableCell CellForIdx(TableView tableView, int idx) {
            return _models[idx].Cell;
        }

        #endregion
    }
}
