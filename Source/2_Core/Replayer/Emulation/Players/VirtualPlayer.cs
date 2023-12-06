using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Utils;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    public class VirtualPlayer : MonoBehaviour {
        public class Pool : MonoMemoryPool<VirtualPlayer> {
            public override void OnSpawned(VirtualPlayer item) {
                item.HandleInstanceSpawned();
            }

            public override void OnDespawned(VirtualPlayer item) {
                item.HandleInstanceDespawned();
            }
        }

        [Inject] private readonly IBeatmapTimeController _beatmapTimeController = null!;

        public IReplay? Replay { get; private set; }
        public IVRControllersProvider? ControllersProvider { get; private set; }

        public bool enableInterpolation = true;

        private LinkedListNode<PlayerMovementFrame>? _lastProcessedNode;
        private LinkedList<PlayerMovementFrame>? _frames;
        private bool _allowPlayback;

        public void Init(IReplay replay, IVRControllersProvider provider) {
            Replay = replay;
            ControllersProvider = provider;
            _frames = new(replay.PlayerMovementFrames);
            _lastProcessedNode = _frames.First;
            _allowPlayback = true;
            gameObject.SetActive(true);
        }

        private void PlayFrame(LinkedListNode<PlayerMovementFrame>? frame) {
            if (frame?.Next == null) return;
            _lastProcessedNode = frame;

            var currentFrame = frame.Value;
            var leftSaberPose = currentFrame.leftHandPose;
            var rightSaberPose = currentFrame.rightHandPose;
            var headPose = currentFrame.headPose;

            if (enableInterpolation) {
                float t = (_beatmapTimeController.SongTime - frame.Value.time) /
                    (frame.Next.Value.time - frame.Value.time);

                var nextFrame = frame.Next.Value;
                leftSaberPose = leftSaberPose.Lerp(nextFrame.leftHandPose, t);
                rightSaberPose = rightSaberPose.Lerp(nextFrame.rightHandPose, t);
                headPose = headPose.Lerp(nextFrame.headPose, t);
            }

            ControllersProvider!.LeftSaber.transform.SetLocalPose(leftSaberPose);
            ControllersProvider.RightSaber.transform.SetLocalPose(rightSaberPose);
            ControllersProvider.Head.transform.SetLocalPose(headPose);
        }

        private void Update() {
            if (_allowPlayback && TryGetFrameByTime(_lastProcessedNode!,
                _beatmapTimeController.SongTime, out var frame)) {
                PlayFrame(frame?.Previous);
            }
        }

        private void HandleInstanceSpawned() {
            _beatmapTimeController.SongWasRewoundEvent += HandleSongWasRewound;
        }

        private void HandleInstanceDespawned() {
            gameObject.SetActive(false);
            _allowPlayback = false;
            _beatmapTimeController.SongWasRewoundEvent -= HandleSongWasRewound;
            ControllersProvider = null;
            _lastProcessedNode = null;
            _frames = null;
        }

        private void HandleSongWasRewound(float time) {
            _lastProcessedNode = _frames!.First;
        }
        
        private static bool TryGetFrameByTime(LinkedListNode<PlayerMovementFrame> entryPoint,
            float time, out LinkedListNode<PlayerMovementFrame>? frame) {
            for (frame = entryPoint; frame != null; frame = frame.Next) {
                if (frame.Value.time >= time) return true;
            }
            frame = null;
            return false;
        }
    }
}
