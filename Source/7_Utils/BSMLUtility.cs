using System;
using BeatSaberMarkupLanguage;
using UnityEngine;
using BeatSaberMarkupLanguage.Attributes;
using System.Reflection;
using System.Threading.Tasks;

namespace BeatLeader.Utils {
    public static class BSMLUtility {
        public static void AddSpriteToBSMLCache(string name, Sprite sprite) {
            Utilities.SpriteCache.Add(name, sprite);
        }
        public static Sprite LoadSprite(string location) {
            Sprite sprite = null;
            if (location.Length > 1 && location.StartsWith("#")) {
                string text = location.Substring(1);
                sprite = FindSpriteCached(text);
            } else {
                var task = Task.Run(async () => await Utilities.LoadSpriteFromAssemblyAsync(location));
                sprite = task.GetAwaiter().GetResult();
                sprite.texture.wrapMode = TextureWrapMode.Clamp;
            }

            if (sprite == null) {
                throw new Exception($"Can not find sprite located at [{location}]!");
            }
            return sprite;
        }
        public static Sprite FindSpriteCached(string name) {
            if (Utilities.SpriteCache.TryGetValue(name, out var value) && value != null) {
                return value;
            }

            Sprite[] array = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (Sprite sprite in array) {
                if (sprite.name.Length != 0) {
                    if (!Utilities.SpriteCache.TryGetValue(sprite.name, out var value2) || value2 == null) {
                        Utilities.SpriteCache[sprite.name] = sprite;
                    }

                    if (sprite.name == name) {
                        value = sprite;
                    }
                }
            }

            return value;
        }
        public static bool ParseInObjectHierarchy<T>(this T obj, string content = null, object host = null) where T : Behaviour {
            content ??= ReadViewDefinition(host?.GetType() ?? typeof(T));
            if (string.IsNullOrEmpty(content)) return false;
            try {
                BSMLParser.Instance.Parse(content, obj.gameObject, host ?? obj);
            } catch (Exception ex) {
                Debug.LogException(ex);
                return false;
            }
            return true;
        }
        public static string ReadViewDefinition<T>() {
            return ReadViewDefinition(typeof(T));
        }
        public static string ReadViewDefinition(this Type type) {
            ViewDefinitionAttribute viewDefinitionAttribute;

            return ((viewDefinitionAttribute = type.GetCustomAttribute<ViewDefinitionAttribute>()) != null) ?
                 Utilities.GetResourceContent(type.Assembly, viewDefinitionAttribute.Definition) : string.Empty;
        }
    }
}