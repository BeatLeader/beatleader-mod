using BeatSaberMarkupLanguage.Attributes;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class EditableController : ReeUIComponentV2 {
        #region UI Components

        [UIComponent("up-button")]
        private readonly Button _upButton = null!;

        [UIComponent("down-button")]
        private readonly Button _downButton = null!;

        [UIValue("anchor-selector")]
        private AnchorSelector _anchorSelector = null!;

        #endregion

        #region Layers

        private bool CanTurnUp => ElementLayer < TotalLayersCount - 1;
        private bool CanTurnDown => ElementLayer > 0;

        private int ElementLayer => _selectedElement?.Layer ?? -1;
        private int TotalLayersCount => _selectedElement?.Root.parent.childCount ?? 0;

        #endregion

        #region Events

        public event Action? TableReloadRequestedEvent;

        #endregion

        #region Setup

        private EditableElement? _selectedElement;

        public void SetEditable(EditableElement element) {
            _selectedElement = element;
            RefreshButtons();
            if (_selectedElement != null) {
                _anchorSelector.Select(element.TempLayoutMap.anchor);
            } else {
                _anchorSelector.SetEnabled(false);
                _anchorSelector.Degenerate();
            }
        }

        public void RefreshButtons() {
            _upButton.interactable = CanTurnUp;
            _downButton.interactable = CanTurnDown;
        }

        protected override void OnInstantiate() {
            _anchorSelector = Instantiate<AnchorSelector>(transform);
            _anchorSelector.AnchorSelectedEvent += HandleAnchorSelected;
        }

        protected override void OnInitialize() {
            RefreshButtons();
        }

        #endregion

        #region Editable Controls

        private void SetAnchor(Vector2 anchor) {
            if (_selectedElement == null) return;
            _selectedElement.TempLayoutMap.anchor = anchor;
        }

        private void TurnLayerDown() {
            if (!CanTurnDown) return;
            _selectedElement!.Layer--;
            TableReloadRequestedEvent?.Invoke();
        }

        private void TurnLayerUp() {
            if (!CanTurnUp) return;
            _selectedElement!.Layer++;
            TableReloadRequestedEvent?.Invoke();
        }

        #endregion

        #region UI Callbacks

        [UIAction("up-button-clicked")]
        private void HandleUpButtonClicked() {
            TurnLayerUp();
            RefreshButtons();
        }

        [UIAction("down-button-clicked")]
        private void HandleDownButtonClicked() {
            TurnLayerDown();
            RefreshButtons();
        }

        private void HandleAnchorSelected(Vector2 anchor) {
            SetAnchor(anchor);
        }

        #endregion
    }
}
