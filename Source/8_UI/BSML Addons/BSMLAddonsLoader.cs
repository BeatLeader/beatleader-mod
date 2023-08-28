using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BeatLeader.Components;
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
        private static readonly Dictionary<string, Sprite> spritesToCache = new() {
            { "black-transparent-bg", BundleLoader.BlackTransparentBG },
            { "black-transparent-bg-outline", BundleLoader.BlackTransparentBGOutline },
            { "cyan-bg-outline", BundleLoader.CyanBGOutline },
            { "white-bg", BundleLoader.WhiteBG },
            { "closed-door-icon", BundleLoader.ClosedDoorIcon },
            { "opened-door-icon", BundleLoader.OpenedDoorIcon },
            { "edit-layout-icon", BundleLoader.EditLayoutIcon },
            { "settings-icon", BundleLoader.SettingsIcon },
            { "replayer-settings-icon", BundleLoader.ReplayerSettingsIcon },
            { "left-arrow-icon", BundleLoader.LeftArrowIcon },
            { "right-arrow-icon", BundleLoader.RightArrowIcon },
            { "play-icon", BundleLoader.PlayIcon },
            { "pause-icon", BundleLoader.PauseIcon },
            { "lock-icon", BundleLoader.LockIcon },
            { "warning-icon", BundleLoader.WarningIcon },
            { "cross-icon", BundleLoader.CrossIcon },
            { "pin-icon", BundleLoader.PinIcon },
            { "align-icon", BundleLoader.AlignIcon },
            { "anchor-icon", BundleLoader.AnchorIcon },
            { "progress-ring-icon", BundleLoader.ProgressRingIcon },
            { "refresh-icon", BundleLoader.RotateRightIcon },
            { "battle-royale-icon", BundleLoader.BattleRoyaleIcon },
        };

        private static readonly List<BSMLTag> addonTags = new() {
            new BetterButtonTag(),
            new BetterImageTag()
        };

        private static readonly List<TypeHandler> addonHandlers = new() {
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
            foreach (var sprite in spritesToCache)
                BSMLUtility.AddSpriteToBSMLCache("bl-" + sprite.Key, sprite.Value);
            LoadReeUIComponentsV3();
            foreach (var tag in addonTags) BSMLParser.instance.RegisterTag(tag);
            foreach (var handler in addonHandlers) BSMLParser.instance.RegisterTypeHandler(handler);
            _ready = true;
        }

        private static void LoadReeUIComponentsV3() {
            const string DATA_METHOD_NAME = "GetBSMLData";

            var types = Assembly.GetExecutingAssembly().GetTypes().Where(x =>
                x.IsSubclassOf(typeof(ReeUIComponentV3Base)) && x is { IsGenericType:false, IsAbstract: false });
            foreach (var type in types) {
                try {
                    var method = type.GetMethod(DATA_METHOD_NAME, ReflectionUtils.StaticFlags | BindingFlags.FlattenHierarchy);
                    if (method is null) throw new MissingMethodException(type.Name, DATA_METHOD_NAME);
                    var (tag, handler) = ((BSMLTag, TypeHandler))method.Invoke(null, null);
                    addonTags.Add(tag);
                    addonHandlers.Add(handler);
                    Plugin.Log.Debug($"UI Component \"{tag.Aliases[0]}\" registered into BSML");
                } catch (Exception ex) {
                    Plugin.Log.Error($"Failed to get {type.Name} data: \n{ex}");
                }
            }
        }
    }
}