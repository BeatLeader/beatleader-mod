using UnityEngine;

namespace BeatLeader {
    public class StringToColorConverter : StringConverter<Color> {
        protected override Color ConvertTo(string str) {
            ColorUtility.TryParseHtmlString(str, out var color);
            return color;
        }
    }
}