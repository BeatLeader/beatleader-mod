﻿using System;
using System.Collections.Generic;
using BeatLeader.Utils;
using BeatLeader.Models;
using UnityEngine;
using Zenject;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

namespace BeatLeader.Replays.Movement
{
    public class Replayer : MonoBehaviour
    {
        [Inject] protected readonly AudioTimeSyncController _audioTimeSyncController;
        [Inject] protected readonly ReplayerManualInstaller.InitData _initData;
        [Inject] protected readonly VRControllersManager _bodyManager;
        [Inject] protected readonly Replay _replay;

        protected LinkedList<Frame> _frames;
        protected LinkedListNode<Frame> _lastProcessedNode;
        protected bool _lerpEnabled;
        protected bool _isPlaying;

        private VRController leftSaber => _bodyManager.leftSaber;
        private VRController rightSaber => _bodyManager.rightSaber;
        private VRController head => _bodyManager.head;
        public bool isPlaying => _isPlaying;

        public void Start()
        {
            _frames = new LinkedList<Frame>(_replay.frames);
            _lastProcessedNode = _frames.First;
            _lerpEnabled = _initData.movementLerp;
            _isPlaying = true;
        }
        public void Update()
        {
            if (isPlaying && _frames.TryGetFrameByTime(_audioTimeSyncController.songTime, out LinkedListNode<Frame> frame))
            {
                PlayFrame(frame);
            }
        }
        public void PlayFrame(LinkedListNode<Frame> frame)
        {
            if (frame == null && frame == frame.Previous) return;

            if (_lerpEnabled && frame.Next != null && frame != null)
            {
                float slerp = (float)((_audioTimeSyncController.songTime - frame.Value.time) / (frame.Next.Value.time - frame.Value.time));
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