using BeatLeader.Models;
using BeatLeader.Themes;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace BeatLeader.Components {
    internal class PlayerAvatar : ReeUIComponentV2 {
        #region Material

        private static readonly int AvatarTexturePropertyId = Shader.PropertyToID("_AvatarTexture");
        private static readonly int FadeValuePropertyId = Shader.PropertyToID("_FadeValue");
        private static readonly int HueShiftPropertyId = Shader.PropertyToID("_HueShift");
        private static readonly int SaturationPropertyId = Shader.PropertyToID("_Saturation");
        private static readonly int ScalePropertyId = Shader.PropertyToID("_Scale");

        private Texture? _texture;
        private float _fadeValue;
        private float _hueShift;
        private float _saturation;

        private Material? _baseMaterial;
        private Material? _materialInstance;
        private bool _materialSet;

        private void SelectMaterial(IPlayerProfileSettings? profileSettings) {
            ThemesUtils.GetAvatarParams(profileSettings, _useSmallMaterialVersion, out var baseMaterial, out _hueShift, out _saturation);

            if (!_materialSet || baseMaterial != _baseMaterial) {
                _baseMaterial = baseMaterial;

                if (_materialSet) Destroy(_materialInstance);
                _materialInstance = Instantiate(baseMaterial);
                _image.material = _materialInstance;
                _materialSet = true;
                var scale = _materialInstance.GetFloat(ScalePropertyId);
                _image.transform.localScale = new Vector3(scale, scale, scale);
            }

            UpdateMaterialProperties();
        }

        private void UpdateMaterialProperties() {
            if (!_materialSet) return;
            _materialInstance!.SetTexture(AvatarTexturePropertyId, _texture);
            _materialInstance.SetFloat(FadeValuePropertyId, _fadeValue);
            _materialInstance.SetFloat(HueShiftPropertyId, _hueShift);
            _materialInstance.SetFloat(SaturationPropertyId, _saturation);
        }

        #endregion

        #region Initialize / Dispose / Setup

        private const int Width = 200;
        private const int Height = 200;

        private RenderTexture _bufferTexture = null!;
        private bool _useSmallMaterialVersion;

        protected override void OnInitialize() {
            _bufferTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.Default, 10);
            _bufferTexture.Create();
        }

        protected override void OnDispose() {
            _bufferTexture.Release();
        }

        public void Setup(bool useSmallMaterialVersion) {
            _useSmallMaterialVersion = useSmallMaterialVersion;
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

        private string? _url = "";

        public void SetAvatar(IPlayer? player) {
            SetAvatar(player?.AvatarUrl, player?.ProfileSettings);
        }

        public void SetAvatar(string? url, IPlayerProfileSettings? profileSettings) {
            if (url is null) {
                ShowSpinner();
                return;
            }
            if (_url == url) return;
            _url = url;
            SelectMaterial(profileSettings);
            UpdateAvatar();
        }

        private void UpdateAvatar() {
            if (!gameObject.activeInHierarchy) return;
            if (_url == null) {
                ShowTexture(BundleLoader.DefaultAvatar.texture);
                return;
            }
            ShowSpinner();
            StopAllCoroutines();
            var loadTask = AvatarStorage.GetPlayerAvatarCoroutine(_url, false, OnAvatarLoadSuccess, OnAvatarLoadFailed);
            StartCoroutine(loadTask);
        }

        private void OnAvatarLoadSuccess(AvatarImage avatarImage) {
            ShowTexture(_bufferTexture);
            StartCoroutine(avatarImage.PlaybackCoroutine(_bufferTexture));
        }

        private void OnAvatarLoadFailed(string reason) {
            ShowTexture(BundleLoader.DefaultAvatar.texture);
        }

        #endregion

        #region Image

        [UIComponent("image-component"), UsedImplicitly]
        private ImageView _image = null!;

        public void SetAlpha(float value) {
            _image.color = new Color(1, 1, 1, value);
        }

        private void ShowSpinner() {
            _fadeValue = 0.0f;
            UpdateMaterialProperties();
        }

        private void ShowTexture(Texture texture) {
            _fadeValue = 1.0f;
            _texture = texture;
            UpdateMaterialProperties();
        }

        #endregion
    }
}