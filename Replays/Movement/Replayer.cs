using BeatLeader.Utils;
using BeatLeader.Models;
using BeatLeader.Replays.Interfaces;
using UnityEngine;
using Zenject;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

namespace BeatLeader.Replays.Movement
{
    public class Replayer : MonoBehaviour, IStateChangeable
    {
        [Inject] protected readonly IScoreController _controller;
        [Inject] protected readonly AudioTimeSyncController _songSyncController;
        [Inject] protected readonly ReplayManualInstaller.InitData _initData;
        [Inject] protected readonly MovementManager _movementManager;
        [Inject] protected readonly Replay _replay;

        protected Frame _currentFrame;
        protected int _currentFrameIndex;
        protected bool _lerpEnabled;
        protected bool _isPlaying;

        private FakeVRController leftHand => _movementManager.leftHand;
        private FakeVRController rightHand => _movementManager.rightHand;
        private FakeVRController head => _movementManager.head;
        private Frame nextFrame => _replay.frames[_replay.frames.Count - 1 > _currentFrameIndex ? _currentFrameIndex + 1 : _currentFrameIndex];
        private Frame previousFrame => _replay.frames[_currentFrameIndex > 0 ? _currentFrameIndex - 1 : _currentFrameIndex];
        public bool isPlaying => _isPlaying;

        public void Start()
        {
            _lerpEnabled = _initData.movementLerp;
            _isPlaying = true;
        }
        public void Update()
        {
            if (isPlaying)
            {
                int index = _replay.GetFrameByTime(_songSyncController.songTime, out _currentFrame);
                _currentFrameIndex = index != 0 ? index : _currentFrameIndex;
                PlayFrame(_currentFrame, nextFrame);
            }
        }
        public void SetEnabled(bool state)
        {
            _isPlaying = state;
        }
        private void PlayFrame(Frame frame, Frame nextFrame)
        {
            if (frame != null && frame != previousFrame)
            {
                if (_lerpEnabled && nextFrame != null && frame != null)
                {
                    float slerp = ComputeLerp(frame.time, nextFrame.time);
                    leftHand.SetTransform(
                        Vector3.Lerp(frame.leftHand.position, nextFrame.leftHand.position, slerp),
                        Quaternion.Lerp(frame.leftHand.rotation, nextFrame.leftHand.rotation, slerp));
                    rightHand.SetTransform(
                        Vector3.Lerp(frame.rightHand.position, nextFrame.rightHand.position, slerp),
                        Quaternion.Lerp(frame.rightHand.rotation, nextFrame.rightHand.rotation, slerp));
                    head.SetTransform(
                        Vector3.Lerp(frame.head.position, nextFrame.head.position, slerp),
                        Quaternion.Lerp(frame.head.rotation, nextFrame.head.rotation, slerp));
                }
                else
                {
                    leftHand.SetTransform(frame.leftHand);
                    rightHand.SetTransform(frame.rightHand);
                    head.SetTransform(frame.head);
                }
            }
        }
        private float ComputeLerp(float currentTime, float nextTime)
        {
            return (float)((_songSyncController.songTime - currentTime) / Mathf.Max((float)1E-06, nextTime - currentTime));
        }
    }
}
