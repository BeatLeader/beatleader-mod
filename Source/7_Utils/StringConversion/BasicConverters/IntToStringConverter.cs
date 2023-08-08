namespace BeatLeader {
    public class IntToStringConverter : StringConverter<int?> {
        protected override int? ConvertTyped(string str) {
            return int.TryParse(str, out var number) ? number : null;
        }
    }
}