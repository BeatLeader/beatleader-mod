using BeatLeader.Models;
using BeatLeader.Utils;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    public class VirtualPlayer : MonoBehaviour {
        public class Pool : MonoMemoryPool<VirtualPlayer> {
            protected override void OnSpawned(VirtualPlayer item) {
                item.HandleInstanceSpawned();
            }
            protected override void OnDespawned(VirtualPlayer item) {
                item.HandleInstanceDespawned();
            }
        }

        [Inject] private readonly IBeatmapTimeController _beatmapTimeController = null!;
        [Inject] private readonly IReplayPauseController _pauseController = null!;

        public Player? Player { get; private set; }
        public Replay? Replay { get; private set; }
        public IVRControllersProvider? ControllersProvider { get; private set; }

        public bool enableInterpolation = true;

        private LinkedListNode<Frame>? _lastProcessedNode;
        private LinkedList<Frame>? _frames;
        private bool _allowPlayback;

        public void Init(Player player, Replay replay, IVRControllersProvider provider) {
            Player = player;
            Replay = replay;
            ControllersProvider = provider;
            _frames = new(replay.frames);
            _lastProcessedNode = _frames.First;
            _allowPlayback = true;
            gameObject.SetActive(true);
        }

        private void PlayFrame(LinkedListNode<Frame> frame) {
            if (frame == null || frame.Next == null) return;
            _lastProcessedNode = frame;

            var leftSaberPose = frame.Value.leftHand.GetPose();
            var rightSaberPose = frame.Value.rightHand.GetPose();
            var headPose = frame.Value.head.GetPose();

            if (enableInterpolation) {
                float slerp = (_beatmapTimeController.SongTime - frame.Value.time) /
                (frame.Next.Value.time - frame.Value.time);

                leftSaberPose = leftSaberPose.Lerp(frame.Next.Value.leftHand.GetPose(), slerp);
                rightSaberPose = rightSaberPose.Lerp(frame.Next.Value.rightHand.GetPose(), slerp);
                headPose = headPose.Lerp(frame.Next.Value.head.GetPose(), slerp);
            }

            ControllersProvider!.LeftSaber.transform.SetLocalPose(leftSaberPose);
            ControllersProvider.RightSaber.transform.SetLocalPose(rightSaberPose);
            ControllersProvider.Head.transform.SetLocalPose(headPose);
        }

        private void Update() {
            if (_allowPlayback && _lastProcessedNode!.TryGetFrameByTime(
                _beatmapTimeController.SongTime, out var frame)) {
                PlayFrame(frame.Previous);
            }
        }

        private void HandleInstanceSpawned() {
            _pauseController.PauseStateChangedEvent += HandlePauseStateChanged;
            _beatmapTimeController.SongWasRewoundEvent += HandleSongWasRewound;
        }

        private void HandleInstanceDespawned() {
            gameObject.SetActive(false);
            _allowPlayback = false;
            _pauseController.PauseStateChangedEvent -= HandlePauseStateChanged;
            _beatmapTimeController.SongWasRewoundEvent -= HandleSongWasRewound;
            ControllersProvider = null;
            _lastProcessedNode = null;
            _frames = null;
        }

        private void HandleSongWasRewound(float time) {
            _lastProcessedNode = _frames!.First;
        }

        private void HandlePauseStateChanged(bool state) {
            _allowPlayback = !state;
        }
    }
}
