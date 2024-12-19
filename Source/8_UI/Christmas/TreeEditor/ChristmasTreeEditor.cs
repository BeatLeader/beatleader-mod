using System;
using System.Linq;
using System.Threading.Tasks;
using BeatLeader.API.Methods;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ChristmasTreeEditor : MonoBehaviour {
        #region Setup

        public bool IsOpened { get; private set; }

        public event Action? EditorClosedEvent;

        private OrnamentStorePanel _ornamentStore = null!;
        private BonusOrnamentStorePanel _bonusOrnamentStore = null!;
        private ChristmasTreeEditorControlPanel _editorPanel = null!;
        private ChristmasTree _tree = null!;

        private Vector3 _editorPos = new(0f, 0f, 1f);
        private Vector3 _initialPos;
        private float _editorScale = 1f;
        private float _initialScale;

        public void Setup(ChristmasTree tree) {
            _tree = tree;
            _ornamentStore.Setup(tree.OrnamentsPool);
            _bonusOrnamentStore.Setup(tree.OrnamentsPool);
            _editorPanel.Setup(tree);
        }

        public void Present() {
            IsOpened = true;
            _initialPos = _tree.transform.position;
            _initialScale = _tree.transform.localScale.y;
            _tree.ScaleTo(_editorScale);
            _tree.MoveTo(_editorPos);
            _tree.SetMoverRestricted(true);
            _tree.SetOrnamentsMovement(true);
            _ornamentStore.Present();
            _bonusOrnamentStore.Present();
            _editorPanel.Present();

            UploadTreeOrnamentsRequest.AddStateListener(OnTreeRequestStateChanged);
        }

        private void Dismiss() {
            IsOpened = false;
            _tree.MoveTo(_initialPos);
            _tree.ScaleTo(_initialScale);
            _tree.SetMoverRestricted(false);
            _tree.SetOrnamentsMovement(false);
            _ornamentStore.Dismiss();
            _bonusOrnamentStore.Dismiss();
            _editorPanel.Dismiss();
            EditorClosedEvent?.Invoke();

            UploadTreeOrnamentsRequest.RemoveStateListener(OnTreeRequestStateChanged);
        }

        private void Awake() {
            _ornamentStore = new GameObject("OrnamentStorePanel").AddComponent<OrnamentStorePanel>();
            var trans = _ornamentStore.transform;
            trans.SetParent(transform, false);
            trans.localPosition = new Vector3(-0.8f, 1f, 0.3f);
            trans.localEulerAngles = new Vector3(0f, 300f, 0f);
            
            _bonusOrnamentStore = new GameObject("BonusOrnamentStorePanel").AddComponent<BonusOrnamentStorePanel>();
            trans = _bonusOrnamentStore.transform;
            trans.SetParent(transform, false);
            trans.localPosition = new Vector3(-1.1f, 1f, -0.25f);
            trans.localEulerAngles = new Vector3(0f, 280f, 0f);

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
            var settings = _tree.Ornaments.Select(static x => x.GetSettings()).ToArray();
            UploadTreeOrnamentsRequest.SendRequest(settings);
        }

        private void SetUploading(bool loading) {
            _uploading = loading;
            _editorPanel.SetLoading(loading);
        }

        private void OnTreeRequestStateChanged(API.RequestState state, string? result, string failReason) {
            switch (state) {
                case API.RequestState.Started:
                    SetUploading(true);
                    break;
                case API.RequestState.Finished:
                    SetUploading(false);
                    Dismiss();
                    break;
                case API.RequestState.Failed:
                    Plugin.Log.Error($"OnTreeRequestStateChanged {failReason}");
                    SetUploading(false);
                    ChristmasTreeRequest.SendRequest();
                    Dismiss();
                    break;
            }
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
            UploadSettings();
        }

        #endregion
    }
}