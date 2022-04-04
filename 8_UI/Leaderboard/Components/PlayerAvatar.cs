using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.PlayerAvatar.bsml")]
    internal class PlayerAvatar : ReeUIComponent {
        #region BufferTexture

        private const int Width = 256;
        private const int Height = 256;

        private RenderTexture _bufferTexture;

        protected override void OnInitialize() {
            _bufferTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.Default, 5);
            _bufferTexture.Create();
        }

        protected override void OnDispose() {
            _bufferTexture.Release();
        }

        #endregion

        #region Events

        protected override void OnActivate(bool firstTime) {
            UpdateAvatar();
        }

        protected override void OnDeactivate() {
            StopAllCoroutines();
        }

        #endregion

        #region SetAvatar

        private static readonly int AvatarTexturePropertyId = Shader.PropertyToID("_AvatarTexture");
        private static readonly int FadeValuePropertyId = Shader.PropertyToID("_FadeValue");

        private string _url;

        public void SetAvatar(string url) {
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
            StartCoroutine(avatarImage.PlaybackCoroutine(_bufferTexture));
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