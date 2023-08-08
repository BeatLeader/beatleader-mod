using System;

namespace BeatLeader {
    public class EnumToStringConverter : StringConverter {
        protected override Type Type { get; } = typeof(Enum);

        protected override object? ConvertTo(string str, Type targetType) {
            try {
                return Enum.Parse(targetType, str);
            } catch (Exception) {
                return null;
            }
        }
    }
}