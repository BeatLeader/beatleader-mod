using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Polyglot;
using TMPro;
using UnityEngine;

namespace BeatLeader {
    public static class BLLocalization {
        #region Initialize

        private static LanguageSO _baseGameLanguageSO;
        private static bool _initialized;

        internal static void Initialize(MainSettingsModelSO mainSettingsModel) {
            if (_initialized) return;
            _baseGameLanguageSO = mainSettingsModel.language;
            _baseGameLanguageSO.didChangeEvent += OnBaseGameLanguageDidChange;
            OnBaseGameLanguageDidChange();
            _initialized = false;
        }

        #endregion

        #region Language

        private static Language _baseGameLanguage = Language.English;
        private static BLLanguage _blLanguageAnalog = BLLanguage.English;

        public static BLLanguage GetCurrentLanguage() {
            return PluginConfig.OverrideLanguage ? PluginConfig.SelectedLanguage : _blLanguageAnalog;
        }

        private static void OnBaseGameLanguageDidChange() {
            _baseGameLanguage = _baseGameLanguageSO.value;

            _blLanguageAnalog = _baseGameLanguage switch {
                Language.English => BLLanguage.English,
                Language.Russian => BLLanguage.Russian,
                Language.Japanese => BLLanguage.Japanese,
                Language.Simplified_Chinese => BLLanguage.Chinese,
                Language.Traditional_Chinese => BLLanguage.Chinese,
                Language.Korean => BLLanguage.Korean,
                Language.French => BLLanguage.French,
                Language.German => BLLanguage.German,
                Language.Spanish => BLLanguage.Spanish,
                _ => BLLanguage.English,
            };
        }

        public static List<BLLanguage> SupportedLanguagesSorted() {
            return new List<BLLanguage> {
                BLLanguage.English,
                BLLanguage.Japanese,
                BLLanguage.Russian,
                BLLanguage.Chinese,
                BLLanguage.Korean,
                BLLanguage.French,
                BLLanguage.German,
                BLLanguage.Spanish,
                BLLanguage.Norwegian,
                BLLanguage.MinecraftEnchantment
            };
        }

        public static string GetLanguageName(BLLanguage language) {
            return language switch {
                BLLanguage.English => "English",
                BLLanguage.Japanese => "Japanese",
                BLLanguage.Russian => "Russian",
                BLLanguage.Chinese => "Chinese",
                BLLanguage.Korean => "Korean",
                BLLanguage.French => "French",
                BLLanguage.German => "German",
                BLLanguage.Spanish => "Spanish",
                BLLanguage.Norwegian => "Norwegian",
                BLLanguage.MinecraftEnchantment => "Minecraft Enchantment",
                _ => "Unknown"
            };
        }

        #endregion

        #region Translations

        private const string LocalizationResourcePath = Plugin.ResourcesPath + ".Localization.json";
        private static readonly Dictionary<string, string> translations = new();
        private static BLLanguage _lastUsedLanguage = BLLanguage.None;

        private static void UpdateTranslationsIfNeeded(BLLanguage language) {
            if (_lastUsedLanguage == language) return;

            translations.Clear();

            if (JObject.Parse(ResourcesUtils.GetEmbeddedResourceText(LocalizationResourcePath))["Tokens"] is JArray tokensArray) {
                foreach (var tokenObject in tokensArray) {
                    var token = tokenObject.Value<string>("Token");
                    if (token == null || tokenObject["Translations"] is not JObject translationsObject) continue;
                    var translation = translationsObject[language.ToString()]?.Value<string>() ?? translationsObject["English"]?.Value<string>();
                    if (translation == null) continue;
                    translations[token] = translation;
                }
            }

            _lastUsedLanguage = language;
        }

        public static bool IsValidToken(string? token) {
            UpdateTranslationsIfNeeded(GetCurrentLanguage());
            return token != null && translations.ContainsKey(token);
        }

        public static string FromToken(string token) {
            UpdateTranslationsIfNeeded(GetCurrentLanguage());
            return translations.ContainsKey(token) ? translations[token] : token;
        }

        #endregion

        #region Fonts

        private static Material _defaultFontMaterial;
        private static bool _defaultFontInitialized;

        private static void LazyInitDefaultFont(TMP_Text textComponent) {
            if (_defaultFontInitialized) return;
            _defaultFontMaterial = textComponent.fontSharedMaterial;
            _defaultFontInitialized = true;
        }

        public static void UpdateFontAsset(TMP_Text textComponent) {
            LazyInitDefaultFont(textComponent);

            var fontAsset = GetLanguageFont();
            textComponent.font = fontAsset;
            if (fontAsset == null) {
                textComponent.fontSharedMaterial = _defaultFontMaterial;
            }
        }

        public static TMP_FontAsset? GetLanguageFont() {
            return GetCurrentLanguage() switch {
                BLLanguage.Japanese => BundleLoader.NotoSansJPFontAsset,
                BLLanguage.Russian => BundleLoader.NotoSansFontAsset,
                BLLanguage.Chinese => BundleLoader.NotoSansSCFontAsset,
                BLLanguage.Korean => BundleLoader.NotoSansKRFontAsset,
                BLLanguage.MinecraftEnchantment => BundleLoader.MinecraftEnchantmentFontAsset,
                BLLanguage.English => default,
                _ => default
            };
        }

        #endregion
    }
}