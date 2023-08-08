namespace BeatLeader {
    public class FloatToStringConverter : StringConverter<float?> {
        protected override float? ConvertTyped(string str) {
            return float.TryParse(str, out var number) ? number : null;
        }
    }
}