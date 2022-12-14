using System.Collections.Generic;
using BeatLeader.Utils;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation
{
    public class VRControllersMovementEmulator : MonoBehaviour
    {
        [Inject] private readonly AudioTimeSyncController _audioTimeSyncController;
        [Inject] private readonly IBeatmapTimeController _beatmapTimeController;
        [Inject] private readonly ReplayerControllersManager _vrControllersManager;
        [Inject] private readonly ReplayLaunchData _replayData;

        private LinkedList<Frame> _frames;
        private LinkedListNode<Frame> _lastProcessedNode;
        public bool lerpEnabled = true;

        private VRController LeftSaber => _vrControllersManager.LeftSaber;
        private VRController RightSaber => _vrControllersManager.RightSaber;
        private VRController Head => _vrControllersManager.Head;
        public bool IsPlaying => _audioTimeSyncController.state.Equals(AudioTimeSyncController.State.Playing);

        private void Start()
        {
            _frames = new LinkedList<Frame>(_replayData.Replay.frames);
            _lastProcessedNode = _frames.First;

            _beatmapTimeController.SongRewindEvent += HandleSongWasRewinded;
        }
        private void OnDestroy()
        {
            _beatmapTimeController.SongRewindEvent -= HandleSongWasRewinded;
        }

        private void Update()
        {
            if (IsPlaying && _lastProcessedNode.TryGetFrameByTime(
                _audioTimeSyncController.songTime, out LinkedListNode<Frame> frame))
            {
                PlayFrame(frame.Previous);
            }
        }
        private void PlayFrame(LinkedListNode<Frame> frame)
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

            LeftSaber.transform.SetLocalPose(leftSaberPose);
            RightSaber.transform.SetLocalPose(rightSaberPose);
            Head.transform.SetLocalPose(headPose);
        }

        private void HandleSongWasRewinded(float time)
        {
            _lastProcessedNode = _frames.First;
        }
    }
}
