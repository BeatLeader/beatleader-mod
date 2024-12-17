using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ChristmasTreeSettingsPanel : ReeUIComponentV2 {
        #region Components

        [UIValue("editor-button"), UsedImplicitly]
        private HeaderButton _editorButton = null!;

        [UIValue("mover-button"), UsedImplicitly]
        private HeaderButton _movementButton = null!;

        [UIValue("mover"), UsedImplicitly]
        private ChristmasTreeMoverControlPanel _moverControlPanel = null!;
        
        #endregion

        #region Toggles

        [UIValue("enable-tree"), UsedImplicitly]
        private bool EnableTree { get; set; }
        
        [UIValue("enable-snow"), UsedImplicitly]
        private bool EnableSnow { get; set; }

        #endregion
        
        #region Setup

        private ChristmasTree _tree = null!;
        public ChristmasTreeEditor _treeEditor = null!;
        private StaticScreen _screen = null!;

        public void Setup(ChristmasTree tree) {
            _tree = tree;
            _treeEditor.Setup(tree);
            _moverControlPanel.Setup(tree);
        }

        public void Present() {
            if (_treeEditor.IsOpened) {
                return;
            }
            _screen.Present();
        }

        public void Dismiss() {
            _screen.Dismiss();
        }

        protected override void OnInitialize() {
            _treeEditor = new GameObject("ChristmasTreeEditor").AddComponent<ChristmasTreeEditor>();
            _treeEditor.EditorClosedEvent += HandleEditorClosed;
            //
            _screen = Content.gameObject.AddComponent<StaticScreen>();
            Content.position = new Vector3(0f, 0.9f, 0.7f);
            Content.eulerAngles = new Vector3(45f, 0f, 0f);
        }

        protected override void OnInstantiate() {
            _moverControlPanel = Instantiate<ChristmasTreeMoverControlPanel>(transform);
            _editorButton = Instantiate<HeaderButton>(transform);
            _movementButton = Instantiate<HeaderButton>(transform);

            _editorButton.Setup(BundleLoader.TreeIcon);
            _movementButton.Setup(BundleLoader.LocationIcon);
            
            _editorButton.OnClick += HandleEditorButtonClicked;
            _movementButton.OnClick += HandleMoverButtonClicked;
        }

        #endregion

        #region Callbacks

        [UIAction("close-click"), UsedImplicitly]
        private void HandleCloseButtonClicked() {
            Dismiss();
        }
        
        public void HandleEditorButtonClicked() {
            _treeEditor.Present();
            Dismiss();
        }

        private void HandleMoverButtonClicked() {
            _moverControlPanel.Present();
        }

        private void HandleEditorClosed() {
            Present();
        }

        #endregion
    }
}