namespace BeatLeader {
    public class StringToFloatConverter : StringConverter<float?> {
        protected override float? ConvertTo(string str) {
            return float.TryParse(str, out var number) ? number : null;
        }
    }
}