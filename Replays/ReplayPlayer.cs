using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Zenject;
using BeatLeader.Replays.Models;
using BeatLeader.Replays.Emulators;

namespace BeatLeader.Replays
{
    public class ReplayPlayer : MonoBehaviour
    {
        [Inject] public AudioTimeSyncController songSyncController;
        [Inject] public Replay replayData;

        public ReplayVRController rightHand;
        public ReplayVRController leftHand;
        private Frame previousFrame;
        private int totalFramesCount;
        private int currentFrame;

        public event Action<Frame> frameWasUpdated;

        public bool isPlaying
        {
            get;
            private set;
        }

        public void Start()
        {
            currentFrame = 0;
            totalFramesCount = replayData.frames.Count;
            isPlaying = replayData != null ? true : false;
        }
        public void Update()
        {
            if (!isPlaying) return;
            PlayFrame(replayData.GetFrameByTime(songSyncController.songTime));
            currentFrame++;
        }
        private void PlayFrame(Frame frame)
        {
            if (frame == null | frame == previousFrame) return;

            var LeftHandPos = new UnityEngine.Vector3(frame.leftHand.position.x, frame.leftHand.position.y, frame.leftHand.position.z);
            var RightHandPos = new UnityEngine.Vector3(frame.rightHand.position.x, frame.rightHand.position.y, frame.rightHand.position.z);
            var LeftHandRot = new UnityEngine.Quaternion(frame.leftHand.rotation.x, frame.leftHand.rotation.y, frame.leftHand.rotation.z, frame.leftHand.rotation.w);
            var RightHandRot = new UnityEngine.Quaternion(frame.rightHand.rotation.x, frame.rightHand.rotation.y, frame.rightHand.rotation.z, frame.rightHand.rotation.w);

            leftHand.SetTransform(LeftHandPos, LeftHandRot);
            rightHand.SetTransform(RightHandPos, RightHandRot);

            previousFrame = frame;
            frameWasUpdated?.Invoke(frame);
        }
    }
}
