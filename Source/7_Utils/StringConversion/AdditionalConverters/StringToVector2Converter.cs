using System.Linq;
using UnityEngine;

namespace BeatLeader {
    public class StringToVector2Converter : StringConverter<Vector2?> {
        protected override Vector2? ConvertTo(string str) {
            var arr = str.Split(',')
                .Select(static x => x.Trim()).ToArray();
            switch (arr.Length) {
                case 1:
                    var val = Convert<float>(arr[0]);
                    return new Vector2(val, val);
                case 2:
                    return new Vector2(
                        Convert<float>(arr[0]),
                        Convert<float>(arr[1])
                    );
                default: return null;
            }
        }
    }
}