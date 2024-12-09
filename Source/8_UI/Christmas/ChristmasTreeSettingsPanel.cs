using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ChristmasTreeSettingsPanel : ReeUIComponentV2 {
        #region Components

        [UIValue("editor-button"), UsedImplicitly]
        private HeaderButton _editorButton = null!;

        [UIValue("movement-button"), UsedImplicitly]
        private HeaderButton _movementButton = null!;

        #endregion

        #region Setup

        private ChristmasTree _tree = null!;
        private ChristmasTreeEditor _treeEditor = null!;
        private StaticScreen _screen = null!;

        public void Setup(ChristmasTree tree) {
            _tree = tree;
            _treeEditor.Setup(tree);
        }

        public void Present() {
            _screen.Present();
        }

        public void Dismiss() {
            _screen.Dismiss();
        }

        public void LoadSettings(ChristmasTreeSettings settings) {
            _treeEditor.LoadSettings(settings);
        }

        protected override void OnInitialize() {
            _treeEditor = new GameObject("ChristmasTreeEditor").AddComponent<ChristmasTreeEditor>();
            _treeEditor.EditorClosedEvent += HandleEditorClosed;
            //
            _screen = Content.gameObject.AddComponent<StaticScreen>();
            Content.position = new Vector3(0f, 1f, 0.5f);
            Content.eulerAngles = new Vector3(45f, 0f, 0f);
        }

        protected override void OnInstantiate() {
            _editorButton = Instantiate<HeaderButton>(transform);
            _movementButton = Instantiate<HeaderButton>(transform);

            _editorButton.Setup(BundleLoader.TreeIcon);
            _movementButton.Setup(BundleLoader.LocationIcon);
            
            _editorButton.OnClick += HandleEditorButtonClicked;
            _movementButton.OnClick += HandleMovementButtonClicked;
        }

        #endregion

        #region Callbacks

        private void HandleEditorButtonClicked() {
            _treeEditor.Present();
            Dismiss();
        }

        private void HandleMovementButtonClicked() { }

        private void HandleEditorClosed() {
            Present();
        }

        #endregion
    }
}