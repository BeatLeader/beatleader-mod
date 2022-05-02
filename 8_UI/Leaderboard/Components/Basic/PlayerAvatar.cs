using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class PlayerAvatar : ReeUIComponentV2 {
        #region BufferTexture

        private const int Width = 200;
        private const int Height = 200;

        private RenderTexture _bufferTexture;

        protected override void OnAfterParse() {
            _bufferTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.Default, 10);
            _bufferTexture.Create();
        }

        protected override void OnDispose() {
            _bufferTexture.Release();
        }

        #endregion

        #region Events

        private void OnEnable() {
            UpdateAvatar();
        }

        private void OnDisable() {
            StopAllCoroutines();
        }

        #endregion

        #region SetAvatar

        private static readonly int AvatarTexturePropertyId = Shader.PropertyToID("_AvatarTexture");
        private static readonly int FadeValuePropertyId = Shader.PropertyToID("_FadeValue");

        private string _url;
        private Coroutine _playbackCoroutine;

        public void SetAvatar(string url) {
            if (url.Equals(_url)) return;
            _url = url;
            UpdateAvatar();
        }

        private void UpdateAvatar() {
            if (!gameObject.activeInHierarchy || _url == null) return;
            ShowSpinner();
            var loadTask = AvatarStorage.GetPlayerAvatarCoroutine(_url, false, OnAvatarLoadSuccess, OnAvatarLoadFailed);
            StartCoroutine(loadTask);
        }

        private void OnAvatarLoadSuccess(AvatarImage avatarImage) {
            ShowTexture(_bufferTexture);
            if (_playbackCoroutine != null) StopCoroutine(_playbackCoroutine);
            _playbackCoroutine = StartCoroutine(avatarImage.PlaybackCoroutine(_bufferTexture));
        }

        private void OnAvatarLoadFailed(string reason) {
            ShowTexture(BundleLoader.FileError.texture);
        }

        #endregion

        #region Image

        [UIComponent("image-component"), UsedImplicitly]
        private ImageView _image;

        private Material _materialInstance;
        private bool _materialSet;

        public void SetAlpha(float value) {
            _image.color = new Color(1, 1, 1, value);
        }

        private void ShowSpinner() {
            SetMaterialLazy();
            _materialInstance.SetFloat(FadeValuePropertyId, 0);
        }

        private void ShowTexture(Texture texture) {
            SetMaterialLazy();
            _materialInstance.SetFloat(FadeValuePropertyId, 1);
            _materialInstance.SetTexture(AvatarTexturePropertyId, texture);
        }

        private void SetMaterialLazy() {
            if (_materialSet) return;
            _materialInstance = Object.Instantiate(BundleLoader.PlayerAvatarMaterial);
            _image.material = _materialInstance;
            _materialSet = true;
        }

        #endregion
    }
}