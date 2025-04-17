using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class ContextsModal : AbstractReeModal<object> {
        #region Components

        [UIComponent("OptionsList"), UsedImplicitly] private CustomListTableData _optionsList;

        protected override void OnInitialize() {
            base.OnInitialize();
            PluginConfig.ScoresContextListChangedEvent += OnScoresContextListWasChanged;
        }

        protected override void OnDispose() {
            base.OnDispose();
            PluginConfig.ScoresContextListChangedEvent -= OnScoresContextListWasChanged;
        }

        private void Update() {
            UpdateListIfDirty();
            UpdateHighlightIfDirty();
        }

        #endregion

        #region Events

        private void OnScoresContextListWasChanged() {
            MarkListDirty();
            MarkHighlightDirty();
        }

        [UIAction("OnOptionSelected"), UsedImplicitly]
        private void OnOptionSelected(TableView tableView, int index) {
            PluginConfig.ScoresContext = _options[index].Id;
            MarkHighlightDirty();
            Close();
        }

        #endregion

        #region List

        private IReadOnlyList<ScoresContext> _options = Array.Empty<ScoresContext>();
        private bool _listDirty = true;

        private void MarkListDirty() {
            _listDirty = true;
        }

        private void UpdateListIfDirty() {
            if (!_listDirty) return;
            _listDirty = false;

            _options = ScoresContexts.AllContexts;

            var cellInfos = new List<CustomListTableData.CustomCellInfo>();

            foreach (var option in _options) {
                var cellInfo = new CustomListTableData.CustomCellInfo(option.Name, option.Description, option.Icon);
                cellInfos.Add(cellInfo);
            }

            _optionsList.Data = cellInfos;
            _optionsList.TableView.ReloadDataKeepingPosition();

            MarkHighlightDirty();
        }

        #endregion

        #region Highlight

        private bool _highlightDirty = true;

        private void MarkHighlightDirty() {
            _highlightDirty = true;
        }

        private void UpdateHighlightIfDirty() {
            if (!_highlightDirty) return;
            _highlightDirty = false;

            var selectedContextId = PluginConfig.ScoresContext;

            for (var i = 0; i < _options.Count; i++) {
                if (_options[i].Id != selectedContextId) continue;
                _optionsList.TableView.SelectCellWithIdx(i);
                return;
            }

            _optionsList.TableView.ClearSelection();
        }

        #endregion
    }
}