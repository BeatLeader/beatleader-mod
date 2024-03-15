using System;

namespace BeatLeader.UI.Reactive.Yoga {
    internal struct YogaVector {
        public static readonly YogaVector Auto = new() {
            x = YogaValue.Auto,
            y = YogaValue.Auto
        };
        
        public static readonly YogaVector Undefined = new() {
            x = YogaValue.Undefined,
            y = YogaValue.Undefined
        };
        
        public YogaVector(YogaValue x, YogaValue y) {
            this.x = x;
            this.y = y;
        }

        public YogaValue this[int idx] {
            get {
                return idx switch {
                    0 => x,
                    1 => y,
                    _ => throw new IndexOutOfRangeException()
                };
            }
        }

        public YogaValue x;
        public YogaValue y;
        
        public static bool operator ==(YogaVector left, YogaVector right) {
            return left.x == right.x && left.y == right.y;
        }
        
        public static bool operator !=(YogaVector left, YogaVector right) {
            return left.x != right.x || left.y != right.y;
        }
    }
}