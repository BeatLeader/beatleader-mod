using System;
using BeatSaberMarkupLanguage;
using UnityEngine;
using BeatSaberMarkupLanguage.Attributes;
using System.Reflection;

namespace BeatLeader.Utils {
    public static class BSMLUtility {
        #region Parser

        private static GameObject Dummy => !_dummy ? _dummy = new GameObject("ParserDummy") : _dummy!;

        private static GameObject? _dummy;
        public static Transform Parse(string markup, object host) {
            if (!_dummy) _dummy = new GameObject("BSMLParserDummy");
            BSMLParser.instance.Parse(markup, _dummy, host);
            var transform = _dummy!.transform.GetChild(0);
            transform.SetParent(null, false);
            return transform;
        }

        #endregion

        #region Sprites

        public static void AddSpriteToBSMLCache(string name, Sprite sprite) {
            Utilities.spriteCache.Add(name, sprite);
        }

        public static Sprite LoadSprite(string location) {
            var sprite = default(Sprite?);
            if (location.Length > 1 && location.StartsWith("#")) {
                var text = location.Substring(1);
                sprite = FindSpriteCached(text);
            } else {
                Utilities.GetData(location, data => {
                    sprite = Utilities.LoadSpriteRaw(data);
                    sprite.texture.wrapMode = TextureWrapMode.Clamp;
                });
            }
            return sprite ?? throw new InvalidOperationException($"Cannot find sprite \"{location}\"!");
        }

        private static Sprite? FindSpriteCached(string name) {
            if (Utilities.spriteCache.TryGetValue(name, out var value) && value != null) return value;
            var array = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (var sprite in array) {
                if (sprite.name.Length == 0) continue;
                if (!Utilities.spriteCache.TryGetValue(sprite.name, out var value2) || value2 == null) {
                    Utilities.spriteCache[sprite.name] = sprite;
                }
                if (sprite.name == name) value = sprite;
            }
            return value;
        }

        #endregion

        #region Extensions

        public static bool ParseInObjectHierarchy<T>(this T obj, string? content = null, object? host = null) where T : Behaviour {
            content ??= ReadViewDefinition(host?.GetType() ?? typeof(T));
            if (string.IsNullOrEmpty(content)) return false;
            try {
                BSMLParser.instance.Parse(content, obj.gameObject, host ?? obj);
            } catch (Exception ex) {
                Debug.LogException(ex);
                return false;
            }
            return true;
        }

        public static string ReadViewDefinition(this Type type) {
            return type.GetCustomAttribute<ViewDefinitionAttribute>() is { } def ?
                Utilities.GetResourceContent(type.Assembly, def.Definition) : string.Empty;
        }

        #endregion

        #region Markup

        public static string ReadMarkupOrFallback(Type componentType) {
            var targetName = $"{componentType.Name}.bsml";

            var resource = componentType.ReadViewDefinition();
            if (resource != string.Empty) return resource;

            var strictMatch = true;
            FindResource: ;
            foreach (var resourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames()) {
                var actualResourceName = GetResourceName(resourceName);
                if (strictMatch ? actualResourceName != targetName : !resourceName.EndsWith(targetName)) continue;
                return Utilities.GetResourceContent(componentType.Assembly, resourceName);
            }

            if (!strictMatch) return $"<text text=\"Resource not found: {targetName}\" align=\"Center\"/>";
            strictMatch = false;
            goto FindResource;
        }

        private static string GetResourceName(string path) {
            var acc = -1;
            for (var i = path.Length - 1; i >= 0; i--) {
                if (path[i] is not '.') continue;
                if (acc != -1) {
                    acc = i;
                    break;
                }
                acc = i;
            }
            return path.Remove(0, acc + 1);
        }

        #endregion
    }
}