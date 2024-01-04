using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System;
using JetBrains.Annotations;
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
        private ILayoutEditor? _layoutEditor;

        public void Setup(ILayoutEditor? editor, IReplayPauseController pauseController) {
            base.OnDispose();
            _layoutEditor = editor;
            _pauseController = pauseController;
        }

        protected override void OnInitialize() {
            Content.gameObject.AddComponent<InputDependentObject>().Init(InputUtils.InputType.FPFC);
            HandlePartialDisplayStateChanged(false);
        }
        
        #endregion

        #region Callbacks

        [UIAction("button-clicked"), UsedImplicitly]
        private void HandleEditorButtonClicked() {
            _layoutEditor!.SetEditorActive(true);
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
