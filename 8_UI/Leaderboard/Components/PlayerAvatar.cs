using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.PlayerAvatar.bsml")]
    internal class PlayerAvatar : ReeUIComponent {
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
            if (!IsActive) return;
            SetMaterialLazy();
            _materialInstance.SetFloat(FadeValuePropertyId, 1);
            var loadTask = AvatarStorage.GetPlayerAvatarCoroutine(_url, false, OnAvatarLoadSuccess, OnAvatarLoadFailed);
            StartCoroutine(loadTask);
        }

        private void OnAvatarLoadSuccess(AvatarImage avatarImage) {
            _materialInstance.SetFloat(FadeValuePropertyId, 0);
            _materialInstance.SetTexture(AvatarTexturePropertyId, avatarImage.Texture);
            if (avatarImage.PlaybackCoroutine == null) return;
            StartCoroutine(avatarImage.PlaybackCoroutine);
        }

        private void OnAvatarLoadFailed(string reason) { }

        #endregion

        #region Image

        [UIComponent("image-component"), UsedImplicitly]
        private ImageView _image;

        private Material _materialInstance;
        private bool _materialSet;

        private void SetMaterialLazy() {
            if (_materialSet) return;
            _materialInstance = Object.Instantiate(BundleLoader.PlayerAvatarMaterial);
            _image.material = _materialInstance;
            _materialSet = true;
        }

        #endregion
    }
}