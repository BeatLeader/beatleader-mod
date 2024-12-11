using System;
using System.Linq;
using System.Threading.Tasks;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ChristmasTreeEditor : MonoBehaviour {
        #region Setup

        public bool IsOpened { get; private set; }

        public event Action? EditorClosedEvent;

        private OrnamentStorePanel _ornamentStore = null!;
        private ChristmasTreeEditorControlPanel _editorPanel = null!;
        private ChristmasTree _tree = null!;

        private Vector3 _editorPos = new(0f, 0f, 1f);
        private Vector3 _initialPos;
        private float _editorScale = 1f;
        private float _initialScale;

        public void Setup(ChristmasTree tree) {
            _tree = tree;
            _ornamentStore.Setup(tree.OrnamentsPool);
        }

        public void Present() {
            IsOpened = true;
            _initialPos = _tree.transform.position;
            _initialScale = _tree.transform.localScale.y;
            _tree.ScaleTo(_editorScale);
            _tree.MoveTo(_editorPos);
            _ornamentStore.Present();
            _editorPanel.Present();
        }

        private void Dismiss() {
            IsOpened = false;
            _tree.MoveTo(_initialPos);
            _tree.ScaleTo(_initialScale);
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

        #endregion

        #region Settings

        private bool _uploading;

        private async Task UploadSettings() {
            var settings = _tree.Ornaments.Select(static x => x.GetSettings());
            await WebUtils.SendAsync($"{BLConstants.BEATLEADER_API_URL}/projecttree/ornaments", "POST", settings);
        }

        private void SetUploading(bool loading) {
            _uploading = loading;
            _editorPanel.SetLoading(loading);
        }

        #endregion

        #region Callbacks

        private void HandleCancelButtonClicked() {
            Dismiss();
        }

        private async void HandleOkButtonClicked() {
            if (_uploading) {
                return;
            }
            SetUploading(true);
            await UploadSettings();
            SetUploading(false);
            Dismiss();
        }

        #endregion
    }
}