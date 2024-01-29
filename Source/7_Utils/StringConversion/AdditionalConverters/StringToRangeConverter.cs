using System.Linq;

namespace BeatLeader {
    public class StringToRangeConverter : StringConverter<Range?> {
        protected override Range? ConvertTo(string str) {
            var arr = str.Split(',')
                .Select(static x => x.Trim()).ToArray();
            if (arr.Length is not 2) return null;
            return new Range(
                Convert<float>(arr[0]),
                Convert<float>(arr[1])
            );
        }
    }
}