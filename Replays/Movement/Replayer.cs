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
            PlayFrame(_replay.GetFrameByTime(_songSyncController.songTime));
        }
        private void PlayFrame(Frame frame)
        {
            if (frame == null | frame == previousFrame) return;

            leftHand.SetTransform(frame.leftHand);
            rightHand.SetTransform(frame.rightHand);
            head.SetTransform(frame.head);

            previousFrame = frame;
        }
        private float ComputeLerp(Frame current, Frame next)
        {
            return (float)((_songSyncController.songTime - current.time) / Mathf.Max((float)1E-06, next.time - current.time));
        }
    }
}
