using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Utils
{
    public static class BSMLUtil
    {
        public static bool ParseInObjectHierarchy<T>(this T obj, string content = null, object host = null) where T : Behaviour
        {
            if (content == null && (content = ReadViewDefinition<T>()) == string.Empty) return false;
            try
            {
                BSMLParser.instance.Parse(content, obj.gameObject, host != null ? host : obj);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
            return true;
        }
        public static string ReadViewDefinition<T>()
        {
            return ReadViewDefinition(typeof(T));
        }
        public static string ReadViewDefinition(this Type type)
        {
            ViewDefinitionAttribute viewDefinitionAttribute;

            return ((viewDefinitionAttribute = type.GetCustomAttribute(typeof(ViewDefinitionAttribute)) as ViewDefinitionAttribute) != null) ?
                 Utilities.GetResourceContent(type.Assembly, viewDefinitionAttribute.Definition) : string.Empty;
        }
    }
}
