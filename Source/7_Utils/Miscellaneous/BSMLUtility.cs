using System;
using BeatSaberMarkupLanguage;
using UnityEngine;
using BeatSaberMarkupLanguage.Attributes;
using System.Reflection;

namespace BeatLeader.Utils {
    public static class BSMLUtility {
        public static string ReadViewDefinition(this Type type) {
            return type.GetCustomAttribute<ViewDefinitionAttribute>() is { } def ?
                Utilities.GetResourceContent(type.Assembly, def.Definition) : string.Empty;
        }
    }
}