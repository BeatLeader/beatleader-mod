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
            foreach (var sprite in spritesToCache) {
                BSMLUtility.AddSpriteToBSMLCache("bl-" + sprite.Key, sprite.Value);
            }
            LoadBSMLComponents();
            foreach (var tag in addonTags) BSMLParser.instance.RegisterTag(tag);
            foreach (var handler in addonHandlers) BSMLParser.instance.RegisterTypeHandler(handler);
            _ready = true;
        }

        private static void LoadBSMLComponents() {
            var asm = Assembly.GetExecutingAssembly();
            var bsmlComponentTypes = asm.GetTypes()
                .Select(static x => (x, x.GetCustomAttributes<BSMLComponentAttribute>()))
                .Where(static x => x.Item2.All(static x => !x.Suppress))
                .Where(static x => x.Item1.IsSubclassOf(typeof(ReeUIComponentV3Base)))
                .Where(static x => x.Item1 is { IsGenericType: false, IsAbstract: false });

            foreach (var pair in bsmlComponentTypes) {
                var type = pair.Item1;
                try {
                    var tagMember = GetMemberWithAttributeOrThrow<BSMLTagAttribute>(type);
                    var handlerMember = GetMemberWithAttributeOrThrow<BSMLHandlerAttribute>(type);

                    tagMember.GetValueImplicitly(null, out var tagObj);
                    handlerMember.GetValueImplicitly(null, out var handlerObj);
                    if (tagObj is not BSMLTag tag || handlerObj is not TypeHandler handler) throw new InvalidCastException();

                    addonTags.Add(tag);
                    addonHandlers.Add(handler);

                    Plugin.Log.Debug($"UI component \"{tag.Aliases[0]}\" registered into BSML");
                } catch (Exception ex) {
                    Plugin.Log.Error($"Failed to register UI component \"{type.Name}\" into BSML: \n{ex}");
                }
            }

            static MemberInfo GetMemberWithAttributeOrThrow<T>(IReflect type) where T : Attribute {
                var member = type.GetMembers(ReflectionUtils.StaticFlags | BindingFlags.FlattenHierarchy)
                    .Where(static x => x.GetCustomAttribute<T>() is not null)
                    .FirstOrDefault();
                if (member is null) throw new Exception("Unable to acquire one of the required fields (BSMLTag, BSMLHandler)");
                return member;
            }
        }
    }
}