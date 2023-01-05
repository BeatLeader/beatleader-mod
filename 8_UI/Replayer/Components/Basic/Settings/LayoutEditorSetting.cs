using BeatLeader.Models;
using BeatLeader.UI.BSML_Addons.Components;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class LayoutEditorSetting : ReeUIComponentV2 {
        #region Events

        public event Action? EnteredEditModeEvent;

        #endregion

        #region UI Components

        [UIComponent("text")]
        private readonly TextMeshProUGUI _text = null!;

        [UIObject("button")]
        private readonly GameObject _button = null!;

        #endregion

        #region Setup

        private IReplayPauseController _pauseController = null!;
        private LayoutEditor? _layoutEditor;

        public void Setup(LayoutEditor? editor, IReplayPauseController pauseController) {
            base.OnDispose();
            _layoutEditor = editor;
            _pauseController = pauseController;
            if (_layoutEditor != null) {
                _layoutEditor.PartialDisplayModeStateWasChangedEvent += HandlePartialDisplayStateChanged;
            }
        }

        protected override void OnInitialize() {
            Content.gameObject.AddComponent<InputDependentObject>().Init(InputUtils.InputType.FPFC);
            HandlePartialDisplayStateChanged(false);
        }

        protected override void OnDispose() {
            if (_layoutEditor != null) {
                _layoutEditor.PartialDisplayModeStateWasChangedEvent -= HandlePartialDisplayStateChanged;
            }
        }

        #endregion

        #region Callbacks

        [UIAction("button-clicked")]
        private void HandleEditorButtonClicked() {
            if (_layoutEditor == null || _layoutEditor.PartialDisplayEnabled) return;
            _layoutEditor.SetEnabled(true);
            _pauseController.Pause();
            EnteredEditModeEvent?.Invoke();
        }

        private void HandlePartialDisplayStateChanged(bool state) {
            _text.text = state ? "<color=\"red\">Partial Display Mode enabled</color>" : "Edit Layout";
            _button.gameObject.SetActive(!state);
        }

        #endregion
    }
}
