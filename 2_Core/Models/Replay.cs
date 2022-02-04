using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Models
{
    class Replay
    {
        public ReplayInfo info = new();

        public List<Frame> head = new();
        public List<Frame> left = new();
        public List<Frame> right = new();

        public List<NoteCutEvent> cuts = new();
        public List<NoteMissEvent> misses = new();
        public List<WallEvent> walls = new();
        public List<AutomaticHeight> height = new();
        public List<Pause> pauses = new();
    }

    class ReplayInfo
    {
        public string version;
        public string gameVersion;

        public string playerID;
        public string playerName;
        public string platform;
        public int hmd;

        public string hash;
        public string songName;
        public string mapper;
        public string difficulty;

        public string mode;
        public string environment;
        public string modifiers;
        public float jumpDistance;
        public bool leftHanded;
        public float height;

        public float startTime;
        public float failTime;
        public float speed;
    }

    class Frame
    {
        public float time;
        public EulerTransform transform;
    };

    class NoteCutEvent
    {
        public int noteHash;
        public float cutTime;
        public float spawnTime;
        public NoteCutInfo noteCutInfo;
    };

    class NoteMissEvent
    {
        public int noteHash;
        public float missTime;
        public float spawnTime;
    };

    class WallEvent
    {
        public int wallHash;
        public float energy;
        public float time;
        public float spawnTime;
    };

    class AutomaticHeight
    {
        public float height;
        public float time;
    };

    class Pause
    {
        public float duration;
        public float time;
    };

    class NoteCutInfo
    {
        public bool speedOK;
        public bool directionOK;
        public bool saberTypeOK;
        public bool wasCutTooSoon;
        public float saberSpeed;
        public Vector3 saberDir;
        public int saberType;
        public float timeDeviation;
        public float cutDirDeviation;
        public Vector3 cutPoint;
        public Vector3 cutNormal;
        public float cutDistanceToCenter;
        public float cutAngle;
        public float beforeCutRating;
        public float afterCutRating;
    };

    struct DifferentiatingNoteData
    {
        public float time;
        public int lineIndex;
        public int noteLineLayer;
        public int colorType;
        public int noteCutDirection;
    };

    enum StructType
    {
        info = 0,
        head = 1,
        left = 2,
        right = 3,
        noteCut = 4,
        noteMiss = 5,
        wall = 6,
        height = 7,
        pauses = 8
    }

    struct Vector3
    {
        public static implicit operator Vector3(UnityEngine.Vector3 unityVector) => new Vector3(unityVector);
        Vector3 (UnityEngine.Vector3 unityVector)
        {
            x = unityVector.x;
            y = unityVector.y;
            z = unityVector.z;
        }
        public float x;
        public float y;
        public float z;
    }

    class EulerTransform
    {
        public Vector3 position;
        public Vector3 rotation;
    }

    static class ReplayEncoder
    {
        public static void Encode(Replay replay, BinaryWriter stream)
        {
            stream.Write(0x442d3d69);
            stream.Write((byte)1);

            for (int a = 0; a < ((int)StructType.pauses) + 1; a++)
            {
                StructType type = (StructType)a;
                stream.Write((byte)a);

                switch (type)
                {
                    case StructType.info:
                        EncodeInfo(replay.info, stream);
                        break;
                    case StructType.head:
                        EncodeFrames(replay.head, stream);
                        break;
                    case StructType.left:
                        EncodeFrames(replay.left, stream);
                        break;
                    case StructType.right:
                        EncodeFrames(replay.right, stream);
                        break;
                    case StructType.noteCut:
                        EncodeNotes(replay.cuts, stream);
                        break;
                    case StructType.noteMiss:
                        EncodeMisses(replay.misses, stream);
                        break;
                    case StructType.wall:
                        EncodeWalls(replay.walls, stream);
                        break;
                    case StructType.height:
                        EncodeHeights(replay.height, stream);
                        break;
                    case StructType.pauses:
                        EncodePauses(replay.pauses, stream);
                        break;
                }
            }
        }

        static void EncodeInfo(ReplayInfo info, BinaryWriter stream)
        {
            EncodeString(info.version, stream);
            EncodeString(info.gameVersion, stream);

            EncodeString(info.playerID, stream);
            EncodeString(info.playerName, stream);
            EncodeString(info.platform, stream);
            stream.Write(info.hmd);

            EncodeString(info.hash, stream);
            EncodeString(info.songName, stream);
            EncodeString(info.mapper, stream);
            EncodeString(info.difficulty, stream);

            EncodeString(info.mode, stream);
            EncodeString(info.environment, stream);
            EncodeString(info.modifiers, stream);
            stream.Write(info.jumpDistance);
            stream.Write(info.leftHanded);
            stream.Write(info.height);

            stream.Write(info.startTime);
            stream.Write(info.failTime);
            stream.Write(info.speed);
        }

        static void EncodeFrames(List<Frame> frames, BinaryWriter stream)
        {
            stream.Write((uint)frames.Count);
            foreach (var frame in frames)
            {
                stream.Write(frame.time);
                EncodeVector(frame.transform.position, stream);
                EncodeVector(frame.transform.rotation, stream);
            }
        }

        static void EncodeNotes(List<NoteCutEvent> notes, BinaryWriter stream)
        {
            stream.Write((uint)notes.Count);
            foreach (var note in notes)
            {
                stream.Write(note.noteHash);
                stream.Write(note.cutTime);
                stream.Write(note.spawnTime);
                EncodeNoteInfo(note.noteCutInfo, stream);
            }
        }

        static void EncodeMisses(List<NoteMissEvent> misses, BinaryWriter stream)
        {
            stream.Write((uint)misses.Count);
            foreach (var miss in misses)
            {
                stream.Write(miss.noteHash);
                stream.Write(miss.missTime);
                stream.Write(miss.spawnTime);
            }
        }

        static void EncodeWalls(List<WallEvent> walls, BinaryWriter stream)
        {
            stream.Write((uint)walls.Count);
            foreach (var wall in walls)
            {
                stream.Write(wall.wallHash);
                stream.Write(wall.energy);
                stream.Write(wall.time);
                stream.Write(wall.spawnTime);
            }
        }

        static void EncodeHeights(List<AutomaticHeight> heights, BinaryWriter stream)
        {
            stream.Write((uint)heights.Count);
            foreach (var height in heights)
            {
                stream.Write(height.height);
                stream.Write(height.time);
            }
        }

        static void EncodePauses(List<Pause> pauses, BinaryWriter stream)
        {
            stream.Write((uint)pauses.Count);
            foreach (var pause in pauses)
            {
                stream.Write(pause.duration);
                stream.Write(pause.time);
            }
        }

        static void EncodeNoteInfo(NoteCutInfo info, BinaryWriter stream)
        {
            stream.Write(info.speedOK);
            stream.Write(info.directionOK);
            stream.Write(info.saberTypeOK);
            stream.Write(info.wasCutTooSoon);
            stream.Write(info.saberSpeed);
            EncodeVector(info.saberDir, stream);
            stream.Write(info.saberType);
            stream.Write(info.timeDeviation);
            stream.Write(info.cutDirDeviation);
            EncodeVector(info.cutPoint, stream);
            EncodeVector(info.cutNormal, stream);
            stream.Write(info.cutDistanceToCenter);
            stream.Write(info.cutAngle);
            stream.Write(info.beforeCutRating);
            stream.Write(info.afterCutRating);
        }

        static void EncodeString(string value, BinaryWriter stream)
        {
            string toEncode = value != null ? value : "";
            stream.Write(toEncode.Length);
            stream.Write(toEncode);
        }

        static void EncodeVector(Vector3 vector, BinaryWriter stream)
        {
            stream.Write(vector.x);
            stream.Write(vector.y);
            stream.Write(vector.z);
        }
    }

    static class ReplayDecoder
    {
        static Replay Decode(byte[] buffer)
        {
            int arrayLength = (int)buffer.Length;

            int pointer = 0;

            int magic = DecodeInt(buffer, ref pointer);
            byte version = buffer[pointer++];

            if (magic == 0x442d3d69 && version == 1)
            {
                Replay replay = new Replay();

                for (int a = 0; a < ((int)StructType.pauses) + 1 && pointer < arrayLength; a++)
                {
                    StructType type = (StructType)buffer[pointer++];

                    switch (type)
                    {
                        case StructType.info:
                            replay.info = DecodeInfo(buffer, ref pointer);
                            break;
                        case StructType.head:
                            replay.head = DecodeFrames(buffer, ref pointer);
                            break;
                        case StructType.left:
                            replay.left = DecodeFrames(buffer, ref pointer);
                            break;
                        case StructType.right:
                            replay.right = DecodeFrames(buffer, ref pointer);
                            break;
                        case StructType.noteCut:
                            replay.cuts = DecodeNotes(buffer, ref pointer);
                            break;
                        case StructType.noteMiss:
                            replay.misses = DecodeMisses(buffer, ref pointer);
                            break;
                        case StructType.wall:
                            replay.walls = DecodeWalls(buffer, ref pointer);
                            break;
                        case StructType.height:
                            replay.height = DecodeHeight(buffer, ref pointer);
                            break;
                        case StructType.pauses:
                            replay.pauses = DecodePauses(buffer, ref pointer);
                            break;
                        }
                }

                return replay;
            }
            else
            {
                return null;
            }
        }

        private static ReplayInfo DecodeInfo(byte[] buffer, ref int pointer)
        {
                ReplayInfo result = new();
                result.version = DecodeString(buffer, ref pointer);
                result.gameVersion = DecodeString(buffer, ref pointer);

                result.playerID = DecodeString(buffer, ref pointer);
                result.playerName = DecodeString(buffer, ref pointer);
                result.platform = DecodeString(buffer, ref pointer);
                result.hmd = DecodeInt(buffer, ref pointer);

                result.hash = DecodeString(buffer, ref pointer);
                result.songName = DecodeString(buffer, ref pointer);
                result.mapper = DecodeString(buffer, ref pointer);
                result.difficulty = DecodeString(buffer, ref pointer);

                result.mode = DecodeString(buffer, ref pointer);
                result.environment = DecodeString(buffer, ref pointer);
                result.modifiers = DecodeString(buffer, ref pointer);
                result.jumpDistance = DecodeFloat(buffer, ref pointer);
                result.leftHanded = DecodeBool(buffer, ref pointer);
                result.height = DecodeFloat(buffer, ref pointer);

                result.startTime = DecodeFloat(buffer, ref pointer);
                result.failTime = DecodeFloat(buffer, ref pointer);
                result.speed = DecodeFloat(buffer, ref pointer);

                return result;
         }

        private static List<Frame> DecodeFrames(byte[] buffer, ref int pointer)
        {
            int length = DecodeInt(buffer, ref pointer);
            List<Frame> result = new List<Frame>();
            for (int i = 0; i < length; i++)
            {
                result.Add(DecodeFrame(buffer, ref pointer));
            }
            return result;
        }

        private static Frame DecodeFrame(byte[] buffer, ref int pointer)
        {
            Frame result = new Frame();
            result.time = DecodeFloat(buffer, ref pointer);
            result.transform = DecodeEuler(buffer, ref pointer);

            return result;
        }

        private static List<NoteCutEvent> DecodeNotes(byte[] buffer, ref int pointer)
        {
            int length = DecodeInt(buffer, ref pointer);
            List<NoteCutEvent> result = new List<NoteCutEvent>();
            for (int i = 0; i < length; i++)
            {
                result.Add(DecodeNote(buffer, ref pointer));
            }
            return result;
        }

        private static List<NoteMissEvent> DecodeMisses(byte[] buffer, ref int pointer)
        {
            int length = DecodeInt(buffer, ref pointer);
            List<NoteMissEvent> result = new List<NoteMissEvent>();
            for (int i = 0; i < length; i++)
            {
                NoteMissEvent miss = new NoteMissEvent();
                miss.noteHash = DecodeInt(buffer, ref pointer);
                miss.missTime = DecodeFloat(buffer, ref pointer);
                miss.spawnTime = DecodeFloat(buffer, ref pointer);
                result.Add(miss);
            }
            return result;
        }

        private static List<WallEvent> DecodeWalls(byte[] buffer, ref int pointer)
        {
            int length = DecodeInt(buffer, ref pointer);
            List<WallEvent> result = new List<WallEvent>();
            for (int i = 0; i < length; i++)
            {
                WallEvent wall = new WallEvent();
                wall.wallHash = DecodeInt(buffer, ref pointer);
                wall.time = DecodeFloat(buffer, ref pointer);
                wall.energy = DecodeFloat(buffer, ref pointer);
                wall.spawnTime = DecodeFloat(buffer, ref pointer);
                result.Add(wall);
            }
            return result;
        }

        private static List<AutomaticHeight> DecodeHeight(byte[] buffer, ref int pointer)
        {
            int length = DecodeInt(buffer, ref pointer);
            List<AutomaticHeight> result = new List<AutomaticHeight>();
            for (int i = 0; i < length; i++)
            {
                AutomaticHeight height = new AutomaticHeight();
                height.height = DecodeFloat(buffer, ref pointer);
                height.time = DecodeFloat(buffer, ref pointer);
                result.Add(height);
            }
            return result;
        }

        private static List<Pause> DecodePauses(byte[] buffer, ref int pointer)
        {
            int length = DecodeInt(buffer, ref pointer);
            List<Pause> result = new List<Pause>();
            for (int i = 0; i < length; i++)
            {
                Pause pause = new Pause();
                pause.duration = DecodeFloat(buffer, ref pointer);
                pause.time = DecodeFloat(buffer, ref pointer);
                result.Add(pause);
            }
            return result;
        }

        private static NoteCutEvent DecodeNote(byte[] buffer, ref int pointer)
        {
            NoteCutEvent result = new NoteCutEvent();
            result.noteHash = DecodeInt(buffer, ref pointer);
            result.cutTime = DecodeFloat(buffer, ref pointer);
            result.spawnTime = DecodeFloat(buffer, ref pointer);
            result.noteCutInfo = DecodeCutInfo(buffer, ref pointer);

            return result;
        }

        private static NoteCutInfo DecodeCutInfo(byte[] buffer, ref int pointer)
        {
            NoteCutInfo result = new NoteCutInfo();
            result.speedOK = DecodeBool(buffer, ref pointer);
            result.directionOK = DecodeBool(buffer, ref pointer);
            result.saberTypeOK = DecodeBool(buffer, ref pointer);
            result.wasCutTooSoon = DecodeBool(buffer, ref pointer);
            result.saberSpeed = DecodeFloat(buffer, ref pointer);
            result.saberDir = DecodeVector3(buffer, ref pointer);
            result.saberType = DecodeInt(buffer, ref pointer);
            result.timeDeviation = DecodeFloat(buffer, ref pointer);
            result.cutDirDeviation = DecodeFloat(buffer, ref pointer);
            result.cutPoint = DecodeVector3(buffer, ref pointer);
            result.cutNormal = DecodeVector3(buffer, ref pointer);
            result.cutDistanceToCenter = DecodeFloat(buffer, ref pointer);
            result.cutAngle = DecodeFloat(buffer, ref pointer);
            result.beforeCutRating = DecodeFloat(buffer, ref pointer);
            result.afterCutRating = DecodeFloat(buffer, ref pointer);
            return result;
        }

        private static EulerTransform DecodeEuler(byte[] buffer, ref int pointer)
        {
            EulerTransform result = new EulerTransform();
            result.position = DecodeVector3(buffer, ref pointer);
            result.rotation = DecodeVector3(buffer, ref pointer);

            return result;
        }

        private static Vector3 DecodeVector3(byte[] buffer, ref int pointer)
        {
            Vector3 result = new Vector3();
            result.x = DecodeFloat(buffer, ref pointer);
            result.y = DecodeFloat(buffer, ref pointer);
            result.z = DecodeFloat(buffer, ref pointer);

            return result;
        }

        private static int DecodeInt(byte[] buffer, ref int pointer)
        {
            int result = BitConverter.ToInt32(buffer, pointer);
            pointer += 4;
            return result;
        }

        private static uint DecodeUInt32(byte[] buffer, ref int pointer)
        {
            uint result = BitConverter.ToUInt32(buffer, pointer);
            pointer += 4;
            return result;
        }

        private static string DecodeString(byte[] buffer, ref int pointer)
        {
            int length = BitConverter.ToInt32(buffer, pointer);
            string @string = Encoding.UTF8.GetString(buffer, pointer + 5, length);
            pointer += length + 5;
            return @string;
        }

        private static int DecodeChar(byte[] buffer, ref int pointer)
        {
            int result = BitConverter.ToInt16(buffer, pointer);
            pointer += 2;
            return result;
        }

        private static float DecodeFloat(byte[] buffer, ref int pointer)
        {
            float result = BitConverter.ToSingle(buffer, pointer);
            pointer += 4;
            return result;
        }

        private static bool DecodeBool(byte[] buffer, ref int pointer)
        {
            bool result = BitConverter.ToBoolean(buffer, pointer);
            pointer++;
            return result;
        }
    }
}
