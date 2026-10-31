using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BeatLeader {
    internal static class DanceDreadSounds {
        private const int SampleRate = 44100;

        private static readonly float[][] vowelFormants = {
            new[] { 730f, 1090f, 2440f }, new[] { 530f, 1840f, 2480f }, new[] { 270f, 2290f, 3010f },
            new[] { 570f, 840f, 2410f }, new[] { 300f, 870f, 2240f }
        };

        private static readonly Dictionary<string, AudioClip?> voices = new();

        private static AudioClip? _heartbeat;
        private static AudioClip? _sting;
        private static AudioClip? _relief;
        private static AudioClip[]? _whispers;

        public static AudioClip Heartbeat => _heartbeat ??= Synthesize("DanceDreadHeartbeat", 0.45f,
            t => Thump(t, 0f, 0.11f, 68f, 42f) + 0.75f * Thump(t, 0.18f, 0.1f, 80f, 50f)
        );

        public static AudioClip Sting => _sting ??= Synthesize("DanceDreadSting", 2.2f, t => {
            var boom = Thump(t, 0f, 1.4f, 58f, 28f);
            var decay = Mathf.Exp(-t * 1.6f) * Mathf.Clamp01(t * 30f);
            var tremolo = 0.75f + 0.25f * Mathf.Sin(2f * Mathf.PI * 5.5f * t);
            var cluster = (Mathf.Sin(2f * Mathf.PI * 207.65f * t) + Mathf.Sin(2f * Mathf.PI * 220f * t) + 0.5f * Mathf.Sin(2f * Mathf.PI * 311.1f * t)) / 2.5f;
            return 0.8f * boom + 0.35f * cluster * decay * tremolo;
        });

        public static AudioClip Relief => _relief ??= Synthesize("DanceDreadRelief", 1.4f,
            t => 0.3f * Bell(t, 0f, 659.25f) + 0.25f * Bell(t, 0.12f, 987.77f)
        );

        public static AudioClip[] Whispers => _whispers ??= new[] {
            CreateWhisper(11), CreateWhisper(23), CreateWhisper(37), CreateWhisper(53)
        };

        public static AudioClip? Voice(string name) {
            if (voices.TryGetValue(name, out var clip)) return clip;
            clip = LoadWav($"voice-{name}.wav");
            voices[name] = clip;
            return clip;
        }

        #region Synthesis

        private static AudioClip CreateWhisper(int seed) {
            var random = new System.Random(seed);
            var syllables = random.Next(5, 9);
            var data = new float[(int)(SampleRate * (0.25f * syllables + 0.3f))];
            var position = 0;
            for (var s = 0; s < syllables && position < data.Length; s++) {
                var length = (int)(SampleRate * (0.16f + 0.12f * (float)random.NextDouble()));
                var formants = vowelFormants[random.Next(vowelFormants.Length)];
                var hiss = random.NextDouble() < 0.45 ? (int)(SampleRate * 0.07f) : 0;
                var filters = new Biquad[3];
                for (var f = 0; f < filters.Length; f++) filters[f] = Biquad.BandPass(formants[f], formants[f] / 120f);
                var hissFilter = Biquad.BandPass(6000f, 1.2f);
                for (var i = 0; i < hiss + length && position < data.Length; i++, position++) {
                    var noise = (float)random.NextDouble() * 2f - 1f;
                    float sample;
                    if (i < hiss) {
                        var x = (float)i / hiss;
                        sample = 0.5f * hissFilter.Process(noise) * Mathf.Sin(Mathf.PI * x);
                    } else {
                        var x = (float)(i - hiss) / length;
                        var envelope = Mathf.Clamp01(x * 8f) * Mathf.Pow(1f - x, 1.5f);
                        sample = (filters[0].Process(noise) + 0.6f * filters[1].Process(noise) + 0.3f * filters[2].Process(noise)) * envelope;
                    }
                    data[position] = sample;
                }
                position += (int)(SampleRate * 0.03f * (float)random.NextDouble());
            }
            var peak = 0.0001f;
            foreach (var v in data) peak = Mathf.Max(peak, Mathf.Abs(v));
            for (var i = 0; i < data.Length; i++) data[i] *= 0.8f / peak;
            return FromSamples($"DanceDreadWhisper{seed}", data, SampleRate);
        }

        private sealed class Biquad {
            private float _b0, _b2, _a1, _a2;
            private float _x1, _x2, _y1, _y2;

            public static Biquad BandPass(float frequency, float q) {
                var w0 = 2f * Mathf.PI * frequency / SampleRate;
                var alpha = Mathf.Sin(w0) / (2f * q);
                var a0 = 1f + alpha;
                return new Biquad {
                    _b0 = alpha / a0, _b2 = -alpha / a0,
                    _a1 = -2f * Mathf.Cos(w0) / a0, _a2 = (1f - alpha) / a0
                };
            }

            public float Process(float x) {
                var y = _b0 * x + _b2 * _x2 - _a1 * _y1 - _a2 * _y2;
                _x2 = _x1;
                _x1 = x;
                _y2 = _y1;
                _y1 = y;
                return y;
            }
        }

        private static float Thump(float t, float start, float length, float fromHz, float toHz) {
            var x = t - start;
            if (x < 0f || x > length) return 0f;
            var progress = x / length;
            var phase = 2f * Mathf.PI * (fromHz * x + (toHz - fromHz) * x * progress * 0.5f);
            var envelope = Mathf.Clamp01(x * 200f) * Mathf.Pow(1f - progress, 2f);
            return Mathf.Sin(phase) * envelope;
        }

        private static float Bell(float t, float start, float hz) {
            var x = t - start;
            if (x < 0f) return 0f;
            var envelope = Mathf.Clamp01(x * 80f) * Mathf.Exp(-x * 3.5f);
            return (Mathf.Sin(2f * Mathf.PI * hz * x) + 0.3f * Mathf.Sin(4f * Mathf.PI * hz * x)) * envelope;
        }

        #endregion

        #region Clips

        private static AudioClip? LoadWav(string file) {
            using var stream = ResourcesUtils.GetEmbeddedResourceStream($"{Plugin.ResourcesPath}.Halloween.{file}");
            if (stream == null) return null;
            using var reader = new BinaryReader(stream);
            if (new string(reader.ReadChars(4)) != "RIFF") return null;
            reader.ReadInt32();
            if (new string(reader.ReadChars(4)) != "WAVE") return null;
            int rate = 0, channels = 0, bits = 0;
            while (stream.Position + 8 <= stream.Length) {
                var id = new string(reader.ReadChars(4));
                var size = reader.ReadInt32();
                if (id == "fmt ") {
                    reader.ReadInt16();
                    channels = reader.ReadInt16();
                    rate = reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt16();
                    bits = reader.ReadInt16();
                    stream.Position += size - 16;
                } else if (id == "data") {
                    if (bits != 16 || channels != 1 || rate <= 0) return null;
                    var samples = new float[size / 2];
                    for (var i = 0; i < samples.Length; i++) samples[i] = reader.ReadInt16() / 32768f;
                    return FromSamples(file, samples, rate);
                } else {
                    stream.Position += size + (size & 1);
                }
            }
            return null;
        }

        private static AudioClip Synthesize(string name, float seconds, System.Func<float, float> sample) {
            var samples = new float[Mathf.CeilToInt(seconds * SampleRate)];
            for (var i = 0; i < samples.Length; i++) samples[i] = Mathf.Clamp(sample((float)i / SampleRate), -1f, 1f);
            return FromSamples(name, samples, SampleRate);
        }

        private static AudioClip FromSamples(string name, float[] samples, int rate) {
            var position = 0;
            return AudioClip.Create(name, samples.Length, 1, rate, false, data => {
                for (var i = 0; i < data.Length; i++, position++) {
                    data[i] = position < samples.Length ? samples[position] : 0f;
                }
            });
        }

        #endregion
    }
}
