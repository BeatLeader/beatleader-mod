using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityVector3 = UnityEngine.Vector3;
using UnityQuaternion = UnityEngine.Quaternion;
using UnityEngine;

namespace BeatLeader.Replays
{
    public class ReplayJsonSerializer : MonoBehaviour
    {
        public string Serialize(Replay replay)
        {
            return JsonUtility.ToJson(replay, true);
        }

        public void Start()
        {
            var replay = new Replay();
            File.WriteAllText($@"C:\Users\{Environment.UserName}\Desktop\replay.bsor", Serialize(replay));
        }

        [Serializable]
        public class Replay
        {
            public Replay()
            {
                this.info = new();
                this.frames = new();
                this.notes = new();
                this.walls = new();
                this.heights = new();
                this.pauses = new();
            }

            [Serializable] public class ReplayInfo
            {
                public string version;
                public string gameVersion;
                public string timestamp;

                public string playerID;
                public string playerName;
                public string platform;

                public string trackingSytem;
                public string hmd;
                public string controller;

                public string hash;
                public string songName;
                public string mapper;
                public string difficulty;

                public int score;
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

            [Serializable] public class Frame
            {
                public float time;
                public int fps;
                public Transform head;
                public Transform leftHand;
                public Transform rightHand;
            }
            [Serializable] public class NoteEvent
            {
                public int noteID;
                public float eventTime;
                public float spawnTime;
                public NoteType noteType;
                public NoteCutResult cutResult;
                public NoteCutInfo noteCutInfo;
            }
            [Serializable] public class WallEvent
            {
                public int wallID;
                public float energy;
                public float time;
                public float spawnTime;
            }

            [Serializable] public class PauseInfo
            {
                public float unPauseTime;
                public float pauseTime;
            }
            [Serializable] public class HeightInfo
            {
                public float height;
                public float time;
            }

            [Serializable] public struct Vector3
            {
                public Vector3(UnityVector3 unityVector)
                {
                    x = unityVector.x;
                    y = unityVector.y;
                    z = unityVector.z;
                }

                public static implicit operator UnityVector3(Vector3 vector)
                => new UnityEngine.Vector3(vector.x, vector.y, vector.z);
                public static implicit operator Vector3(UnityVector3 vector)
                => new Vector3(vector);

                public float x;
                public float y;
                public float z;
            }
            [Serializable] public struct Quaternion
            {
                public Quaternion(UnityQuaternion unityQuaternion)
                {
                    x = unityQuaternion.x;
                    y = unityQuaternion.y;
                    z = unityQuaternion.z;
                    w = unityQuaternion.w;
                }

                public static implicit operator UnityQuaternion(Quaternion quaternion)
                => new UnityEngine.Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
                public static implicit operator Quaternion(UnityQuaternion quaternion)
                => new Quaternion(quaternion);

                public float x;
                public float y;
                public float z;
                public float w;
            }
            [Serializable] public class Transform
            {
                public Transform() { }
                public Transform(UnityEngine.Transform transform)
                {
                    this.localPosition = transform.localPosition;
                    this.localRotation = transform.localRotation;
                }
                public Transform(Vector3 position, Quaternion rotation)
                {
                    this.localPosition = position;
                    this.localRotation = rotation;
                }
                public Transform(UnityVector3 position, UnityQuaternion rotation)
                {
                    this.localPosition = position;
                    this.localRotation = rotation;
                }

                public static implicit operator Transform(UnityEngine.Transform transform)
                => new Transform(transform);

                public Vector3 localPosition;
                public Quaternion localRotation;
            }

            [Serializable] public struct NoteCutInfo
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
            }
            public enum NoteType
            {
                Note,
                Slider,
                BurstSlider,
                Bomb
            }
            public enum NoteCutResult
            {
                Good,
                Bad,
                Miss
            }

            public ReplayInfo info;

            public List<Frame> frames;
            public List<NoteEvent> notes;
            public List<WallEvent> walls;
            public List<HeightInfo> heights;
            public List<PauseInfo> pauses;
        }
    }
}
