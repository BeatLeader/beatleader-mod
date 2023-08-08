namespace BeatLeader {
    public class BoolToStringConverter : StringConverter<bool?> {
        protected override bool? ConvertTyped(string str) {
            return bool.TryParse(str, out var value) ? value : null;
        }
    }
}