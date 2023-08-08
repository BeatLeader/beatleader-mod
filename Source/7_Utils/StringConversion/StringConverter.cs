using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatLeader {
    public abstract class StringConverter<T> : StringConverter {
        protected sealed override Type Type { get; } = typeof(T);

        protected sealed override object? ConvertTo(string str, Type targetType) => ConvertTyped(str);

        protected abstract T? ConvertTyped(string str);
    }
    
    public abstract class StringConverter {
        #region Abstraction

        protected abstract Type Type { get; }

        protected abstract object? ConvertTo(string str, Type targetType);

        #endregion

        public static readonly List<StringConverter> Converters = new() {
            new IntToStringConverter(),
            new FloatToStringConverter(),
            new DoubleToStringConverter(),
            new BoolToStringConverter(),
            new EnumToStringConverter(),
            new SpriteToStringConverter()
        };
        
        public static object? Convert(string str, Type targetType) {
            var converter = Converters.FirstOrDefault(x => x.Type == targetType);
            converter ??= Converters.FirstOrDefault(x => targetType.IsSubclassOf(x.Type));
            return converter?.ConvertTo(str, targetType);
        }
    }
}