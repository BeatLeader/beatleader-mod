using System;
using System.Runtime.InteropServices;

namespace BeatLeader.UI.Reactive.Yoga {
    [StructLayout(LayoutKind.Sequential)]
    internal struct YogaValue {
        public YogaValue(float value, Unit unit) {
            this.value = value;
            this.unit = unit;
        }
        
        public static readonly YogaValue Zero = new(0, Unit.Point);
        public static readonly YogaValue Undefined = new(float.NaN, Unit.Undefined);
        public static readonly YogaValue Auto = new(float.NaN, Unit.Auto);

        public float value;
        public Unit unit;

        public override string ToString() {
            return $"{value}.{unit}";
        }

        public static YogaValue Percent(float value) {
            return new YogaValue(value, Unit.Percent);
        }

        public static implicit operator YogaValue(float value) {
            return new YogaValue(value, Unit.Point);
        }

        public static implicit operator YogaValue(string str) {
            var unit = Unit.Undefined;
            var value = 0f;
            if (str is "auto") {
                unit = Unit.Auto;
            } else if (str.EndsWith("%")) {
                value = float.Parse(str.Replace("%", ""));
                unit = Unit.Percent;
            } else if (float.TryParse(str, out value)) {
                unit = Unit.Point;
            }
            return new YogaValue(value, unit);
        }

        public static bool operator ==(YogaValue left, YogaValue right) {
            return left.unit == right.unit && (left.unit is Unit.Undefined || Math.Abs(left.value - right.value) < 0.001f);
        }

        public static bool operator !=(YogaValue left, YogaValue right) {
            return left.unit != right.unit || (left.unit is not Unit.Undefined && Math.Abs(left.value - right.value) >= 0.001f);
        }
    }
}