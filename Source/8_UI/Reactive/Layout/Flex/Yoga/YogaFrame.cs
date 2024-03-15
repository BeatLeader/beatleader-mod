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