using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Utils;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class TextTabSelector : LayoutComponentBase<TextTabSelector> {
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
                if (!IsInitialized) return;
                Refresh();
            }
        }
        
        [ExternalProperty, UsedImplicitly]
        public bool HideBg {
            get => _hideBg;
            set {
                _hideBg = value;
                if (!IsInitialized) return;
                Refresh();
            }
        }

        [ExternalProperty, UsedImplicitly]
        public float BgSkew {
            set => _textSegmentedControl.GetComponentsInChildren<ImageView>().ForEach(x => x.SetField("_skew", value));
        }

        private IReadOnlyList<string> _tabNames = Array.Empty<string>();
        private bool _hideBg;

        #endregion

        #region Setup

        protected override LayoutGroupType LayoutGroupDirection => LayoutGroupType.Horizontal;
        
        [ExternalComponent]
        private TextSegmentedControl _textSegmentedControl = null!;

        public bool SelectTab(string tabName) {
            ValidateAndThrow();
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
            Refresh();
        }

        private void Refresh() {
            _textSegmentedControl.SetField("_hideCellBackground", _hideBg);
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