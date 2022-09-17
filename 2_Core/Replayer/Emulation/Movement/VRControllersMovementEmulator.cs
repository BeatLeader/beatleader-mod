using System;
using System.Collections.Generic;
using BeatLeader.Utils;
using BeatLeader.Models;
using UnityEngine;
using Zenject;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

namespace BeatLeader.Replayer.Movement
{
    public class VRControllersMovementEmulator : MonoBehaviour
    {
        [Inject] private readonly AudioTimeSyncController _audioTimeSyncController;
        [Inject] private readonly VRControllersManager _vrControllersManager;
        [Inject] private readonly ReplayLaunchData _replayData;

        protected LinkedList<Frame> _frames;
        protected LinkedListNode<Frame> _lastProcessedNode;
        public bool lerpEnabled = true;

        protected VRController _LeftSaber => _vrControllersManager.LeftSaber;
        protected VRController _RightSaber => _vrControllersManager.RightSaber;
        protected VRController _Head => _vrControllersManager.HeadContainer;
        public bool IsPlaying => _audioTimeSyncController.state.Equals(AudioTimeSyncController.State.Playing);

        protected void Start()
        {
            _frames = new LinkedList<Frame>(_replayData.replay.frames);
            _lastProcessedNode = _frames.First;
        }
        protected virtual void Update()
        {
            if (IsPlaying && _frames.TryGetFrameByTime(_audioTimeSyncController.songTime, out LinkedListNode<Frame> frame))
            {
                PlayFrame(frame.Previous);
            }
        }
        protected virtual void PlayFrame(LinkedListNode<Frame> frame)
        {
            if (frame == null || frame.Next == null) return;
            _lastProcessedNode = frame;

            var leftSaberPos = frame.Value.leftHand.position;
            var leftSaberRot = frame.Value.leftHand.rotation;

            var rightSaberPos = frame.Value.rightHand.position;
            var rightSaberRot = frame.Value.rightHand.rotation;

            var headPos = frame.Value.head.position;
            var headRot = frame.Value.head.rotation;

            if (lerpEnabled)
            {
                float slerp = (_audioTimeSyncController.songTime - frame.Value.time) /
                (frame.Next.Value.time - frame.Value.time);

                leftSaberPos = Vector3.Lerp(frame.Value.leftHand.position, frame.Next.Value.leftHand.position, slerp);
                leftSaberRot = Quaternion.Lerp(frame.Value.leftHand.rotation, frame.Next.Value.leftHand.rotation, slerp);

                rightSaberPos = Vector3.Lerp(frame.Value.rightHand.position, frame.Next.Value.rightHand.position, slerp);
                rightSaberRot = Quaternion.Lerp(frame.Value.rightHand.rotation, frame.Next.Value.rightHand.rotation, slerp);

                headPos = Vector3.Lerp(frame.Value.head.position, frame.Next.Value.head.position, slerp);
                headRot = Quaternion.Lerp(frame.Value.head.rotation, frame.Next.Value.head.rotation, slerp);
            }

            _LeftSaber.transform.localPosition = leftSaberPos;
            _LeftSaber.transform.localRotation = leftSaberRot;

            _RightSaber.transform.localPosition = rightSaberPos;
            _RightSaber.transform.localRotation = rightSaberRot;

            _Head.transform.localPosition = headPos;
            _Head.transform.localRotation = headRot;
        }
    }
}
