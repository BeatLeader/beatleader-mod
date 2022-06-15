using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.TypeHandlers;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage;
using UnityEngine;

namespace BeatLeader.Replays.UI.BSML_Addons
{
    public static class BSMLAddonsLoader
    {
        private static Dictionary<string, Sprite> _spritesToCache = new Dictionary<string, Sprite>()
        {
            { "beatleader-timeline-background", Sprite.Create(Utilities.FindTextureInAssembly("BeatLeader._9_Resources.Icons.TimelineBackground.png"),
                new Rect(0, 0, 356, 356), new Vector2(), 10, 0, SpriteMeshType.Tight, new Vector4(128, 128, 128, 128)) }
        };
        private static bool _wasLoaded;

        public static void LoadAddons()
        {
            if (_wasLoaded) return;
            foreach (var item in GetListOfType<BSMLTag>(Assembly.GetExecutingAssembly()))
                BSMLParser.instance.RegisterTag(item);

            List<TypeHandler> typeHandlers = GetListOfType<TypeHandler>(Assembly.GetExecutingAssembly());
            foreach (TypeHandler typeHandler in typeHandlers.ToArray())
            {
                Type type = (typeHandler.GetType().GetCustomAttributes(typeof(ComponentHandler), true).FirstOrDefault() as ComponentHandler)?.type;
                if (type == null)
                {
                    Plugin.Log.Warn($"TypeHandler {typeHandler.GetType().FullName} does not have the [ComponentHandler] attribute and will be ignored.");
                    typeHandlers.Remove(typeHandler);
                }
            }
            typeHandlers.ForEach(x => BSMLParser.instance.RegisterTypeHandler(x));
            foreach (var item in _spritesToCache)
            {
                BSMLUtility.AddSpriteToBSMLCache(item.Key, item.Value);
            }
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
