using System;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader {
    public static class BundleLoader {
        #region Initialize

        private const string BundleName = Plugin.ResourcesPath + ".AssetBundles.asset_bundle";
        private static bool _ready;

        public static void Initialize() {
            if (_ready) return;

            using var stream = ResourcesUtils.GetEmbeddedResourceStream(BundleName);
            var localAssetBundle = AssetBundle.LoadFromStream(stream);

            if (localAssetBundle == null) {
                throw new Exception("AssetBundle load error!");
            }

            LoadAssets(localAssetBundle);

            localAssetBundle.Unload(false);
            _ready = true;
        }

        #endregion

        #region Assets

        public static GameObject MonkeyPrefab;
        public static Material LogoMaterial;
        public static Material PlayerAvatarMaterial;
        public static Material UIAdditiveGlowMaterial;
        public static Material ScoreBackgroundMaterial;
        public static Material ScoreUnderlineMaterial;

        [UsedImplicitly]
        public static Sprite LocationIcon;

        [UsedImplicitly]
        public static Sprite RowSeparatorIcon;

        [UsedImplicitly]
        public static Sprite BeatLeaderLogoGradient;

        [UsedImplicitly]
        public static Sprite TransparentPixel;

        [UsedImplicitly]
        public static Sprite FileError;

        #endregion

        #region LoadAssets

        private static void LoadAssets(AssetBundle assetBundle) {
            MonkeyPrefab = assetBundle.LoadAsset<GameObject>("TemplatePrefab");

            LogoMaterial = assetBundle.LoadAsset<Material>("LogoMaterial");
            PlayerAvatarMaterial = assetBundle.LoadAsset<Material>("PlayerAvatarMaterial");
            UIAdditiveGlowMaterial = assetBundle.LoadAsset<Material>("UIAdditiveGlow");
            ScoreBackgroundMaterial = assetBundle.LoadAsset<Material>("ScoreBackgroundMaterial");
            ScoreUnderlineMaterial = assetBundle.LoadAsset<Material>("ScoreUnderlineMaterial");

            LocationIcon = assetBundle.LoadAsset<Sprite>("LocationIcon");
            RowSeparatorIcon = assetBundle.LoadAsset<Sprite>("RowSeparatorIcon");
            BeatLeaderLogoGradient = assetBundle.LoadAsset<Sprite>("BeatLeaderLogoGradient");
            TransparentPixel = assetBundle.LoadAsset<Sprite>("TransparentPixel");
            FileError = assetBundle.LoadAsset<Sprite>("FileError");
        }

        #endregion
    }
}