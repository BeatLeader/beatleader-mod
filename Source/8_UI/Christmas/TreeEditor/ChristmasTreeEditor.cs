using System;
using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ChristmasTreeEditor : MonoBehaviour {
        public event Action? EditorClosedEvent;

        private OrnamentStorePanel _ornamentStore = null!;
        private ChristmasTreeEditorControlPanel _editorPanel = null!;
        
        private ChristmasTree _tree = null!;
        private Vector3 _editorPos = new(0f, 0f, 1f);
        private Vector3 _initialPos;

        public void Setup(ChristmasTree tree) {
            _tree = tree;
        }

        public void LoadSettings(ChristmasTreeSettings settings) {
            _ornamentStore.Reload(settings.ornaments);
        }

        public void Present() {
            _initialPos = _tree.transform.position;
            _tree.ScaleTo(0.5f);
            _tree.MoveTo(_editorPos);
            _ornamentStore.Present();
            _editorPanel.Present();
        }

        private void Dismiss() {
            _tree.MoveTo(_initialPos);
            _tree.ScaleTo(1f);
            _ornamentStore.Dismiss();
            _editorPanel.Dismiss();
            EditorClosedEvent?.Invoke();
        }

        private void Awake() {
            _ornamentStore = new GameObject("OrnamentStorePanel").AddComponent<OrnamentStorePanel>();
            var trans = _ornamentStore.transform;
            trans.SetParent(transform, false);
            trans.localPosition = new Vector3(-0.8f, 1f, 0.3f);
            trans.localEulerAngles = new Vector3(0f, 300f, 0f);

            _editorPanel = ReeUIComponentV2.Instantiate<ChristmasTreeEditorControlPanel>(transform);
            _editorPanel.ManualInit(transform);
            _editorPanel.CancelButtonClickedEvent += HandleCancelButtonClicked;
            _editorPanel.OkButtonClickedEvent += HandleOkButtonClicked;
            
            trans = _editorPanel.GetRootTransform();
            trans.localPosition = new Vector3(0.8f, 1f, 0.3f);
            trans.localEulerAngles = new Vector3(0f, 60f, 0f);
        }

        private void HandleCancelButtonClicked() {
            Dismiss();
        }

        private void HandleOkButtonClicked() {
            //TODO: implement saving logic 
            Dismiss();
        }
    }
}