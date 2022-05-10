using System.Collections.Generic;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace BeatLeader.Components {
    internal class PlayerAvatar : ReeUIComponentV2 {
        #region ColorScheme

        private static readonly Dictionary<PlayerRole, ColorScheme> ColorSchemes = new() {
            {
                PlayerRole.Default, new ColorScheme(
                    new Color(0.0f, 0.0f, 0.1f, 0.3f),
                    new Color(0.0f, 0.0f, 0.0f),
                    new Color(0.0f, 0.0f, 0.0f),
                    0.0f
                )
            }, {
                PlayerRole.Tipper, new ColorScheme(
                    new Color(0.0f, 0.0f, 0.0f, 0.4f),
                    new Color(1.0f, 1.0f, 0.7f),
                    new Color(1.0f, 0.6f, 0.0f),
                    0.3f
                )
            }, {
                PlayerRole.Supporter, new ColorScheme(
                    new Color(0.0f, 0.0f, 0.0f, 0.4f),
                    new Color(1.0f, 1.0f, 0.7f),
                    new Color(1.0f, 0.6f, 0.0f),
                    0.8f
                )
            }, {
                PlayerRole.Sponsor, new ColorScheme(
                    new Color(0.0f, 0.0f, 0.1f, 0.4f),
                    new Color(1.0f, 1.0f, 0.6f),
                    new Color(1.0f, 0.3f, 0.0f),
                    1.0f
                )
            }
        };

        private void ApplyColorScheme(PlayerRole[] playerRoles) {
            var supporterRole = FormatUtils.GetSupporterRole(playerRoles);
            var scheme = ColorSchemes.ContainsKey(supporterRole) ? ColorSchemes[supporterRole] : ColorSchemes[PlayerRole.Default];
            scheme.Apply(_materialInstance);
        }

        private readonly struct ColorScheme {
            private static readonly int BackgroundColorPropertyId = Shader.PropertyToID("_BackgroundColor");
            private static readonly int RimColorPropertyId = Shader.PropertyToID("_RimColor");
            private static readonly int HaloColorPropertyId = Shader.PropertyToID("_HaloColor");
            private static readonly int WavesAmplitudePropertyId = Shader.PropertyToID("_WavesAmplitude");

            private readonly Color _backgroundColor;
            private readonly Color _rimColor;
            private readonly Color _haloColor;
            private readonly float _wavesAmplitude;

            public ColorScheme(Color backgroundColor, Color rimColor, Color haloColor, float wavesAmplitude) {
                _backgroundColor = backgroundColor;
                _rimColor = rimColor;
                _haloColor = haloColor;
                _wavesAmplitude = wavesAmplitude;
            }

            public void Apply(Material material) {
                material.SetColor(BackgroundColorPropertyId, _backgroundColor);
                material.SetColor(RimColorPropertyId, _rimColor);
                material.SetColor(HaloColorPropertyId, _haloColor);
                material.SetFloat(WavesAmplitudePropertyId, _wavesAmplitude);
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
        private PlayerRole[] _playerRoles;

        public void SetAvatar(string url, PlayerRole[] playerRoles) {
            if (url.Equals(_url)) return;
            _url = url;
            _playerRoles = playerRoles;
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
            ApplyColorScheme(_playerRoles);
            StartCoroutine(avatarImage.PlaybackCoroutine(_bufferTexture));
        }

        private void OnAvatarLoadFailed(string reason) {
            ShowTexture(BundleLoader.FileError.texture);
        }

        #endregion

        #region Image

        private static readonly int AvatarTexturePropertyId = Shader.PropertyToID("_AvatarTexture");
        private static readonly int FadeValuePropertyId = Shader.PropertyToID("_FadeValue");

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