using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ChristmasTreeMoverControlPanel : ReeUIComponentV2 {
        #region Components

        [UIObject("loading-indicator"), UsedImplicitly]
        private GameObject _loadingIndicator = null!;

        [UIObject("bg"), UsedImplicitly]
        private GameObject _bg = null!;

        [UIComponent("modal"), UsedImplicitly]
        private ModalView _modal = null!;

        #endregion

        #region Setup

        private ChristmasTree _tree = null!;
        private Pose _initialPose;

        public void Setup(ChristmasTree tree) {
            _tree = tree;
        }

        public void Present() {
            _initialPose = _tree.transform.GetLocalPose();
            _modal.Show(true, true);
            _tree.SetMoverEnabled(true);
        }

        private void Dismiss() {
            _modal.Hide(true);
            _tree.SetMoverEnabled(false);
        }

        protected override void OnInitialize() {
            SetUploading(false);
        }

        #endregion

        #region Settings

        private bool _uploading;

        public void SetUploading(bool loading) {
            _loadingIndicator.SetActive(loading);
            _bg.SetActive(!loading);
            _uploading = loading;
        }

        private async Task<bool> UploadSettings() {
            var pos = (SerializablePose)_tree.transform.GetLocalPose();
            var res = await WebUtils.SendAsync($"{BLConstants.BEATLEADER_API_URL}/projecttree/game", "POST", pos);
            return res?.IsSuccessStatusCode ?? false;
        }

        #endregion

        #region Callbacks

        [UIAction("finish-click"), UsedImplicitly]
        private async void HandleFinishButtonClicked() {
            if (_uploading) {
                return;
            }
            SetUploading(true);
            if (!await UploadSettings()) {
                _tree.transform.SetLocalPose(_initialPose);
            }
            SetUploading(false);
            Dismiss();
        }

        #endregion
    }
}