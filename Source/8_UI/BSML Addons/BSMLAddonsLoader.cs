using System.Collections.Generic;
using BeatSaberMarkupLanguage.TypeHandlers;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage;
using UnityEngine;
using BeatLeader.Utils;
using BeatLeader.UI.BSML_Addons.Tags;
using BeatLeader.UI.BSML_Addons.TypeHandlers;
using BeatLeader.UI.BSML_Addons.Extensions;

namespace BeatLeader.UI.BSML_Addons {
    internal static class BSMLAddonsLoader {
        private static readonly Dictionary<string, Sprite> spritesToCache = new()
        {
            { "black-transparent-bg", BundleLoader.BlackTransparentBG },
            { "black-transparent-bg-outline", BundleLoader.BlackTransparentBGOutline },
            { "cyan-bg-outline", BundleLoader.CyanBGOutline },
            { "white-bg", BundleLoader.WhiteBG },
            { "closed-door-icon", BundleLoader.ClosedDoorIcon },
            { "opened-door-icon", BundleLoader.OpenedDoorIcon },
            { "edit-layout-icon", BundleLoader.EditLayoutIcon },
            { "settings-icon", BundleLoader.SettingsIcon },
            { "left-arrow-icon", BundleLoader.LeftArrowIcon },
            { "right-arrow-icon", BundleLoader.RightArrowIcon },
            { "play-icon", BundleLoader.PlayIcon },
            { "pause-icon", BundleLoader.PauseIcon },
            { "lock-icon", BundleLoader.LockIcon },
            { "warning-icon",  BundleLoader.WarningIcon },
            { "cross-icon", BundleLoader.CrossIcon },
            { "pin-icon", BundleLoader.PinIcon },
            { "align-icon", BundleLoader.AlignIcon },
            { "anchor-icon", BundleLoader.AnchorIcon },
            { "progress-ring-icon", BundleLoader.ProgressRingIcon },
        };

        private static readonly BSMLTag[] addonTags = new BSMLTag[] {
            new BetterButtonTag(),
            new BetterImageTag()
        };

        private static readonly TypeHandler[] addonHandlers = new TypeHandler[] {
            new BetterButtonHandler(),
            new BetterImageHandler(),
            new GenericSettingExtensionHandler(),
            new GraphicExtensionHandler(),
            new ImageViewExtensionHandler(),
            new LayoutElementExtensionHandler(),
            new ModalViewExtensionHandler()
        };

        private static bool _ready;

        public static void LoadAddons() {
            if (_ready) return;
            foreach (var sprite in spritesToCache) {
                BSMLUtility.AddSpriteToBSMLCache("bl-" + sprite.Key, sprite.Value);
            }

            foreach (var tag in addonTags) {
                BSMLParser.instance.RegisterTag(tag);
            }
            foreach (var handler in addonHandlers) {
                BSMLParser.instance.RegisterTypeHandler(handler);
            }
            _ready = true;
        }

        private static string ParseFromBundle(string nameInBundle) {
            string parsed = string.Empty;
            bool prevWasLower = false;
            for (int i = 0; i < nameInBundle.Length; i++) {
                var ch = nameInBundle[i];
                parsed += prevWasLower && !(prevWasLower = !char.IsUpper(ch)) ? "-" : string.Empty;
                parsed += char.ToLower(ch);
            }
            return parsed;
        }
    }
}
