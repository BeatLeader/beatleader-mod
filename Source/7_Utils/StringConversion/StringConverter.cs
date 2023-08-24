using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace BeatLeader {
    [PublicAPI]
    public abstract class StringConverter<T> : StringConverter {
        private readonly Type _type = typeof(T);
        
        protected abstract T? ConvertTo(string str);

        protected sealed override bool CanConvertTo(Type targetType) {
            return targetType == _type || targetType.IsSubclassOf(_type)
                || (targetType.IsValueType && _type == typeof(Nullable<>).MakeGenericType(targetType));
        }

        protected sealed override object? ConvertTo(string str, Type targetType) => ConvertTo(str);
    }

    [PublicAPI]
    public abstract class StringConverter {
        #region Abstraction

        protected abstract bool CanConvertTo(Type targetType);

        protected abstract object? ConvertTo(string str, Type targetType);

        #endregion

        private static readonly Dictionary<Type, StringConverter> cachedConverters = new();

        private static readonly HashSet<StringConverter> converters = new() {
            new StringToIntConverter(),
            new StringToFloatConverter(),
            new StringToDoubleConverter(),
            new StringToBoolConverter(),
            new StringToEnumConverter(),
            new StringToSpriteConverter(),
            new StringToRectOffsetConverter(),
            new StringToColorConverter(),
            new StringToVector3Converter(),
            new StringToArrayConverter(),
            new StringToKeyValuePairConverter()
        };

        public static bool AddConverter(StringConverter converter) {
            return converters.Add(converter);
        }

        public static bool RemoveConverter(StringConverter converter) {
            foreach (var item in cachedConverters
                .Where(x => x.Value == converter)) {
                cachedConverters.Remove(item.Key);
            }
            return converters.Remove(converter);
        }

        public static object? Convert(string str, Type targetType) {
            str = str.Trim();
            if (targetType == typeof(string) || targetType == typeof(object)) return str;
            if (!cachedConverters.TryGetValue(targetType, out var converter)) {
                converter = converters.FirstOrDefault(x => x.CanConvertTo(targetType));
                cachedConverters[targetType] = converter;
            }
            return converter?.ConvertTo(str, targetType);
        }

        public static T? Convert<T>(string str) {
            return Convert(str, typeof(T)) is T result ? result : default;
        }
    }
}