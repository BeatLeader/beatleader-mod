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
        public static Material UIAdditiveGlowMaterial;
        
        [UsedImplicitly]
        public static Sprite LocationIcon;
        
        [UsedImplicitly]
        public static Sprite RowSeparatorIcon;
        
        [UsedImplicitly]
        public static Sprite ScoreDetailsIcon;

        #endregion

        #region LoadAssets

        private static void LoadAssets(AssetBundle assetBundle) {
            MonkeyPrefab = assetBundle.LoadAsset<GameObject>("TemplatePrefab");
            LogoMaterial = assetBundle.LoadAsset<Material>("LogoMaterial");
            UIAdditiveGlowMaterial = assetBundle.LoadAsset<Material>("UIAdditiveGlow");
            LocationIcon = assetBundle.LoadAsset<Sprite>("LocationIcon");
            RowSeparatorIcon = assetBundle.LoadAsset<Sprite>("RowSeparatorIcon");
            ScoreDetailsIcon = assetBundle.LoadAsset<Sprite>("ScoreDetailsIcon");
        }

        #endregion
    }
}