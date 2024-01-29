namespace BeatLeader {
    public class Range {
        public readonly float Start;
        public readonly float End;
        public readonly float Amplitude;

        public Range(float start, float end) {
            Start = start;
            End = end;
            Amplitude = end - start;
        }

        public float GetRatioClamped(float value) {
            var ratio = GetRatio(value);
            return ratio switch {
                <= 0 => 0,
                >= 1 => 1,
                _ => ratio
            };
        }

        public float GetValueClamped(float ratio) {
            return ratio switch {
                <= 0 => Start,
                >= 1 => End,
                _ => SlideBy(ratio)
            };
        }

        public float GetRatio(float value) {
            return (value - Start) / Amplitude;
        }

        public float SlideBy(float ratio) {
            return Start + Amplitude * ratio;
        }
    }
}