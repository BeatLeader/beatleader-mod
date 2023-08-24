using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatLeader {
    public class StringToKeyValuePairConverter : StringConverter {
        private static readonly Type type = typeof(KeyValuePair<,>);
        
        protected override bool CanConvertTo(Type targetType) {
            return targetType.IsGenericType && targetType.GetGenericTypeDefinition() == type;
        }

        protected override object? ConvertTo(string str, Type targetType) {
            var pair = str.Split(new[] { " - " }, StringSplitOptions.None)
                .Select(static x => x.Trim())
                .ToArray();
            var generics = targetType.GetGenericArguments();
            return Activator.CreateInstance(
                targetType, 
                Convert(pair[0], generics[0]), 
                Convert(pair[1], generics[1]));
        }
    }
}