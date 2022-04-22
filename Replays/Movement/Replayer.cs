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
        private void PlayFrame(Frame frame, Frame nextFrame)
        {
            if (frame != null && frame != previousFrame)
            {
                if (_lerpEnabled && nextFrame != null && frame != null)
                {
                    float slerp = ComputeLerp(frame, nextFrame);
                    leftHand.SetTransform(
                        Vector3.Lerp(frame.leftHand.position, frame.leftHand.position, slerp),
                        Quaternion.Lerp(frame.leftHand.rotation, frame.leftHand.rotation, slerp));
                    rightHand.SetTransform(
                        Vector3.Lerp(frame.rightHand.position, frame.rightHand.position, slerp),
                        Quaternion.Lerp(frame.rightHand.rotation, frame.rightHand.rotation, slerp));
                    head.SetTransform(
                        Vector3.Lerp(frame.head.position, frame.head.position, slerp),
                        Quaternion.Lerp(frame.head.rotation, frame.head.rotation, slerp));
                }
                else
                {
                    leftHand.SetTransform(frame.leftHand);
                    rightHand.SetTransform(frame.rightHand);
                    head.SetTransform(frame.head);
                }
            }
        }
        private float ComputeLerp(Frame current, Frame next)
        {
            return (float)((_songSyncController.songTime - current.time) / Mathf.Max((float)1E-06, next.time - current.time));
        }
    }
}
