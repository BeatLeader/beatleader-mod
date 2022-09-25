using BeatLeader.Models;
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

        private Texture _texture;
        private float _fadeValue;
        private float _hueShift;
        private float _saturation;

        private Material _materialInstance;
        private bool _materialSet;

        private void SelectMaterial(Player player) {
            GetPlayerSpecificSettings(player, out var baseMaterial, out _hueShift, out _saturation);
            if (!_materialSet || _materialInstance.shader != baseMaterial.shader) {
                if (_materialSet) Destroy(_materialInstance);
                _materialInstance = Instantiate(baseMaterial);
                _image.material = _materialInstance;
                _materialSet = true;
            }
            UpdateMaterialProperties();
        }

        private void UpdateMaterialProperties() {
            if (!_materialSet) return;
            _materialInstance.SetTexture(AvatarTexturePropertyId, _texture);
            _materialInstance.SetFloat(FadeValuePropertyId, _fadeValue);
            _materialInstance.SetFloat(HueShiftPropertyId, _hueShift);
            _materialInstance.SetFloat(SaturationPropertyId, _saturation);
        }

        private static void GetPlayerSpecificSettings(Player player, out Material baseMaterial, out float hueShift, out float saturation) {
            if (player.profileSettings == null) {
                hueShift = 0.0f;
                saturation = 1.0f;
                baseMaterial = BundleLoader.DefaultAvatarMaterial;
                return;
            }
            
            hueShift = (player.profileSettings.hue / 360.0f) * (Mathf.PI * 2);
            saturation = player.profileSettings.saturation;
            baseMaterial = player.profileSettings.effectName switch {
                "TheSun_Tier1" => BundleLoader.TheSunTier1Material,
                "TheSun_Tier2" => BundleLoader.TheSunTier2Material,
                "TheSun_Tier3" => BundleLoader.TheSunTier3Material,

                "TheMoon_Tier1" => BundleLoader.TheMoonTier1Material,
                "TheMoon_Tier2" => BundleLoader.TheMoonTier2Material,
                "TheMoon_Tier3" => BundleLoader.TheMoonTier3Material,

                "TheStar_Tier1" => BundleLoader.TheStarTier1Material,
                "TheStar_Tier2" => BundleLoader.TheStarTier2Material,
                "TheStar_Tier3" => BundleLoader.TheStarTier3Material,

                "Sparks_Tier1" => BundleLoader.SparksTier1Material,
                "Sparks_Tier2" => BundleLoader.SparksTier2Material,
                "Sparks_Tier3" => BundleLoader.SparksTier3Material,

                "Special_Tier1" => BundleLoader.SpecialTier1Material,
                "Special_Tier2" => BundleLoader.SpecialTier2Material,
                "Special_Tier3" => BundleLoader.SpecialTier3Material,

                _ => BundleLoader.DefaultAvatarMaterial
            };
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

        public void SetPlayer(Player player) {
            if (player.avatar.Equals(_url)) return;
            _url = player.avatar;
            SelectMaterial(player);
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
            StartCoroutine(avatarImage.PlaybackCoroutine(_bufferTexture));
        }

        private void OnAvatarLoadFailed(string reason) {
            ShowTexture(BundleLoader.FileError.texture);
        }

        #endregion

        #region Image

        [UIComponent("image-component"), UsedImplicitly]
        private ImageView _image;

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