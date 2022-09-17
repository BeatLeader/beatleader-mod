using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.TypeHandlers;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage;
using UnityEngine;

namespace BeatLeader.UI.BSML_Addons
{
    internal static class BSMLAddonsLoader
    {
        private static Dictionary<string, Sprite> _spritesToCache = new Dictionary<string, Sprite>()
        {
            { "black-transparent-bg", BundleLoader.GetSpriteFromBundle("BlackTransparentBG") },
            { "black-transparent-bg-outline", BundleLoader.GetSpriteFromBundle("BlackTransparentBGWithOutline") },
            { "white-bg", BundleLoader.GetSpriteFromBundle("WhiteBG") },
            { "cyan-bg-outline", BundleLoader.GetSpriteFromBundle("CyanBGWithOutline") },
            { "yellow-bg-outline", BundleLoader.GetSpriteFromBundle("YellowBGWithOutline") },
            { "purple-bg-outline", BundleLoader.GetSpriteFromBundle("PurpleBGWithOutline") },
            { "closed-door-icon", BundleLoader.GetSpriteFromBundle("ClosedDoorIcon") },
            { "opened-door-icon", BundleLoader.GetSpriteFromBundle("OpenedDoorIcon") },
            { "edit-layout-icon", BundleLoader.GetSpriteFromBundle("EditLayoutIcon") },
            { "camera-icon", BundleLoader.GetSpriteFromBundle("CameraIcon") },
            { "settings-icon", BundleLoader.GetSpriteFromBundle("SettingsIcon") },
            { "left-arrow-icon", BundleLoader.GetSpriteFromBundle("LeftArrowIcon") },
            { "right-arrow-icon", BundleLoader.GetSpriteFromBundle("RightArrowIcon") },
            { "play-icon", BundleLoader.GetSpriteFromBundle("PlayIcon") },
            { "pause-icon", BundleLoader.GetSpriteFromBundle("PauseIcon") },
            { "lock-icon", BundleLoader.GetSpriteFromBundle("LockIcon") },
            { "warning-icon", BundleLoader.GetSpriteFromBundle("WarningIcon") },
            { "bad-cut-icon", BundleLoader.GetSpriteFromBundle("BadCutIcon") },
            { "rotate-left-icon", BundleLoader.GetSpriteFromBundle("RotateLeftIcon") },
            { "rotate-center-icon", BundleLoader.GetSpriteFromBundle("RotateCenterIcon") },
            { "rotate-right-icon", BundleLoader.GetSpriteFromBundle("RotateRightIcon") },
            { "vertical-sync-icon", BundleLoader.GetSpriteFromBundle("VerticalSyncIcon") },
            { "horizontal-sync-icon", BundleLoader.GetSpriteFromBundle("HorizontalSyncIcon") },
            { "pin-icon", BundleLoader.GetSpriteFromBundle("PinIcon") },
            { "align-icon", BundleLoader.GetSpriteFromBundle("AlignIcon") },
            { "reset-progress-icon", BundleLoader.GetSpriteFromBundle("ResetProgressIcon") },
        };
        private static bool _wasLoaded;

        public static void LoadAddons()
        {
            if (_wasLoaded) return;
            foreach (var item in GetListOfType<BSMLTag>(Assembly.GetExecutingAssembly()))
                BSMLParser.instance.RegisterTag(item);

            List<TypeHandler> typeHandlers = GetListOfType<TypeHandler>(Assembly.GetExecutingAssembly());
            foreach (TypeHandler typeHandler in typeHandlers)
            {
                Type type = (typeHandler.GetType().GetCustomAttributes(typeof(ComponentHandler), true).FirstOrDefault() as ComponentHandler)?.type;
                if (type == null)
                {
                    Plugin.Log.Warn($"TypeHandler {typeHandler.GetType().FullName} does not have the [ComponentHandler] attribute and will be ignored.");
                    typeHandlers.Remove(typeHandler);
                }
            }
            typeHandlers.ForEach(x => BSMLParser.instance.RegisterTypeHandler(x));
            _spritesToCache.ToList().ForEach(x => BSMLUtility.AddSpriteToBSMLCache(x.Key, x.Value));
            _wasLoaded = true;
        }
        private static List<T> GetListOfType<T>(Assembly assembly = null, params object[] constructorArgs)
        {
            if (assembly == null) assembly = Assembly.GetAssembly(typeof(T));
            List<T> objects = new List<T>();
            foreach (Type type in assembly.GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
                objects.Add((T)Activator.CreateInstance(type, constructorArgs));

            return objects;
        }
    }
}
