using System.Linq;
using UnityEngine;

namespace BeatLeader {
    public class StringToVector3Converter : StringConverter<Vector3?> {
        protected override Vector3? ConvertTo(string str) {
            var arr = str.Split(',')
                .Select(static x => x.Trim()).ToArray();
            switch (arr.Length) {
                case 1: {
                    var val = Convert<float>(arr[0]);
                    return new Vector3(val, val, val);
                }
                case 3:
                    return new Vector3(
                        Convert<float>(arr[0]),
                        Convert<float>(arr[1]),
                        Convert<float>(arr[2])
                    );
                default: return null;
            }
        }
    }
}