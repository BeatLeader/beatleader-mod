using System;
using BeatLeader.Models.Replay;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.EyeTracking {
    [UsedImplicitly]
    internal class EyeTrackingRecorder : IInitializable, IDisposable, ILateTickable {
        [Inject, UsedImplicitly]
        private ReplayRecorder _replayRecorder = null!;

        [Inject, UsedImplicitly]
        private AudioTimeSyncController _timeSyncController = null!;

        private readonly EyeTrackingData _data = new();
        private float _lastTime = float.MinValue;
        private int _lastOscSequence;
        private bool _usedOpenXR;
        private bool _usedOsc;
        private double _gazePredictionSum;
        private int _gazePredictionCount;

        public void Initialize() {
            _data.runtime = EyeTrackingManager.Runtime;
            _replayRecorder.OnFinalizeReplay += HandleFinalizeReplay;
        }

        public void Dispose() {
            _replayRecorder.OnFinalizeReplay -= HandleFinalizeReplay;
        }

        public void LateTick() {
            var time = _timeSyncController.songTime;
            if (time <= _lastTime) return;

            var sample = new EyeTrackingSample { time = time };
            var hasOpenXRGaze = OpenXREyeGaze.TryGetGaze(out var gaze);
            if (hasOpenXRGaze) {
                sample.flags = EyeTrackingSampleFlags.Gaze | EyeTrackingSampleFlags.GazeFromOpenXR;
                sample.gaze = gaze;
                _usedOpenXR = true;
                if (OpenXREyeGaze.GazePrediction.IsMeasured) {
                    _gazePredictionSum += OpenXREyeGaze.GazePrediction.Seconds;
                    _gazePredictionCount++;
                }
            }

            var osc = EyeTrackingManager.Receiver?.GetSnapshot();
            if (osc.HasValue && (hasOpenXRGaze || osc.Value.sequence != _lastOscSequence)) {
                ApplyOsc(ref sample, osc.Value, hasOpenXRGaze);
            }

            if (sample.flags == EyeTrackingSampleFlags.None) return;
            _lastTime = time;
            _data.samples.Add(sample);
        }

        private void ApplyOsc(ref EyeTrackingSample sample, OscEyeTrackingReceiver.Snapshot osc, bool hasOpenXRGaze) {
            _lastOscSequence = osc.sequence;
            if (!hasOpenXRGaze && osc.gaze is { } oscGaze) {
                sample.flags |= EyeTrackingSampleFlags.Gaze;
                sample.gaze = oscGaze;
                _usedOsc = true;
            }
            if (osc.leftEyeClosed is { } leftEyeClosed) {
                sample.flags |= EyeTrackingSampleFlags.LeftEyeClosed;
                sample.leftEyeClosed = leftEyeClosed;
                _usedOsc = true;
            }
            if (osc.rightEyeClosed is { } rightEyeClosed) {
                sample.flags |= EyeTrackingSampleFlags.RightEyeClosed;
                sample.rightEyeClosed = rightEyeClosed;
                _usedOsc = true;
            }
        }

        private void HandleFinalizeReplay() {
            if (_data.samples.Count == 0) return;

            _data.source = _usedOpenXR && _usedOsc ? "OpenXR+SteamLinkOSC" : _usedOpenXR ? "OpenXR" : "SteamLinkOSC";
            _data.gazePosePrediction = _gazePredictionCount > 0 ? (float)(_gazePredictionSum / _gazePredictionCount) : float.NaN;
            _replayRecorder.TryWriteCustomData(EyeTrackingData.CustomDataKey, _data.Encode());
        }
    }
}
