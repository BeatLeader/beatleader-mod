using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Utils;
using UnityEngine;
using EyeTrackingData = BeatLeader.Models.Replay.EyeTrackingData;
using EyeTrackingSampleFlags = BeatLeader.Models.Replay.EyeTrackingSampleFlags;

namespace BeatLeader.Replayer.Emulation {
    internal class EyeTrackingTrack {
        private const float EyeTrackerLatency = 0.06f;
        private const float MaxGazeGap = 0.1f;
        private const float MaxEyelidsGap = 0.25f;
        private const float BlinkStart = 0.6f;
        private const float BlinkEnd = 0.95f;
        private const int SmoothingTaps = 5;
        private const float SmoothingTapSpacing = 0.004f;
        private const float SmoothingSigma = 0.01f;

        private readonly IReadOnlyList<PlayerMovementFrame> _frames;
        private readonly List<float> _frameTimes;
        private readonly List<float> _gazeTimes = new();
        private readonly List<Vector3> _gaze = new();
        private readonly List<float> _eyelidTimes = new();
        private readonly List<float> _eyesClosed = new();

        public EyeTrackingTrack(EyeTrackingData data, IReadOnlyList<PlayerMovementFrame> frames) {
            _frames = frames;
            _frameTimes = frames.Select(static x => x.time).ToList();

            foreach (var sample in data.samples) {
                var time = sample.time - EyeTrackerLatency;
                if ((sample.flags & EyeTrackingSampleFlags.Gaze) != 0) {
                    _gazeTimes.Add(time);
                    _gaze.Add(sample.gaze);
                }

                var hasLeft = (sample.flags & EyeTrackingSampleFlags.LeftEyeClosed) != 0;
                var hasRight = (sample.flags & EyeTrackingSampleFlags.RightEyeClosed) != 0;
                if (hasLeft || hasRight) {
                    _eyelidTimes.Add(time);
                    // One eyelid can read closed while the eye looks down, so it's a blink only when both close
                    _eyesClosed.Add(hasLeft && hasRight ? Mathf.Min(sample.leftEyeClosed, sample.rightEyeClosed) : hasLeft ? sample.leftEyeClosed : sample.rightEyeClosed);
                }
            }
        }

        // The eyes compensate head movements, so it's the gaze in the world that is steady and gets smoothed
        public bool TryGetGaze(float time, out Vector3 headPosition, out Vector3 direction, out float openness) {
            headPosition = default;
            direction = default;
            openness = 0f;
            if (!TryGetHeadPose(time, out var head) || !TryGetLocalGaze(time, out _)) return false;

            var gazeSum = Vector3.zero;
            var blinkSum = 0f;
            var weightSum = 0f;
            for (var i = -SmoothingTaps; i <= SmoothingTaps; i++) {
                var tapTime = time + i * SmoothingTapSpacing;
                if (!TryGetLocalGaze(tapTime, out var gaze) || !TryGetHeadPose(tapTime, out var tapHead)) continue;

                var offset = i * SmoothingTapSpacing / SmoothingSigma;
                var weight = Mathf.Exp(-0.5f * offset * offset);
                gazeSum += tapHead.rotation * gaze * weight;
                blinkSum += GetBlink(tapTime) * weight;
                weightSum += weight;
            }
            if (gazeSum.sqrMagnitude < 1e-8f) return false;

            headPosition = head.position;
            direction = gazeSum.normalized;
            openness = 1f - blinkSum / weightSum;
            return true;
        }

        private bool TryGetHeadPose(float time, out Pose pose) {
            if (!TryFind(_frameTimes, time, float.PositiveInfinity, out var i, out var t)) {
                pose = default;
                return false;
            }
            pose = _frames[i].headPose.Lerp(_frames[i + 1].headPose, t);
            return true;
        }

        private bool TryGetLocalGaze(float time, out Vector3 gaze) {
            if (!TryFind(_gazeTimes, time, MaxGazeGap, out var i, out var t)) {
                gaze = default;
                return false;
            }
            gaze = Vector3.Lerp(_gaze[i], _gaze[i + 1], t).normalized;
            return true;
        }

        private float GetBlink(float time) {
            if (!TryFind(_eyelidTimes, time, MaxEyelidsGap, out var i, out var t)) return 0f;
            return Mathf.InverseLerp(BlinkStart, BlinkEnd, Mathf.Lerp(_eyesClosed[i], _eyesClosed[i + 1], t));
        }

        private static bool TryFind(List<float> times, float time, float maxGap, out int index, out float t) {
            index = times.BinarySearch(time);
            if (index < 0) index = ~index - 1;
            if (index < 0 || index >= times.Count - 1 || times[index + 1] - times[index] > maxGap) {
                t = 0f;
                return false;
            }
            t = Mathf.InverseLerp(times[index], times[index + 1], time);
            return true;
        }
    }
}
