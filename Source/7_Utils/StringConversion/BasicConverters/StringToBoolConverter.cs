namespace BeatLeader {
    public class StringToBoolConverter : StringConverter<bool?> {
        protected override bool? ConvertTo(string str) {
            return bool.TryParse(str, out var value) ? value : null;
        }
    }
}