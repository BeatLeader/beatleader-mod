namespace BeatLeader {
    public class Range {
        public float Start;
        public float End;
        public float Amplitude => End - Start;

        public Range() { }
        
        public Range(float start, float end) {
            Start = start;
            End = end;
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