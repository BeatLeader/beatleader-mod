using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Replays.Models.Quest
{
    struct Vector3
    {
        public float x;
        public float y;
        public float z;
    }

    enum StructType
    {
        head = 0,
        left = 1,
        right = 2,
        noteCut = 3,
        noteMiss = 4,
        wall = 5
    }

    struct EulerTransform
    {
        public Vector3 position;
        public Vector3 rotation;
    }

    struct Frame
    {
        public float time;
        public EulerTransform transform;
    };

    struct SimpleNoteCutInfo
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
    };

    struct DifferentiatingNoteData
    {
        float time;
        int lineIndex;
        int noteLineLayer;
        int colorType;
        int noteCutDirection;
    };

    struct SwingRating
    {
        public float beforeCutRating;
        public float afterCutRating;
    };

    struct NoteCutEvent
    {
        public int noteHash;
        public float time;
        public SimpleNoteCutInfo noteCutInfo;
        public SwingRating swingRating;
    };

    struct NoteMissEvent
    {
        public int noteHash;
        public float time;
    };

    struct WallEvent
    {
        public float time;
        public float energy;
    };

    class QuestReplay
    {
        public List<Frame> head;
        public List<Frame> left;
        public List<Frame> right;

        public List<NoteCutEvent> cuts;
        public List<NoteMissEvent> misses;
        public List<WallEvent> walls;
    }

    static class QuestReplayDecoder
    {
        static QuestReplay Decode(byte[] buffer)
        {
            int arrayLength = (int)buffer.Length;

            int pointer = 0;

            int magic = DecodeInt(buffer, ref pointer);
            byte version = buffer[pointer++];

            if (magic == 0x443d3d38 && version == 0)
            {
                QuestReplay replay = new QuestReplay();

                uint jsonLength = DecodeUInt32(buffer, ref pointer);
                string json = Encoding.UTF8.GetString(buffer, pointer, (int)jsonLength);

                pointer += (int)jsonLength;

                for (int a = 0; a < ((int)StructType.wall) + 1 && pointer < arrayLength; a++)
                {
                    StructType type = (StructType)buffer[pointer++];

                    switch (type)
                    {
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
                    }
                }

                return replay;
            } else
            {
                return null;
            }
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
                miss.time = DecodeFloat(buffer, ref pointer);
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
                wall.time = DecodeFloat(buffer, ref pointer);
                wall.energy = DecodeFloat(buffer, ref pointer);
                result.Add(wall);
            }
            return result;
        }

        private static NoteCutEvent DecodeNote(byte[] buffer, ref int pointer)
        {
            NoteCutEvent result = new NoteCutEvent();
            result.noteHash = DecodeInt(buffer, ref pointer);
            result.time = DecodeFloat(buffer, ref pointer);
            result.noteCutInfo = DecodeCutInfo(buffer, ref pointer);

            SwingRating swingRating = new SwingRating();
            swingRating.beforeCutRating = DecodeFloat(buffer, ref pointer);
            swingRating.afterCutRating = DecodeFloat(buffer, ref pointer);
            result.swingRating = swingRating;

            return result;
        }

        private static SimpleNoteCutInfo DecodeCutInfo(byte[] buffer, ref int pointer)
        {
            SimpleNoteCutInfo result = new SimpleNoteCutInfo();
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
