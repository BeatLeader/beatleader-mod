using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Utils;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayer : TickablePoolItem, IVirtualPlayer, IVirtualPlayerMovementProcessor {
        #region Pool

        public class Pool : MemoryPool<VirtualPlayer> {
            public override void OnSpawned(VirtualPlayer item) => item.HandleInstanceSpawned();

            public override void OnDespawned(VirtualPlayer item) => item.HandleInstanceDespawned();
        }

        private void HandleInstanceSpawned() {
            _beatmapTimeController.SongWasRewoundEvent += HandleSongWasRewound;
            InitializeTickable();
        }

        private void HandleInstanceDespawned() {
            DisposeTickable();
            _beatmapTimeController.SongWasRewoundEvent -= HandleSongWasRewound;
            _scoreProcessorsPool.Despawn(_scoreEventsProcessor);
            _eventProcessorsPool.Despawn(_beatmapEventsProcessor);
            _allowPlayback = false;
            _lastProcessedNode = null;
            _frames = null;
        }

        #endregion

        #region Injection

        [Inject] private readonly IBeatmapTimeController _beatmapTimeController = null!;
        [Inject] private readonly ReplayBeatmapEventsProcessor.Pool _eventProcessorsPool = null!;
        [Inject] private readonly ReplayScoreEventsProcessor.Pool _scoreProcessorsPool = null!;

        #endregion

        #region VirtualPlayer

        public IReplay Replay { get; private set; } = null!;
        public IVirtualPlayerMovementProcessor MovementProcessor => this;
        public IVirtualPlayerBody Body => _controllableBody;
        public IReplayScoreEventsProcessor ReplayScoreEventsProcessor => _scoreEventsProcessor;
        public IReplayBeatmapEventsProcessor ReplayBeatmapEventsProcessor => _beatmapEventsProcessor;

        #endregion

        #region Setup

        public bool enableInterpolation = true;

        private LinkedListNode<PlayerMovementFrame>? _lastProcessedNode;
        private LinkedList<PlayerMovementFrame>? _frames;
        private ReplayScoreEventsProcessor _scoreEventsProcessor = null!;
        private ReplayBeatmapEventsProcessor _beatmapEventsProcessor = null!;
        private IControllableVirtualPlayerBody _controllableBody = null!;
        private bool _allowPlayback;

        public void Init(IReplay replay) {
            Replay = replay;
            //processing
            _scoreEventsProcessor = _scoreProcessorsPool.Spawn();
            _scoreEventsProcessor.Init(Replay);
            _beatmapEventsProcessor = _eventProcessorsPool.Spawn();
            _beatmapEventsProcessor.Init(Replay);
            //playback
            _frames = new(replay.PlayerMovementFrames);
            _lastProcessedNode = _frames.First;
            _allowPlayback = true;
        }

        public void LateInit(IControllableVirtualPlayerBody body) {
            _controllableBody = body;
        }

        #endregion

        #region MovementProcessor

        public PlayerMovementFrame CurrentMovementFrame => _lastProcessedNode?.Value ?? new();
        
        private readonly List<IVirtualPlayerPoseReceiver> _poseReceivers = new();

        public void AddListener(IVirtualPlayerPoseReceiver poseReceiver) {
            _poseReceivers.Add(poseReceiver);
        }

        public void RemoveListener(IVirtualPlayerPoseReceiver poseReceiver) {
            _poseReceivers.Remove(poseReceiver);
        }

        private void UpdatePoseReceivers(
            Pose headPose,
            Pose leftHandPose,
            Pose rightHandPose
        ) {
            foreach (var receiver in _poseReceivers) {
                ApplyPose(receiver, headPose, leftHandPose, rightHandPose);
            }
        }

        private static void ApplyPose(
            IVirtualPlayerPoseReceiver poseReceiver,
            Pose headPose,
            Pose leftHandPose,
            Pose rightHandPose
        ) {
            poseReceiver.ApplyPose(headPose, leftHandPose, rightHandPose);
        }

        #endregion

        #region Playback

        public override void Tick() {
            if (!_allowPlayback) return;
            var result = TryGetFrameByTime(
                _lastProcessedNode!,
                _beatmapTimeController.SongTime,
                out var frame
            );
            if (result) PlayFrame(frame?.Previous);
        }

        private void PlayFrame(LinkedListNode<PlayerMovementFrame>? frame) {
            if (frame?.Next == null) return;
            _lastProcessedNode = frame;

            var currentFrame = frame.Value;
            var leftSaberPose = currentFrame.leftHandPose;
            var rightSaberPose = currentFrame.rightHandPose;
            var headPose = currentFrame.headPose;

            if (enableInterpolation) {
                var currentTime = frame.Value.time;
                var nextTime = frame.Next.Value.time;
                var songTime = _beatmapTimeController.SongTime;
                var t = (songTime - currentTime) / (nextTime - currentTime);

                var nextFrame = frame.Next.Value;
                headPose = headPose.Lerp(nextFrame.headPose, t);
                leftSaberPose = leftSaberPose.Lerp(nextFrame.leftHandPose, t);
                rightSaberPose = rightSaberPose.Lerp(nextFrame.rightHandPose, t);
            }

            ApplyPose(_controllableBody, headPose, leftSaberPose, rightSaberPose);
            UpdatePoseReceivers(headPose, leftSaberPose, rightSaberPose);
        }

        private static bool TryGetFrameByTime(
            LinkedListNode<PlayerMovementFrame> entryPoint,
            float time, out LinkedListNode<PlayerMovementFrame>? frame
        ) {
            for (frame = entryPoint; frame != null; frame = frame.Next) {
                if (frame.Value.time >= time) return true;
            }
            frame = null;
            return false;
        }

        #endregion

        #region Callbacks

        private void HandleSongWasRewound(float time) {
            _lastProcessedNode = _frames!.First;
        }

        #endregion
    }
}