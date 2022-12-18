using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System;

namespace BeatLeader.Components {
    internal class LayoutEditorSetting : ReeUIComponentV2 {
        #region Events

        public event Action? EnteredEditModeEvent;

        #endregion

        #region Setup

        private IReplayPauseController _pauseController = null!;
        private LayoutEditor? _layoutEditor;

        public void Setup(LayoutEditor? editor, IReplayPauseController pauseController) {
            _layoutEditor = editor;
            _pauseController = pauseController;
        }

        protected override void OnInitialize() {
            Content.gameObject.AddComponent<InputDependentObject>().Init(InputUtils.InputType.FPFC);
        }

        #endregion

        #region UI Callbacks

        [UIAction("button-clicked")]
        private void OpenEditor() {
            if (_layoutEditor == null) return;
            _layoutEditor.SetEditorEnabled(true);
            _pauseController.Pause();
            EnteredEditModeEvent?.Invoke();
        }

        #endregion
    }
}
