using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Utils;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class TabSegmentedControl : ComponentLayoutBase<TabSegmentedControl> {
        #region Prefab

        [FirstResource]
        private static TextSegmentedControl _textSegmentedControlPrefab = null!;

        #endregion

        #region Events

        [ExternalProperty, UsedImplicitly]
        public event Action<string>? TabSelectedEvent;

        #endregion

        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public IReadOnlyList<string> TabNames {
            get => _tabNames;
            set {
                _tabNames = value;
                RefreshTexts();
            }
        }

        #endregion

        #region Setup

        protected override LayoutGroupType LayoutGroup => LayoutGroupType.Horizontal;
        
        [ExternalComponent]
        private TextSegmentedControl _textSegmentedControl = null!;
        
        private IReadOnlyList<string> _tabNames = Array.Empty<string>();
        
        public bool SelectTab(string tabName) {
            var cell =_textSegmentedControl.cells
                .Select(static x => x as TextSegmentedControlCell)
                .FirstOrDefault(x => x!.text == tabName);
            if (cell is null) return false;
            cell.SetSelected(true, SelectableCell.TransitionType.Instant, null, false);
            return true;
        }
        
        protected override void OnInitialize() {
            if (!_textSegmentedControlPrefab) this.LoadResources();
            _textSegmentedControl = _textSegmentedControlPrefab.CopyComponent<TextSegmentedControl>(Content);
            _textSegmentedControl.didSelectCellEvent += HandleCellSelected;
            RefreshTexts();
        }

        private void RefreshTexts() {
            _textSegmentedControl.SetTexts(TabNames);
        }

        #endregion

        #region Callbacks

        private void HandleCellSelected(SegmentedControl control, int index) {
            TabSelectedEvent?.Invoke(TabNames[index]);
        }

        #endregion
    }
}