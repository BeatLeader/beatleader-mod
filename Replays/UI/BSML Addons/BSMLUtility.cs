using System;
using System.Xml;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage;
using UnityEngine;
using HarmonyLib;

namespace BeatLeader.Replays.UI.BSML_Addons
{
    public static class BSMLUtility
    {
        private static Dictionary<string, GameObject> _objectsWithIds = new Dictionary<string, GameObject>();
        private static Dictionary<string, List<Component>> _componentsWithIds = new Dictionary<string, List<Component>>();

        public static event Action<XmlNode> onXmlParsed;

        public static bool TryGetObjectWithId(string id, out GameObject go)
        {
            go = default;
            if (_objectsWithIds.TryGetValue(id, out GameObject go2))
            {
                if (go2 == null)
                {
                    _objectsWithIds.Remove(id);
                    return false;
                }

                go = go2;
                return true;
            }
            return false;
        }
        public static bool TryGetComponentWithIdOfType<T>(string id, out T component) where T : Component
        {
            component = default;
            List<Component> componentsToRemove = new List<Component>();
            if (_componentsWithIds.TryGetValue(id, out List<Component> components))
            {
                foreach (var item in components)
                {
                    if (item != null && item.GetType() == typeof(T))
                    {
                        component = (T)item;
                    }
                    else if (item == null) componentsToRemove.Add(item);
                }
                componentsToRemove.ForEach(x => components.Remove(x));
                if (component != null)
                    return true;
            }
            return false;
        }
        public static void AddSpriteToBSMLCache(string name, Sprite sprite)
        {
            Utilities.spriteCache.Add(name, sprite);
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
        private static class BSMLIDLogger //hello, Karghoff, how is your day going?
        {
            private static void Postfix(XmlNode node, IEnumerable<BSMLParser.ComponentTypeWithData> componentInfo)
            {
                if (node.Attributes["id"] != null)
                {
                    Component component = componentInfo.First().component;
                    if (component == null) return;

                    string key = node.Attributes["id"].Value;
                    List<Component> components = component.GetComponents(typeof(Component)).ToList();
                    if (components != null)
                    {
                        bool flag = false;
                        if (_componentsWithIds.TryGetValue(key, out List<Component> list))
                        {
                            if (list == null || list.Count == 0 || list.AtLeastOneCellIsNull())
                            {
                                _componentsWithIds.Remove(key);
                                flag = true;
                            }
                        }
                        else flag = true;

                        if (flag) _componentsWithIds.Add(key, components);
                    }
                    GameObject go = component.gameObject;
                    if (go != null)
                    {
                        bool flag = false;
                        if (_objectsWithIds.TryGetValue(key, out GameObject go2))
                        {
                            if (go2 == null)
                            {
                                _objectsWithIds.Remove(key);
                                flag = true;
                            }
                        }
                        else flag = true;

                        if (flag) _objectsWithIds.Add(key, go);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(BSMLParser), "Parse", new Type[] { typeof(XmlNode), typeof(GameObject), typeof(object) })]
        private static class BSMLParserEventsHandler
        {
            private static void Postfix(XmlNode parentNode)
            {
                onXmlParsed?.Invoke(parentNode);
                //foreach (var item in _componentsWithIds)
                //{
                //    Debug.LogWarning(item.Key);
                //    foreach (var item2 in item.Value)
                //    {
                //        Debug.Log(item2);
                //    }
                //}
            }
        }
    }
}