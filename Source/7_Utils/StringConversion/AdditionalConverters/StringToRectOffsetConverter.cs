using System.Linq;
using UnityEngine;

namespace BeatLeader {
    public class StringToRectOffsetConverter : StringConverter<RectOffset> {
        protected override RectOffset? ConvertTo(string str) {
            var coords = str.Split(',')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => Convert<int?>(x) ?? -1)
                .ToArray();
            if (coords.Contains(-1)) return null;
            return coords.Length switch {
                1 => new RectOffset {
                    left = coords[0],
                    top = coords[0],
                    right = coords[0],
                    bottom = coords[0],
                },
                4 => new RectOffset {
                    left = coords[0],
                    top = coords[1],
                    right = coords[2],
                    bottom = coords[3],
                },
                _ => null
            };
        }
    }
}