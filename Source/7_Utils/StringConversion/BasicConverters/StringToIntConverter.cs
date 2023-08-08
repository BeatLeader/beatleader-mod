namespace BeatLeader {
    public class StringToIntConverter : StringConverter<int?> {
        protected override int? ConvertTo(string str) {
            return int.TryParse(str, out var number) ? number : null;
        }
    }
}