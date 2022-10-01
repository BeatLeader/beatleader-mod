using System.Collections.Generic;
using BeatLeader.Utils;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Movement
{
    public class VRControllersMovementEmulator : MonoBehaviour
    {
        [Inject] private readonly AudioTimeSyncController _audioTimeSyncController;
        [Inject] private readonly BeatmapTimeController _beatmapTimeController;
        [Inject] private readonly VRControllersProvider _vrControllersManager;
        [Inject] private readonly ReplayLaunchData _replayData;

        protected LinkedList<Frame> _frames;
        protected LinkedListNode<Frame> _lastProcessedNode;
        public bool lerpEnabled = true;

        protected VRController _LeftSaber => _vrControllersManager.LeftSaber;
        protected VRController _RightSaber => _vrControllersManager.RightSaber;
        protected VRController _Head => _vrControllersManager.Head;
        public bool IsPlaying => _audioTimeSyncController.state.Equals(AudioTimeSyncController.State.Playing);

        protected void Start()
        {
            _frames = new LinkedList<Frame>(_replayData.Replay.frames);
            _lastProcessedNode = _frames.First;

            _beatmapTimeController.SongRewindEvent += HandleSongWasRewinded;
        }
        protected void OnDestroy()
        {
            _beatmapTimeController.SongRewindEvent -= HandleSongWasRewinded;
        }

        protected virtual void Update()
        {
            if (IsPlaying && _lastProcessedNode.TryGetFrameByTime(
                _audioTimeSyncController.songTime, out LinkedListNode<Frame> frame))
            {
                PlayFrame(frame.Previous);
            }
        }
        protected virtual void PlayFrame(LinkedListNode<Frame> frame)
        {
            if (frame == null || frame.Next == null) return;
            _lastProcessedNode = frame;

            var leftSaberPose = frame.Value.leftHand.GetPose();
            var rightSaberPose = frame.Value.rightHand.GetPose();
            var headPose = frame.Value.head.GetPose();

            if (lerpEnabled)
            {
                float slerp = (_audioTimeSyncController.songTime - frame.Value.time) /
                (frame.Next.Value.time - frame.Value.time);

                leftSaberPose = leftSaberPose.Lerp(frame.Next.Value.leftHand.GetPose(), slerp);
                rightSaberPose = rightSaberPose.Lerp(frame.Next.Value.rightHand.GetPose(), slerp);
                headPose = headPose.Lerp(frame.Next.Value.head.GetPose(), slerp);
            }

            _LeftSaber.transform.SetLocalPose(leftSaberPose);
            _RightSaber.transform.SetLocalPose(rightSaberPose);
            _Head.transform.SetLocalPose(headPose);
        }

        private void HandleSongWasRewinded(float time)
        {
            _lastProcessedNode = _frames.First;
        }
    }
}
