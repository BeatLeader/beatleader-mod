using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;

namespace BeatLeader.EyeTracking {
    internal sealed class OscEyeTrackingReceiver : IDisposable {
        private static readonly int[] steamLinkPorts = { 9000, 9015 };
        private static readonly long maxValueAge = Stopwatch.Frequency / 4;
        private static readonly long receivingTimeout = Stopwatch.Frequency;

        private static readonly byte[] bundleHeader = Encoding.ASCII.GetBytes("#bundle\0");
        private static readonly byte[] gazeAddress = Encoding.ASCII.GetBytes("/tracking/eye/CenterVecFull");
        private static readonly byte[] leftEyeLidAddress = Encoding.ASCII.GetBytes("/avatar/parameters/LeftEyeLid");
        private static readonly byte[] rightEyeLidAddress = Encoding.ASCII.GetBytes("/avatar/parameters/RightEyeLid");

        #region Values

        public readonly struct Snapshot {
            public readonly int sequence;
            public readonly Vector3? gaze;
            public readonly float? leftEyeClosed;
            public readonly float? rightEyeClosed;

            public Snapshot(int sequence, Vector3? gaze, float? leftEyeClosed, float? rightEyeClosed) {
                this.sequence = sequence;
                this.gaze = gaze;
                this.leftEyeClosed = leftEyeClosed;
                this.rightEyeClosed = rightEyeClosed;
            }
        }

        private struct TimedValue<T> where T : struct {
            public T value;
            public long timestamp;

            public T? GetFresh(long now) => timestamp != 0 && now - timestamp <= maxValueAge ? value : null;
        }

        private readonly object _lock = new();
        private TimedValue<Vector3> _gaze;
        private TimedValue<float> _leftEyeClosed;
        private TimedValue<float> _rightEyeClosed;
        private long _lastValueTimestamp;
        private int _sequence;

        public Snapshot GetSnapshot() {
            var now = Stopwatch.GetTimestamp();
            lock (_lock) {
                return new Snapshot(_sequence, _gaze.GetFresh(now), _leftEyeClosed.GetFresh(now), _rightEyeClosed.GetFresh(now));
            }
        }

        private void Set<T>(ref TimedValue<T> target, T value) where T : struct {
            var now = Stopwatch.GetTimestamp();
            lock (_lock) {
                target.value = value;
                target.timestamp = now;
                _lastValueTimestamp = now;
                _sequence++;
            }
        }

        private void SetGaze(Vector3 gaze) {
            var magnitude = gaze.magnitude;
            if (magnitude > 1e-5f && !float.IsInfinity(magnitude)) Set(ref _gaze, gaze / magnitude);
        }

        private void SetEyeClosed(ref TimedValue<float> target, float value) {
            if (!float.IsNaN(value)) Set(ref target, Mathf.Clamp01(value));
        }

        #endregion

        #region Status

        public string GetStatusText() {
            bool isReceiving;
            lock (_lock) {
                isReceiving = _lastValueTimestamp != 0 && Stopwatch.GetTimestamp() - _lastValueTimestamp < receivingTimeout;
            }
            if (isReceiving) return "<color=#88FF88>receiving</color>";
            if (_listeners.Count == 0) return "<color=#FF8888>ports are used by another app</color>";
            return "waiting. When streaming, enable OSC and eye tracking sharing in SteamVR Settings > Steam Link";
        }

        #endregion

        #region Listeners

        private readonly List<Listener> _listeners;
        private volatile bool _disposed;

        public OscEyeTrackingReceiver() {
            _listeners = steamLinkPorts.Select(TryListen).OfType<Listener>().ToList();
        }

        public void Dispose() {
            _disposed = true;
            foreach (var listener in _listeners) {
                listener.Close();
            }
        }

        private Listener? TryListen(int port) {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try {
                socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
            } catch (Exception ex) {
                socket.Close();
                Plugin.Log.Warn($"[EyeTracking] Unable to listen on port {port}: {ex.Message}");
                return null;
            }
            return new Listener(this, socket, port);
        }

        private sealed class Listener {
            private readonly OscEyeTrackingReceiver _receiver;
            private readonly Socket _socket;
            private readonly int _port;
            private readonly byte[] _buffer = new byte[ushort.MaxValue];
            private readonly float[] _args = new float[3];

            public Listener(OscEyeTrackingReceiver receiver, Socket socket, int port) {
                _receiver = receiver;
                _socket = socket;
                _port = port;
                new Thread(Run) { IsBackground = true, Name = $"BeatLeader OSC {port}" }.Start();
            }

            public void Close() {
                _socket.Close();
            }

            private void Run() {
                try {
                    while (!_receiver._disposed) {
                        var length = _socket.Receive(_buffer);
                        ParsePacket(0, length, 0);
                    }
                } catch (Exception ex) {
                    if (!_receiver._disposed) {
                        Plugin.Log.Warn($"[EyeTracking] Stopped listening on port {_port}: {ex.Message}");
                    }
                }
            }

            #region OSC

            private void ParsePacket(int start, int end, int depth) {
                if (!Matches(start, end, bundleHeader)) {
                    ParseMessage(start, end);
                    return;
                }
                if (depth >= 8) return;

                var i = start + bundleHeader.Length + 8;
                while (i + 4 <= end) {
                    var size = ReadInt32(_buffer, i);
                    i += 4;
                    if (size <= 0 || size > end - i) return;
                    ParsePacket(i, i + size, depth + 1);
                    i += size;
                }
            }

            private void ParseMessage(int start, int end) {
                var addressEnd = Array.IndexOf(_buffer, (byte)0, start, end - start);
                if (addressEnd <= start) return;
                var tagsStart = start + Pad4(addressEnd - start + 1);
                if (tagsStart >= end || _buffer[tagsStart] != (byte)',') return;
                var tagsEnd = Array.IndexOf(_buffer, (byte)0, tagsStart, end - tagsStart);
                if (tagsEnd < 0) return;
                var argsStart = tagsStart + Pad4(tagsEnd - tagsStart + 1);

                if (IsAddress(start, addressEnd, gazeAddress)) {
                    if (TryReadFloats(tagsStart, tagsEnd, argsStart, end, 3)) {
                        _receiver.SetGaze(new Vector3(_args[0], _args[1], _args[2]));
                    }
                } else if (IsAddress(start, addressEnd, leftEyeLidAddress)) {
                    if (TryReadFloats(tagsStart, tagsEnd, argsStart, end, 1)) {
                        _receiver.SetEyeClosed(ref _receiver._leftEyeClosed, _args[0]);
                    }
                } else if (IsAddress(start, addressEnd, rightEyeLidAddress)) {
                    if (TryReadFloats(tagsStart, tagsEnd, argsStart, end, 1)) {
                        _receiver.SetEyeClosed(ref _receiver._rightEyeClosed, _args[0]);
                    }
                }
            }

            private bool TryReadFloats(int tagsStart, int tagsEnd, int argsStart, int end, int count) {
                if (tagsEnd - tagsStart - 1 < count || argsStart + count * 4 > end) return false;
                for (var i = 0; i < count; i++) {
                    if (_buffer[tagsStart + 1 + i] != (byte)'f') return false;
                    _args[i] = ReadFloat(_buffer, argsStart + i * 4);
                }
                return true;
            }

            private bool IsAddress(int start, int end, byte[] address) {
                return end - start == address.Length && Matches(start, end, address);
            }

            private bool Matches(int start, int end, byte[] prefix) {
                if (end - start < prefix.Length) return false;
                for (var i = 0; i < prefix.Length; i++) {
                    if (_buffer[start + i] != prefix[i]) return false;
                }
                return true;
            }

            private static int Pad4(int length) => (length + 3) & ~3;

            #endregion
        }

        #endregion

        #region BigEndian

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatBits {
            [FieldOffset(0)] public int int32;
            [FieldOffset(0)] public float float32;
        }

        private static int ReadInt32(byte[] buffer, int index) {
            return (buffer[index] << 24) | (buffer[index + 1] << 16) | (buffer[index + 2] << 8) | buffer[index + 3];
        }

        private static float ReadFloat(byte[] buffer, int index) {
            return new FloatBits { int32 = ReadInt32(buffer, index) }.float32;
        }

        #endregion
    }
}
