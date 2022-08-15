using System;
using System.Collections.Generic;
using BeatLeader.Utils;
using BeatLeader.Models;
using UnityEngine;
using Zenject;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

namespace BeatLeader.Replays.Movement
{
    public class VRControllersMovementEmulator : MonoBehaviour
    {
        [Inject] private readonly AudioTimeSyncController _audioTimeSyncController;
        [Inject] private readonly ReplayerManualInstaller.InitData _initData;
        [Inject] private readonly VRControllersManager _vrControllersManager;
        [Inject] private readonly Replay _replay;

        private LinkedList<Frame> _frames;
        protected LinkedListNode<Frame> _lastProcessedNode;
        protected bool _isPlaying;
        public bool lerpEnabled;

        protected VRController leftSaber => _vrControllersManager.LeftSaber;
        protected VRController rightSaber => _vrControllersManager.RightSaber;
        protected VRController head => _vrControllersManager.Head;
        public bool isPlaying => _isPlaying;

        public void Start()
        {
            _frames = new LinkedList<Frame>(_replay.frames);
            _lastProcessedNode = _frames.First;
            lerpEnabled = _initData.movementLerp;
            _isPlaying = true;
        }
        public void Update()
        {
            if (isPlaying && _frames.TryGetFrameByTime(_audioTimeSyncController.songTime, out LinkedListNode<Frame> frame))
            {
                PlayFrame(frame.Previous);
            }
        }
        public virtual void PlayFrame(LinkedListNode<Frame> frame)
        {
            if (frame == null || frame.Next == null) return;
            if (lerpEnabled)
            {
                float slerp = (_audioTimeSyncController.songTime - frame.Value.time) / 
                    (frame.Next.Value.time - frame.Value.time);
                leftSaber.transform.SetPositionAndRotation(
                    Vector3.Lerp(frame.Value.leftHand.position, frame.Next.Value.leftHand.position, slerp),
                    Quaternion.Lerp(frame.Value.leftHand.rotation, frame.Next.Value.leftHand.rotation, slerp));
                rightSaber.transform.SetPositionAndRotation(
                    Vector3.Lerp(frame.Value.rightHand.position, frame.Next.Value.rightHand.position, slerp),
                    Quaternion.Lerp(frame.Value.rightHand.rotation, frame.Next.Value.rightHand.rotation, slerp));
                head.transform.SetPositionAndRotation(
                    Vector3.Lerp(frame.Value.head.position, frame.Next.Value.head.position, slerp),
                    Quaternion.Lerp(frame.Value.head.rotation, frame.Next.Value.head.rotation, slerp));
            }
            else
            {
                leftSaber.transform.SetPositionAndRotation(frame.Value.leftHand.position, frame.Value.leftHand.rotation);
                rightSaber.transform.SetPositionAndRotation(frame.Value.rightHand.position, frame.Value.rightHand.rotation);
                head.transform.SetPositionAndRotation(frame.Value.head.position, frame.Value.head.rotation);
            }
            _lastProcessedNode = frame;
        }
    }
}
