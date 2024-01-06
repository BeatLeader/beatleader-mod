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
        private class DummyBSMLTag : BSMLTag {
            public DummyBSMLTag(
                string name,
                Func<Transform, GameObject> constructor
            ) {
                Aliases = new[] { name };
                _constructor = constructor;
            }

            public override string[] Aliases { get; }

            private readonly Func<Transform, GameObject> _constructor;

            public override GameObject CreateObject(Transform parent) {
                return _constructor(parent);
            }
        }

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
            new ContentSizeFitterExtensionHandler(),
            new RectExtensionHandler(),
            new LayoutGroupExtensionHandler(),
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
                .Select(static x => (x, x.GetCustomAttribute<BSMLComponentAttribute>()))
                .Where(static x => x.Item2 is { Suppress: false })
                .Where(static x => x.Item1.IsSubclassOf(typeof(ReeUIComponentV3Base)))
                .Where(static x => x.Item1 is { IsGenericType: false, IsAbstract: false });
            
            foreach (var (type, attr) in bsmlComponentTypes) {
                try {
                    var tagMember = GetMemberWithAttributeOrThrow<BSMLConstructorAttribute>(type);
                    var handlerMember = GetMemberWithAttributeOrThrow<BSMLHandlerAttribute>(type);

                    handlerMember.GetValueImplicitly(null, out var handlerObj);
                    if (handlerObj is not TypeHandler handler) {
                        throw new InvalidCastException("The field is not a valid TypeHandler");
                    }
                    if (tagMember is not MethodInfo method || !ValidateConstructorSignature(method)) {
                        throw new InvalidCastException("The constructor method has invalid signature");
                    }
                    var constructor = (Func<Transform, GameObject>)method.CreateDelegate(typeof(Func<Transform, GameObject>));

                    var name = attr.Name ?? type.Name;
                    if (attr.Namespace is not null) {
                        name = $"{attr.Namespace}.{name}";
                    }

                    var tag = new DummyBSMLTag(name, constructor);
                    addonTags.Add(tag);
                    addonHandlers.Add(handler);

                    Plugin.Log.Debug($"UI component \"{name}\" registered into BSML");
                } catch (Exception ex) {
                    Plugin.Log.Error($"Failed to register UI component \"{type.Name}\" into BSML: \n{ex}");
                }
            }

            static bool ValidateConstructorSignature(MethodInfo info) {
                return info.ReturnType == typeof(GameObject) &&
                    info.GetParameters() is { Length: 1 } mtdParams &&
                    mtdParams[0].ParameterType == typeof(Transform);
            }

            static MemberInfo GetMemberWithAttributeOrThrow<T>(IReflect type) where T : Attribute {
                var member = type.GetMembers(ReflectionUtils.StaticFlags | BindingFlags.FlattenHierarchy)
                    .Where(static x => x.GetCustomAttribute<T>() is not null)
                    .FirstOrDefault();
                if (member is null) throw new Exception("Unable to acquire one of the required fields (BSMLConstructor, BSMLHandler)");
                return member;
            }
        }
    }
}