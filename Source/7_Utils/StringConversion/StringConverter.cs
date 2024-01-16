using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Utils;
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

        #region Converters

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
            new StringToVector2Converter(),
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

        #endregion

        #region Convert

        public static object? Convert(string str, Type targetType) {
            str = str.Trim();
            if (CanSkipConversion(targetType)) {
                return str;
            }
            if (!TryConvertWithConverter(str, targetType, out var value)) {
                //trying to convert using implicit operator if failed
                TryConvertWithOp(str, targetType, out value);
            }
            return value;
        }

        public static T? Convert<T>(string str) {
            return Convert(str, typeof(T)) is T result ? result : default;
        }

        #endregion

        #region Convert Tools

        private static bool CanSkipConversion(Type type) {
            return type == typeof(string) || type == typeof(object);
        }

        private static bool TryConvertWithConverter(string str, Type targetType, out object? value) {
            if (!cachedConverters.TryGetValue(targetType, out var converter)) {
                converter = converters.FirstOrDefault(x => x.CanConvertTo(targetType));
                cachedConverters[targetType] = converter;
            }
            value = converter?.ConvertTo(str, targetType);
            return value is not null;
        }

        private static bool TryConvertWithOp(string str, Type targetType, out object? value) {
            return ReflectionUtils.CastValueOp(targetType, str, out value);
        }

        #endregion
    }
}