using Reactive;
using Reactive.Yoga;
using UnityEngine;
using AnimationCurve = Reactive.AnimationCurve;

namespace BeatLeader.UI.Hub {
    internal static class ProfileReveal {
        public const float SectionDelay = 0.12f;
        public const float ItemDelay = 0.035f;
        public const float Duration = 0.6f;
        public const float TotalTime = 4f;
        public const float SlideDistance = 4f;

        public static void Restart(AnimatedState<float> time) {
            time.SetValueImmediate(0f);
            time.TargetValue = TotalTime;
        }

        public static float Progress(float time, float delay = 0f, float duration = Duration) {
            return AnimationCurve.EaseOutCubic.Evaluate(Mathf.Clamp01((time - delay) / duration));
        }

        public static MappedState<float, float> Stage(IState<float> time, float delay, float duration = Duration) {
            return time.Map(x => Progress(x, delay, duration));
        }

        public static MappedState<float, YogaFrame> Slide(IState<float> progress, float distance = SlideDistance) {
            return progress.Map(x => new YogaFrame { top = (1f - x) * distance });
        }

        public static MappedState<float, Vector3> Pop(IState<float> progress, float from = 0.6f) {
            return progress.Map(x => Vector3.one * Mathf.LerpUnclamped(from, 1f, AnimationCurve.EaseOutBack.Evaluate(x)));
        }
    }
}
