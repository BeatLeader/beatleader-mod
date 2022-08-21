using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader
{
    public static class BundleLoader
    {
        #region Initialize

        private const string BundleName = Plugin.ResourcesPath + ".AssetBundles.asset_bundle";
        private static AssetBundle _assetBundle;
        private static bool _ready;

        public static void Initialize()
        {
            if (_ready) return;

            using var stream = ResourcesUtils.GetEmbeddedResourceStream(BundleName);
            var localAssetBundle = AssetBundle.LoadFromStream(stream);

            if (localAssetBundle == null)
            {
                throw new Exception("AssetBundle load error!");
            }

            _assetBundle = localAssetBundle;
            LoadSprites(localAssetBundle);
            LoadMaterials(localAssetBundle);
            LoadPrefabs(localAssetBundle);

            localAssetBundle.Unload(false);
            _ready = true;
        }

        #endregion

        #region Prefabs

        public static GameObject MonkeyPrefab;
        public static GameObject AccuracyGraphPrefab;

        private static void LoadPrefabs(AssetBundle assetBundle)
        {
            MonkeyPrefab = assetBundle.LoadAsset<GameObject>("TemplatePrefab");
            AccuracyGraphPrefab = assetBundle.LoadAsset<GameObject>("AccuracyGraph");
        }

        #endregion

        #region Materials

        public static Material LogoMaterial;
        public static Material PlayerAvatarMaterial;
        public static Material UIAdditiveGlowMaterial;
        public static Material ScoreBackgroundMaterial;
        public static Material ScoreUnderlineMaterial;
        public static Material AccGridBackgroundMaterial;
        public static Material HandAccIndicatorMaterial;
        public static Material AccDetailsRowMaterial;
        public static Material ClanTagBackgroundMaterial;
        public static Material VotingButtonMaterial;
        public static Material UIBlurMaterial;

        private static void LoadMaterials(AssetBundle assetBundle)
        {
            LogoMaterial = assetBundle.LoadAsset<Material>("LogoMaterial");
            PlayerAvatarMaterial = assetBundle.LoadAsset<Material>("PlayerAvatarMaterial");
            UIAdditiveGlowMaterial = assetBundle.LoadAsset<Material>("UIAdditiveGlow");
            ScoreBackgroundMaterial = assetBundle.LoadAsset<Material>("ScoreBackgroundMaterial");
            ScoreUnderlineMaterial = assetBundle.LoadAsset<Material>("ScoreUnderlineMaterial");
            AccGridBackgroundMaterial = assetBundle.LoadAsset<Material>("AccGridBackgroundMaterial");
            HandAccIndicatorMaterial = assetBundle.LoadAsset<Material>("HandAccIndicatorMaterial");
            AccDetailsRowMaterial = assetBundle.LoadAsset<Material>("AccDetailsRowMaterial");
            ClanTagBackgroundMaterial = assetBundle.LoadAsset<Material>("ClanTagBackgroundMaterial");
            VotingButtonMaterial = assetBundle.LoadAsset<Material>("VotingButtonMaterial");
            UIBlurMaterial = assetBundle.LoadAsset<Material>("UIBlurMaterial");
        }

        #endregion

        #region Sprites

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

        [UsedImplicitly]
        public static Sprite ModifiersIcon;

        [UsedImplicitly]
        public static Sprite OverviewIcon;

        [UsedImplicitly]
        public static Sprite DetailsIcon;

        [UsedImplicitly]
        public static Sprite GridIcon;

        [UsedImplicitly]
        public static Sprite GraphIcon;

        [UsedImplicitly]
        public static Sprite NotificationIcon;

        [UsedImplicitly]
        public static Sprite WebsiteLinkIcon;

        [UsedImplicitly]
        public static Sprite DiscordLinkIcon;

        [UsedImplicitly]
        public static Sprite PatreonLinkIcon;

        private static List<Sprite> _loadedSprites;

        public static Sprite GetSpriteFromBundle(string name)
        {
            return _ready ? _loadedSprites.Where(x => x.name == name).FirstOrDefault() : null;
        }
        public static bool TryGetSpriteFromBundle(string name, out Sprite sprite)
        {
            return (sprite = GetSpriteFromBundle(name)) != null;
        }
        private static void LoadSprites(AssetBundle assetBundle)
        {
            LocationIcon = assetBundle.LoadAsset<Sprite>("LocationIcon");
            RowSeparatorIcon = assetBundle.LoadAsset<Sprite>("RowSeparatorIcon");
            BeatLeaderLogoGradient = assetBundle.LoadAsset<Sprite>("BeatLeaderLogoGradient");
            TransparentPixel = assetBundle.LoadAsset<Sprite>("TransparentPixel");
            FileError = assetBundle.LoadAsset<Sprite>("FileError");
            ModifiersIcon = assetBundle.LoadAsset<Sprite>("ModifiersIcon");
            OverviewIcon = assetBundle.LoadAsset<Sprite>("BL_OverviewIcon");
            DetailsIcon = assetBundle.LoadAsset<Sprite>("BL_DetailsIcon");
            GridIcon = assetBundle.LoadAsset<Sprite>("BL_GridIcon");
            GraphIcon = assetBundle.LoadAsset<Sprite>("BL_GraphIcon");
            NotificationIcon = assetBundle.LoadAsset<Sprite>("BL_NotificationIcon");
            WebsiteLinkIcon = assetBundle.LoadAsset<Sprite>("BL_Website");
            DiscordLinkIcon = assetBundle.LoadAsset<Sprite>("BL_Discord");
            PatreonLinkIcon = assetBundle.LoadAsset<Sprite>("BL_Patreon");
            _loadedSprites = assetBundle.LoadAllAssets<Sprite>().ToList();
        }

        #endregion
    }
}