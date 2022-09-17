using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using BeatSaberMarkupLanguage;
using UnityEngine;
using HarmonyLib;

namespace BeatLeader.UI.BSML_Addons
{
    public static class BSMLUtility
    {
        private static Dictionary<string, Dictionary<string, GameObject>> _objectsWithIds = new Dictionary<string, Dictionary<string, GameObject>>();
        private static Dictionary<string, Dictionary<string, List<Component>>> _componentsWithIds = new Dictionary<string, Dictionary<string, List<Component>>>();

        public static event Action<XmlDocument> OnXmlParsed;

        public static bool TryGetObjectWithId(string docName, string id, out GameObject go)
        {
            go = default;
            if (_objectsWithIds.TryGetValue(docName, out Dictionary<string, GameObject> pairs)
                && pairs.TryGetValue(id, out GameObject go2))
            {
                if (go2 == null)
                {
                    pairs.Remove(id);
                    return false;
                }

                go = go2;
                return true;
            }
            return false;
        }
        public static bool TryGetComponentWithIdOfType<T>(string docName, string id, out T component) where T : Component
        {
            component = default;
            if (_componentsWithIds.TryGetValue(docName, out Dictionary<string, List<Component>> pairs)
                && pairs.TryGetValue(id, out List<Component> components))
            {
                List<Component> components2 = components;
                foreach (var item in components)
                {
                    if (item != null && item.GetType() == typeof(T))
                    {
                        component = (T)item;
                    }
                    else if (item == null) components.Remove(item);
                }
                if (component != null) return true;
            }
            return false;
        }
        public static void AddSpriteToBSMLCache(string name, Sprite sprite)
        {
            Utilities.spriteCache.Add(name, sprite);
        }
        public static Sprite LoadSprite(string location)
        {
            Sprite sprite = null;
            if (location.Length > 1 && location.StartsWith("#"))
            {
                string text = location.Substring(1);
                sprite = FindSpriteCached(text);
            }
            else Utilities.GetData(location, (byte[] data) =>
            {
                sprite = Utilities.LoadSpriteRaw(data);
                sprite.texture.wrapMode = TextureWrapMode.Clamp;
            });

            if (sprite == null)
            {
                throw new Exception($"Can not find sprite located at [{location}]!");
            }
            return sprite;
        }
        public static Sprite FindSpriteCached(string name)
        {
            if (Utilities.spriteCache.TryGetValue(name, out var value) && value != null)
            {
                return value;
            }

            Sprite[] array = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (Sprite sprite in array)
            {
                if (sprite.name.Length != 0)
                {
                    if (!Utilities.spriteCache.TryGetValue(sprite.name, out var value2) || value2 == null)
                    {
                        Utilities.spriteCache[sprite.name] = sprite;
                    }

                    if (sprite.name == name)
                    {
                        value = sprite;
                    }
                }
            }

            return value;
        }
        private static bool AtLeastOneCellIsNull<T>(this List<T> list)
        {
            foreach (var item in list)
            {
                if (item == null)
                    return true;
            }
            return false;
        }

        [HarmonyPatch(typeof(BSMLParser), "HandleTagNode")]
        private static class BSMLIDLogger
        {
            private static void Postfix(XmlNode node, IEnumerable<BSMLParser.ComponentTypeWithData> componentInfo)
            {
                if (node.Attributes["id"] == null) return;

                Component component = componentInfo.First().component;
                if (component == null) return;

                string key = node.Attributes["id"].Value;
                List<Component> components = component.GetComponents(typeof(Component)).ToList();
                if (components != null)
                {
                    if (_componentsWithIds.TryGetValue(node.OwnerDocument.Name, out Dictionary<string, List<Component>> pairs))
                    {
                        if (!pairs.TryGetValue(key, out List<Component> components2))
                            pairs.Add(key, components);
                        else if (components2.AtLeastOneCellIsNull())
                        {
                            components2.Clear();
                            components2.AddRange(components);
                        }
                    }
                    else
                    {
                        _componentsWithIds.Add(node.OwnerDocument.Name, new Dictionary<string, List<Component>>() { { key, components } });
                    }
                }

                GameObject go = component.gameObject;
                if (go != null)
                {
                    if (_objectsWithIds.TryGetValue(node.OwnerDocument.Name, out Dictionary<string, GameObject> pairs))
                    {
                        if (!pairs.TryGetValue(key, out GameObject go2))
                            pairs.Add(key, go);
                        else if (go2 == null)
                        {
                            pairs.Remove(key);
                            pairs.Add(key, go);
                        }
                    }
                    else
                    {
                        _objectsWithIds.Add(node.OwnerDocument.Name, new Dictionary<string, GameObject>() { { key, go } });
                    }
                }
            }
        }
        [HarmonyPatch(typeof(BSMLParser), "Parse", new Type[] { typeof(XmlNode), typeof(GameObject), typeof(object) })]
        private static class BSMLParserEventsHandler
        {
            private static void Postfix(XmlNode parentNode)
            {
                OnXmlParsed?.Invoke(parentNode.OwnerDocument);
            }
        }
    }
}