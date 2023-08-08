namespace BeatLeader {
    public class DoubleToStringConverter : StringConverter<double?> {
        protected override double? ConvertTyped(string str) {
            return double.TryParse(str, out var number) ? number : null;
        }
    }
}