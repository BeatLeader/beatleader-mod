using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Themes;
using JetBrains.Annotations;
using TMPro;
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

            LoadSprites(localAssetBundle);
            LoadMaterials(localAssetBundle);
            LoadPrefabs(localAssetBundle);
            LoadFonts(localAssetBundle);

            localAssetBundle.Unload(false);
            _ready = true;
        }

        #endregion

        #region Prefabs

        public static GameObject MonkeyPrefab;
        public static GameObject AccuracyGraphPrefab;
        public static ThemesCollection ThemesCollection;

        private static void LoadPrefabs(AssetBundle assetBundle) {
            MonkeyPrefab = assetBundle.LoadAsset<GameObject>("TemplatePrefab");
            AccuracyGraphPrefab = assetBundle.LoadAsset<GameObject>("AccuracyGraph");
            ThemesCollection = assetBundle.LoadAsset<ThemesCollection>("ThemesCollection");
        }

        #endregion

        #region Materials

        public static Material LogoMaterial;
        public static Material DefaultAvatarMaterial;
        public static Material UIAdditiveGlowMaterial;
        public static Material ScoreBackgroundMaterial;
        public static Material ScoreUnderlineMaterial;
        public static Material AccGridBackgroundMaterial;
        public static Material HandAccIndicatorMaterial;
        public static Material AccDetailsRowMaterial;
        public static Material ClanTagBackgroundMaterial;
        public static Material VotingButtonMaterial;
        public static Material MiniProfileBackgroundMaterial;
        public static Material UIGridMaterial;
        public static Material TextureSplitterMaterial;
        public static Material SkillTriangleMaterial;

        private static void LoadMaterials(AssetBundle assetBundle) {
            LogoMaterial = assetBundle.LoadAsset<Material>("LogoMaterial");
            DefaultAvatarMaterial = assetBundle.LoadAsset<Material>("DefaultAvatar");
            UIAdditiveGlowMaterial = assetBundle.LoadAsset<Material>("UIAdditiveGlow");
            UIAdditiveGlowMaterial.renderQueue = 4999;
            ScoreBackgroundMaterial = assetBundle.LoadAsset<Material>("ScoreBackgroundMaterial");
            ScoreUnderlineMaterial = assetBundle.LoadAsset<Material>("ScoreUnderlineMaterial");
            AccGridBackgroundMaterial = assetBundle.LoadAsset<Material>("AccGridBackgroundMaterial");
            HandAccIndicatorMaterial = assetBundle.LoadAsset<Material>("HandAccIndicatorMaterial");
            AccDetailsRowMaterial = assetBundle.LoadAsset<Material>("AccDetailsRowMaterial");
            ClanTagBackgroundMaterial = assetBundle.LoadAsset<Material>("ClanTagBackgroundMaterial");
            VotingButtonMaterial = assetBundle.LoadAsset<Material>("VotingButtonMaterial");
            MiniProfileBackgroundMaterial = assetBundle.LoadAsset<Material>("UIMiniProfileBackgroundMaterial");
            UIGridMaterial = assetBundle.LoadAsset<Material>("UIGridMaterial");
            TextureSplitterMaterial = assetBundle.LoadAsset<Material>("TextureSplitterMaterial");
            SkillTriangleMaterial = assetBundle.LoadAsset<Material>("UISkillTriangleMaterial");
        }

        #endregion

        #region Sprites

        [UsedImplicitly] public static Sprite LocationIcon;

        [UsedImplicitly] public static Sprite RowSeparatorIcon;

        [UsedImplicitly] public static Sprite BeatLeaderLogoGradient;

        [UsedImplicitly] public static Sprite TransparentPixel;

        [UsedImplicitly] public static Sprite FileError;

        [UsedImplicitly] public static Sprite NoModifiersIcon;
        [UsedImplicitly] public static Sprite NoPauseIcon;
        [UsedImplicitly] public static Sprite GolfIcon;
        [UsedImplicitly] public static Sprite SCPMIcon;
        [UsedImplicitly] public static Sprite GeneralContextIcon;

        [UsedImplicitly] public static Sprite Overview1Icon;

        [UsedImplicitly] public static Sprite Overview2Icon;

        [UsedImplicitly] public static Sprite DetailsIcon;

        [UsedImplicitly] public static Sprite GridIcon;

        [UsedImplicitly] public static Sprite GraphIcon;

        [UsedImplicitly] public static Sprite NotificationIcon;

        [UsedImplicitly] public static Sprite WebsiteLinkIcon;

        [UsedImplicitly] public static Sprite DiscordLinkIcon;

        [UsedImplicitly] public static Sprite PatreonLinkIcon;

        [UsedImplicitly] public static Sprite TwitterIcon;

        [UsedImplicitly] public static Sprite TwitchIcon;

        [UsedImplicitly] public static Sprite YoutubeIcon;

        [UsedImplicitly] public static Sprite ProfileIcon;

        [UsedImplicitly] public static Sprite FriendsIcon;

        [UsedImplicitly] public static Sprite IncognitoIcon;

        [UsedImplicitly] public static Sprite ReplayIcon;

        [UsedImplicitly] public static Sprite UIIcon;

        [UsedImplicitly] public static Sprite DebrisIcon;

        [UsedImplicitly] public static Sprite SceneIcon;

        [UsedImplicitly] public static Sprite JumpDistanceIcon;

        [UsedImplicitly] public static Sprite SaveIcon;

        [UsedImplicitly] public static Sprite AlignIcon;

        [UsedImplicitly] public static Sprite AnchorIcon;

        [UsedImplicitly] public static Sprite CrossIcon;

        [UsedImplicitly] public static Sprite EditLayoutIcon;

        [UsedImplicitly] public static Sprite ClosedDoorIcon;

        [UsedImplicitly] public static Sprite OpenedDoorIcon;

        [UsedImplicitly] public static Sprite ExitIcon;

        [UsedImplicitly] public static Sprite LeftArrowIcon;

        [UsedImplicitly] public static Sprite RightArrowIcon;

        [UsedImplicitly] public static Sprite LockIcon;

        [UsedImplicitly] public static Sprite PauseIcon;

        [UsedImplicitly] public static Sprite PlayIcon;

        [UsedImplicitly] public static Sprite PinIcon;

        [UsedImplicitly] public static Sprite ProgressRingIcon;

        [UsedImplicitly] public static Sprite RotateLeftIcon;

        [UsedImplicitly] public static Sprite RotateRightIcon;

        [UsedImplicitly] public static Sprite SettingsIcon;
        
        [UsedImplicitly] public static Sprite ReplayerSettingsIcon;

        [UsedImplicitly] public static Sprite WarningIcon;

        [UsedImplicitly] public static Sprite EyeIcon;

        [UsedImplicitly] public static Sprite BlackTransparentBG;

        [UsedImplicitly] public static Sprite CyanBGOutline;

        [UsedImplicitly] public static Sprite AnchorBGDots;

        [UsedImplicitly] public static Sprite BlackTransparentBGOutline;

        [UsedImplicitly] public static Sprite WhiteBG;

        [UsedImplicitly] public static Sprite DefaultAvatar;
        
        [UsedImplicitly] public static Sprite UnknownIcon;
        
        private static List<Sprite> _loadedSprites = null!;

        public static Sprite? GetSpriteFromBundle(string name) {
            return _ready ? _loadedSprites.FirstOrDefault(x => x.name == name) : null;
        }

        private static void LoadSprites(AssetBundle assetBundle) {
            LocationIcon = assetBundle.LoadAsset<Sprite>("LocationIcon");
            RowSeparatorIcon = assetBundle.LoadAsset<Sprite>("RowSeparatorIcon");
            BeatLeaderLogoGradient = assetBundle.LoadAsset<Sprite>("BeatLeaderLogoGradient");
            TransparentPixel = assetBundle.LoadAsset<Sprite>("TransparentPixel");
            FileError = assetBundle.LoadAsset<Sprite>("FileError");
            NoModifiersIcon = assetBundle.LoadAsset<Sprite>("BL_ContextNoModifiers");
            NoPauseIcon = assetBundle.LoadAsset<Sprite>("BL_ContextNoPause");
            GolfIcon = assetBundle.LoadAsset<Sprite>("BL_ContextGolf");
            SCPMIcon = assetBundle.LoadAsset<Sprite>("BL_ContextSCPM");
            GeneralContextIcon = assetBundle.LoadAsset<Sprite>("BL_ContextGeneral");
            Overview1Icon = assetBundle.LoadAsset<Sprite>("BL_Overview1Icon");
            Overview2Icon = assetBundle.LoadAsset<Sprite>("BL_Overview2Icon");
            DetailsIcon = assetBundle.LoadAsset<Sprite>("BL_DetailsIcon");
            GridIcon = assetBundle.LoadAsset<Sprite>("BL_GridIcon");
            GraphIcon = assetBundle.LoadAsset<Sprite>("BL_GraphIcon");
            NotificationIcon = assetBundle.LoadAsset<Sprite>("BL_NotificationIcon");
            WebsiteLinkIcon = assetBundle.LoadAsset<Sprite>("BL_Website");
            DiscordLinkIcon = assetBundle.LoadAsset<Sprite>("BL_Discord");
            PatreonLinkIcon = assetBundle.LoadAsset<Sprite>("BL_Patreon");
            TwitterIcon = assetBundle.LoadAsset<Sprite>("BL_TwitterIcon");
            TwitchIcon = assetBundle.LoadAsset<Sprite>("BL_TwitchIcon");
            YoutubeIcon = assetBundle.LoadAsset<Sprite>("BL_YoutubeIcon");
            ProfileIcon = assetBundle.LoadAsset<Sprite>("BL_ProfileIcon");
            FriendsIcon = assetBundle.LoadAsset<Sprite>("BL_FriendsIcon");
            IncognitoIcon = assetBundle.LoadAsset<Sprite>("BL_IncognitoIcon");
            ReplayIcon = assetBundle.LoadAsset<Sprite>("BL_ReplayIcon");
            UIIcon = assetBundle.LoadAsset<Sprite>("BL_UIIcon");
            DebrisIcon = assetBundle.LoadAsset<Sprite>("BL_DebrisIcon");
            SceneIcon = assetBundle.LoadAsset<Sprite>("BL_SceneIcon");
            JumpDistanceIcon = assetBundle.LoadAsset<Sprite>("BL_JumpDistanceIcon");
            SaveIcon = assetBundle.LoadAsset<Sprite>("BL_SaveIcon");
            AlignIcon = assetBundle.LoadAsset<Sprite>("BL_AlignIcon");
            AnchorIcon = assetBundle.LoadAsset<Sprite>("BL_AnchorIcon");
            CrossIcon = assetBundle.LoadAsset<Sprite>("BL_CrossIcon");
            EditLayoutIcon = assetBundle.LoadAsset<Sprite>("BL_EditLayoutIcon");
            ClosedDoorIcon = assetBundle.LoadAsset<Sprite>("BL_ClosedDoorIcon");
            OpenedDoorIcon = assetBundle.LoadAsset<Sprite>("BL_OpenedDoorIcon");
            ExitIcon = assetBundle.LoadAsset<Sprite>("BL_ExitIcon");
            LeftArrowIcon = assetBundle.LoadAsset<Sprite>("BL_LeftArrowIcon");
            RightArrowIcon = assetBundle.LoadAsset<Sprite>("BL_RightArrowIcon");
            LockIcon = assetBundle.LoadAsset<Sprite>("BL_LockIcon");
            PauseIcon = assetBundle.LoadAsset<Sprite>("BL_PauseIcon");
            PlayIcon = assetBundle.LoadAsset<Sprite>("BL_PlayIcon");
            PinIcon = assetBundle.LoadAsset<Sprite>("BL_PinIcon");
            ProgressRingIcon = assetBundle.LoadAsset<Sprite>("BL_ProgressRingIcon");
            RotateLeftIcon = assetBundle.LoadAsset<Sprite>("BL_RotateLeftIcon");
            RotateRightIcon = assetBundle.LoadAsset<Sprite>("BL_RotateRightIcon");
            ReplayerSettingsIcon = assetBundle.LoadAsset<Sprite>("BL_ReplayerSettingsIcon");
            SettingsIcon = assetBundle.LoadAsset<Sprite>("BL_SettingsIcon");
            WarningIcon = assetBundle.LoadAsset<Sprite>("BL_WarningIcon");
            EyeIcon = assetBundle.LoadAsset<Sprite>("BL_EyeIcon");
            BlackTransparentBG = assetBundle.LoadAsset<Sprite>("BL_BlackTransparentBG");
            AnchorBGDots = assetBundle.LoadAsset<Sprite>("BL_AnchorBGDots");
            BlackTransparentBGOutline = assetBundle.LoadAsset<Sprite>("BL_BlackTransparentBGOutline");
            WhiteBG = assetBundle.LoadAsset<Sprite>("BL_WhiteBG");
            CyanBGOutline = assetBundle.LoadAsset<Sprite>("BL_CyanBGOutline");
            DefaultAvatar = assetBundle.LoadAsset<Sprite>("BL_DefaultAvatar");
            UnknownIcon = assetBundle.LoadAsset<Sprite>("BL_UnknownIcon");
            _loadedSprites = assetBundle.LoadAllAssets<Sprite>().ToList();
        }

        #endregion

        #region Fonts

        public static TMP_FontAsset NotoSansFontAsset;
        public static TMP_FontAsset NotoSansJPFontAsset;
        public static TMP_FontAsset NotoSansSCFontAsset;
        public static TMP_FontAsset NotoSansKRFontAsset;
        public static TMP_FontAsset MinecraftEnchantmentFontAsset;

        private static readonly Dictionary<int, TMP_FontAsset> fontAssetsLookup = new();

        public static bool TryGetFontAsset(int hashCode, ref TMP_FontAsset fontAsset) {
            if (!fontAssetsLookup.ContainsKey(hashCode)) return false;
            fontAsset = fontAssetsLookup[hashCode];
            return true;
        }

        private static TMP_FontAsset LoadFontAsset(this AssetBundle assetBundle, string name) {
            var asset = assetBundle.LoadAsset<TMP_FontAsset>(name);
            if (asset != null) fontAssetsLookup[asset.hashCode] = asset;
            return asset;
        }

        private static void LoadFonts(AssetBundle assetBundle) {
            NotoSansFontAsset = assetBundle.LoadFontAsset("NotoSans-SemiBold SDF");
            NotoSansJPFontAsset = assetBundle.LoadFontAsset("NotoSansJP-SemiBold SDF");
            NotoSansSCFontAsset = assetBundle.LoadFontAsset("NotoSansSC-SemiBold SDF");
            NotoSansKRFontAsset = assetBundle.LoadFontAsset("NotoSansKR-SemiBold SDF");
            MinecraftEnchantmentFontAsset = assetBundle.LoadFontAsset("minecraft-enchantment SDF");
        }

        #endregion
    }
}