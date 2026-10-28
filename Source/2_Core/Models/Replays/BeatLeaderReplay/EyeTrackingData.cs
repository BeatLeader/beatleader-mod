using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UVector3 = UnityEngine.Vector3;

namespace BeatLeader.Models.Replay {
    public class EyeTrackingData {
        public const string CustomDataKey = "EyeTracking";
        public const byte FormatVersion = 2;

        private const int SampleSize = 4 + 1 + 3 * 4 + 2 * 4;

        public string source = "";
        public string runtime = "";
        public float gazePosePrediction = float.NaN;
        public List<EyeTrackingSample> samples = new();

        public byte[] Encode() {
            using var stream = new MemoryStream(64 + samples.Count * SampleSize);
            using var writer = new BinaryWriter(stream);

            writer.Write(FormatVersion);
            WriteString(source, writer);
            WriteString(runtime, writer);
            writer.Write(gazePosePrediction);
            writer.Write(samples.Count);
            foreach (var sample in samples) {
                writer.Write(sample.time);
                writer.Write((byte)sample.flags);
                writer.Write(sample.gaze.x);
                writer.Write(sample.gaze.y);
                writer.Write(sample.gaze.z);
                writer.Write(sample.leftEyeClosed);
                writer.Write(sample.rightEyeClosed);
            }
            writer.Flush();
            return stream.ToArray();
        }

        public static EyeTrackingData? Decode(byte[] bytes) {
            try {
                using var reader = new BinaryReader(new MemoryStream(bytes));

                if (reader.ReadByte() != FormatVersion) return null;
                var data = new EyeTrackingData {
                    source = ReadString(reader),
                    runtime = ReadString(reader),
                    gazePosePrediction = reader.ReadSingle()
                };
                var count = reader.ReadInt32();
                if (count < 0 || count > (reader.BaseStream.Length - reader.BaseStream.Position) / SampleSize) return null;

                data.samples.Capacity = count;
                for (var i = 0; i < count; i++) {
                    data.samples.Add(new EyeTrackingSample {
                        time = reader.ReadSingle(),
                        flags = (EyeTrackingSampleFlags)reader.ReadByte(),
                        gaze = new UVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                        leftEyeClosed = reader.ReadSingle(),
                        rightEyeClosed = reader.ReadSingle()
                    });
                }
                return data;
            } catch (EndOfStreamException) {
                return null;
            }
        }

        private static void WriteString(string value, BinaryWriter writer) {
            var bytes = Encoding.UTF8.GetBytes(value ?? "");
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadString(BinaryReader reader) {
            var length = reader.ReadInt32();
            if (length < 0 || length > reader.BaseStream.Length - reader.BaseStream.Position) {
                throw new EndOfStreamException();
            }
            return Encoding.UTF8.GetString(reader.ReadBytes(length));
        }
    }

    [Flags]
    public enum EyeTrackingSampleFlags : byte {
        None = 0,
        Gaze = 1 << 0,
        LeftEyeClosed = 1 << 1,
        RightEyeClosed = 1 << 2,
        GazeFromOpenXR = 1 << 3
    }

    public struct EyeTrackingSample {
        public float time;
        public EyeTrackingSampleFlags flags;
        public UVector3 gaze;
        public float leftEyeClosed;
        public float rightEyeClosed;
    }
}
