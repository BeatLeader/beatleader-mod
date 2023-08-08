namespace BeatLeader {
    public class StringToDoubleConverter : StringConverter<double?> {
        protected override double? ConvertTo(string str) {
            return double.TryParse(str, out var number) ? number : null;
        }
    }
}