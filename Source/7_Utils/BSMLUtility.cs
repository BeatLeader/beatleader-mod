using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using System.Reflection;

namespace BeatLeader.Utils {
    public static class BSMLUtility {
        public static string ReadViewDefinition(this Type type) {
            ViewDefinitionAttribute viewDefinitionAttribute;

            return ((viewDefinitionAttribute = type.GetCustomAttribute<ViewDefinitionAttribute>()) != null) ?
                 Utilities.GetResourceContent(type.Assembly, viewDefinitionAttribute.Definition) : string.Empty;
        }
    }
}