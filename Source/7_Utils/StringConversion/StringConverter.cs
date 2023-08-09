using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Utils;

namespace BeatLeader {
    public abstract class StringConverter<T> : StringConverter {
        protected sealed override Type Type { get; } = typeof(T);

        protected sealed override object? ConvertTo(string str, Type targetType) => ConvertTo(str);

        protected abstract T? ConvertTo(string str);
    }

    public abstract class StringConverter {
        #region Abstraction

        protected abstract Type Type { get; }

        protected abstract object? ConvertTo(string str, Type targetType);

        #endregion

        static StringConverter() {
            var defaultConverters = new List<StringConverter> {
                new StringToIntConverter(),
                new StringToFloatConverter(),
                new StringToDoubleConverter(),
                new StringToBoolConverter(),
                new StringToEnumConverter(),
                new StringToSpriteConverter(),
                new StringToRectOffsetConverter()
            };
            foreach (var converter in defaultConverters) AddConverter(converter);
        }

        private static readonly Dictionary<Type, StringConverter> converters = new();

        public static bool AddConverter(StringConverter converter) {
            return converters.TryAdd(converter.Type, converter);
        }

        public static bool RemoveConverter(StringConverter converter) {
            return converters.Remove(converter.Type);
        }

        public static object? Convert(string str, Type targetType) {
            if (!converters.TryGetValue(targetType, out var converter)) {
                converters.TryGetValue(typeof(Nullable<>).MakeGenericType(targetType), out converter);
            }
            converter ??= converters.FirstOrDefault(x => targetType.IsSubclassOf(x.Key)).Value;
            return converter?.ConvertTo(str, targetType);
        }

        public static T? Convert<T>(string str) {
            return Convert(str, typeof(T)) is T result ? result : default;
        }
    }
}