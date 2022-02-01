using System;
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

        #endregion

        #region LoadAssets

        private static void LoadAssets(AssetBundle assetBundle) {
            MonkeyPrefab = assetBundle.LoadAsset<GameObject>("TemplatePrefab");
        }

        #endregion
    }
}