namespace BeatLeader.UI.Reactive.Yoga {
    internal struct YogaFrame {
        public static readonly YogaFrame Zero = new() {
            top = YogaValue.Zero,
            left = YogaValue.Zero,
            right = YogaValue.Zero,
            bottom = YogaValue.Zero
        };

        public static readonly YogaFrame Auto = new() {
            top = YogaValue.Auto,
            left = YogaValue.Auto,
            right = YogaValue.Auto,
            bottom = YogaValue.Auto
        };

        public static readonly YogaFrame Undefined = new() {
            top = YogaValue.Undefined,
            left = YogaValue.Undefined,
            right = YogaValue.Undefined,
            bottom = YogaValue.Undefined
        };

        public YogaFrame(
            YogaValue top,
            YogaValue bottom,
            YogaValue left,
            YogaValue right
        ) {
            this.top = top;
            this.bottom = bottom;
            this.left = left;
            this.right = right;
        }
        
        public YogaFrame(YogaValue all) {
            top = all;
            bottom = all;
            left = all;
            right = all;
        }

        public YogaValue top;
        public YogaValue bottom;
        public YogaValue left;
        public YogaValue right;

        public static implicit operator YogaFrame(float value) {
            return new YogaFrame {
                top = value,
                bottom = value,
                left = value,
                right = value
            };
        }
    }
}