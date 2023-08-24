using System;

namespace BeatLeader {
    public class StringToEnumConverter : StringConverter {
        protected override bool CanConvertTo(Type targetType) => targetType.IsEnum;

        protected override object? ConvertTo(string str, Type targetType) {
            try {
                return Enum.Parse(targetType, str);
            } catch (Exception) {
                return null;
            }
        }
    }
}