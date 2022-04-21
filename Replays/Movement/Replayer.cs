using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Zenject;
using IPA.Utilities;
using BeatLeader.Utils;
using BeatLeader.Models;
using BeatLeader.Replays;
using ReplayNoteCutInfo = BeatLeader.Models.NoteCutInfo;
using ReplayVector3 = BeatLeader.Models.Vector3;
using ReplayQuaternion = BeatLeader.Models.Quaternion;
using ReplayTransform = BeatLeader.Models.Transform;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using Transform = UnityEngine.Transform;

namespace BeatLeader.Replays.Movement
{
    public class Replayer : MonoBehaviour
    {
        [Inject] protected readonly IScoreController _controller;
        [Inject] protected readonly AudioTimeSyncController _songSyncController;
        [Inject] protected readonly MovementManager _movementManager;
        [Inject] protected readonly Replay _replay;

        private FakeVRController leftHand => _movementManager.leftHand;
        private FakeVRController rightHand => _movementManager.rightHand;
        private FakeVRController head => _movementManager.head;

        private Frame previousFrame;
        private bool _lerpEnabled;
        private bool _isPlaying;

        public bool isPlaying => _isPlaying;

        public void Start()
        {
            _isPlaying = true;
        }
        public void Update()
        {
            if (!isPlaying) return;

            int i = _replay.GetFrameByTime(_songSyncController.songTime, out Frame frame);
            Frame nextFrame = _replay.frames[_replay.frames.Count > i ? i + 1 : i];
            PlayFrame(frame, true, nextFrame);
        }
        private void PlayFrame(Frame frame, bool lerp = false, Frame nextFrame = null)
        {
            if (frame == null | frame == previousFrame) return;

            if (lerp && nextFrame != null && previousFrame != null)
            {
                float slerp = ComputeLerp(frame, nextFrame);
                leftHand.SetTransform(LerpVector(previousFrame.leftHand.position, frame.leftHand.position, slerp), 
                    LerpQuaternion(previousFrame.leftHand.rotation, frame.leftHand.rotation, slerp));
                rightHand.SetTransform(LerpVector(previousFrame.rightHand.position, frame.rightHand.position, slerp),
                    LerpQuaternion(previousFrame.rightHand.rotation, frame.rightHand.rotation, slerp));
                head.SetTransform(LerpVector(previousFrame.head.position, frame.head.position, slerp),
                    LerpQuaternion(previousFrame.head.rotation, frame.head.rotation, slerp));
            }
            else
            {
                leftHand.SetTransform(frame.leftHand);
                rightHand.SetTransform(frame.rightHand);
                head.SetTransform(frame.head);
            }

            previousFrame = frame;
        }
        private Quaternion LerpQuaternion(Quaternion a, Quaternion b, float c)
        {
            return new Quaternion(Mathf.Lerp(a.x, b.x, c), Mathf.Lerp(a.y, b.y, c), Mathf.Lerp(a.z, b.z, c), Mathf.Lerp(a.w, b.w, c));
        }
        private Vector3 LerpVector(Vector3 a, Vector3 b, float c)
        {
            return new Vector3(Mathf.Lerp(a.x, b.x, c), Mathf.Lerp(a.y, b.y, c), Mathf.Lerp(a.z, b.z, c));
        }
        private float ComputeLerp(Frame current, Frame next)
        {
            return (float)((_songSyncController.songTime - current.time) / Mathf.Max((float)1E-06, next.time - current.time));
        }
    }
}
