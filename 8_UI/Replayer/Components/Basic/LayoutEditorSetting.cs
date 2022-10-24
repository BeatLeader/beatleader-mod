using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using System;

namespace BeatLeader.Components {
    internal class LayoutEditorSetting : ReeUIComponentV2 {
        #region Events

        public event Action EnteredEditModeEvent;

        #endregion

        #region Setup

        private LayoutEditor _layoutEditor;

        public void Setup(LayoutEditor editor) {
            _layoutEditor = editor;
        }

        protected override void OnInitialize() {
            Content.gameObject.AddComponent<InputDependentObject>().Init(InputUtils.InputType.FPFC);
        }

        #endregion

        #region UI Callbacks

        [UIAction("button-clicked")]
        private void OpenEditor() {
            if (_layoutEditor == null) return;

            _layoutEditor.SetEditModeEnabled(true);
            EnteredEditModeEvent?.Invoke();
        }

        #endregion
    }
}
