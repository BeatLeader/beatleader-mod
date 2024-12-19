using BeatLeader.API.Methods;
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
        private Vector3 _initialScale;

        public void Setup(ChristmasTree tree) {
            _tree = tree;
        }

        public void Present() {
            _initialPose = _tree.transform.GetLocalPose();
            _initialScale = _tree.transform.localScale;
            _modal.Show(true, true);
            _tree.SetMoverFull(true);
        }

        private void Dismiss() {
            _modal.Hide(true);
            _tree.SetMoverFull(false);
        }

        protected override void OnInitialize() {
            SetUploading(false);
            UpdateTreeRequest.AddStateListener(OnTreeRequestStateChanged);
        }

        protected override void OnDispose() {
            UpdateTreeRequest.RemoveStateListener(OnTreeRequestStateChanged);
        }

        #endregion

        #region Settings

        private bool _uploading;

        public void SetUploading(bool loading) {
            _loadingIndicator.SetActive(loading);
            _bg.SetActive(!loading);
            _uploading = loading;
        }

        private void UploadSettings() {
            var localPos = _tree.transform.GetLocalPose();

            var pos = new FullSerializablePose {
                position = localPos.position,
                rotation = localPos.rotation,
                scale = _tree.transform.localScale
            };
            UpdateTreeRequest.SendRequest(pos);
        }

        private void OnTreeRequestStateChanged(API.RequestState state, string? result, string failReason) {
            switch (state) {
                case API.RequestState.Started:
                    SetUploading(true);
                    break;
                case API.RequestState.Finished:
                    SetUploading(false);
                    ChristmasTreeRequest.SendRequest();
                    Dismiss();
                    break;
                case API.RequestState.Failed:
                    Plugin.Log.Error($"OnTreeRequestStateChanged {failReason}");
                    SetUploading(false);
                    _tree.transform.SetLocalPose(_initialPose);
                    _tree.transform.localScale = _initialScale;
                    Dismiss();
                    break;
            }
        }

        #endregion

        #region Callbacks

        [UIAction("finish-click"), UsedImplicitly]
        private void HandleFinishButtonClicked() {
            if (_uploading) {
                return;
            }
            UploadSettings();
        }

        [UIAction("reset-click"), UsedImplicitly]
        private void HandleResetButtonClicked() {
            _tree.transform.SetLocalPose(new FullSerializablePose {
                position = new Vector3(2.7f, 0f, 4f),
                rotation = new Quaternion(0, 0, 0, 1),
            });
            _tree.transform.localScale = Vector3.one * 1.7f;
        }

        #endregion
    }
}