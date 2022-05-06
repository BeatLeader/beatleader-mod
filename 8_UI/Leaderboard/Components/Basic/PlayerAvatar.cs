using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace BeatLeader.Components {
    internal class PlayerAvatar : ReeUIComponentV2 {
        #region ColorScheme

        private static readonly int AvatarTexturePropertyId = Shader.PropertyToID("_AvatarTexture");
        private static readonly int FadeValuePropertyId = Shader.PropertyToID("_FadeValue");
        private static readonly int BackgroundColorPropertyId = Shader.PropertyToID("_BackgroundColor");
        private static readonly int RimColorPropertyId = Shader.PropertyToID("_RimColor");
        private static readonly int HaloColorPropertyId = Shader.PropertyToID("_HaloColor");

        private static readonly ColorScheme DefaultColorScheme = new(
            new Color(0.0f, 0.0f, 0.1f, 0.3f),
            new Color(0.0f, 0.0f, 0.0f),
            new Color(0.0f, 0.0f, 0.0f)
        );

        private static readonly ColorScheme AdminColorScheme = new(
            new Color(0.0f, 0.0f, 0.0f, 0.8f),
            new Color(1.0f, 0.9f, 0.75f),
            new Color(1.0f, 0.0f, 0.0f)
        );

        private static readonly ColorScheme SupporterColorScheme = new(
            new Color(0.0f, 0.0f, 0.0f, 0.8f),
            new Color(1.0f, 1.0f, 0.7f),
            new Color(1.0f, 0.6f, 0.0f)
        );

        private void ApplyColorScheme(PlayerRole playerRole) {
            var scheme = playerRole switch {
                PlayerRole.Default => DefaultColorScheme,
                PlayerRole.Admin => AdminColorScheme,
                PlayerRole.Supporter => SupporterColorScheme,
                _ => DefaultColorScheme
            };

            _materialInstance.SetColor(BackgroundColorPropertyId, scheme.BackgroundColor);
            _materialInstance.SetColor(RimColorPropertyId, scheme.RimColor);
            _materialInstance.SetColor(HaloColorPropertyId, scheme.HaloColor);
        }

        private struct ColorScheme {
            public readonly Color BackgroundColor;
            public readonly Color RimColor;
            public readonly Color HaloColor;

            public ColorScheme(Color backgroundColor, Color rimColor, Color haloColor) {
                BackgroundColor = backgroundColor;
                RimColor = rimColor;
                HaloColor = haloColor;
            }
        }

        #endregion

        #region BufferTexture

        private const int Width = 200;
        private const int Height = 200;
        private const float Scale = 1.5f;

        private RenderTexture _bufferTexture;

        protected override void OnInitialize() {
            _bufferTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.Default, 10);
            _bufferTexture.Create();
            _image.transform.localScale = new Vector3(Scale, Scale, Scale);
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

        private string _url;
        private PlayerRole _playerRole;

        public void SetAvatar(string url, PlayerRole playerRole) {
            if (url.Equals(_url)) return;
            _url = url;
            _playerRole = playerRole;
            UpdateAvatar();
        }

        private void UpdateAvatar() {
            if (!gameObject.activeInHierarchy || _url == null) return;
            ShowSpinner();
            StopAllCoroutines();
            var loadTask = AvatarStorage.GetPlayerAvatarCoroutine(_url, false, OnAvatarLoadSuccess, OnAvatarLoadFailed);
            StartCoroutine(loadTask);
        }

        private void OnAvatarLoadSuccess(AvatarImage avatarImage) {
            ShowTexture(_bufferTexture);
            ApplyColorScheme(_playerRole);
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