using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Utils;
using System.Collections.Generic;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayer : TickablePoolItem, IVirtualPlayer {
        #region Pool
        
        public class Pool : MemoryPool<VirtualPlayer> {
            protected override void OnSpawned(VirtualPlayer item) => item.HandleInstanceSpawned();
            
            protected override void OnDespawned(VirtualPlayer item) => item.HandleInstanceDespawned();
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
        public IReplayScoreEventsProcessor ReplayScoreEventsProcessor => _scoreEventsProcessor;
        public IReplayBeatmapEventsProcessor ReplayBeatmapEventsProcessor => _beatmapEventsProcessor;
        public IVRControllersProvider ControllersProvider { get; private set; } = null!;

        #endregion
        
        #region Setup
        
        public bool enableInterpolation = true;

        private LinkedListNode<PlayerMovementFrame>? _lastProcessedNode;
        private LinkedList<PlayerMovementFrame>? _frames;
        private ReplayScoreEventsProcessor _scoreEventsProcessor = null!;
        private ReplayBeatmapEventsProcessor _beatmapEventsProcessor = null!;
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

        public void ApplyControllers(IVRControllersProvider provider) {
            ControllersProvider = provider;
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
                var t = (_beatmapTimeController.SongTime - frame.Value.time) /
                    (frame.Next.Value.time - frame.Value.time);

                var nextFrame = frame.Next.Value;
                leftSaberPose = leftSaberPose.Lerp(nextFrame.leftHandPose, t);
                rightSaberPose = rightSaberPose.Lerp(nextFrame.rightHandPose, t);
                headPose = headPose.Lerp(nextFrame.headPose, t);
            }

            ControllersProvider.LeftSaber.transform.SetLocalPose(leftSaberPose);
            ControllersProvider.RightSaber.transform.SetLocalPose(rightSaberPose);
            ControllersProvider.Head.transform.SetLocalPose(headPose);
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