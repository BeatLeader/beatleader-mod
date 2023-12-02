using System;
using System.Collections.Generic;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Models;
using BeatLeader.Utils;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class ReplayBeatmapEventsProcessor : LateTickablePoolItem, IReplayBeatmapEventsProcessor {
        #region Pool

        public class Pool : MemoryPool<ReplayBeatmapEventsProcessor> {
            protected override void OnSpawned(ReplayBeatmapEventsProcessor item) {
                item.HandleInstanceSpawned();
            }

            protected override void OnDespawned(ReplayBeatmapEventsProcessor item) {
                item.HandleInstanceDespawned();
            }
        }

        private void HandleInstanceSpawned() {
            _beatmapTimeController.SongWasRewoundEvent += HandleSongWasRewound;
            InitializeTickable();
        }

        private void HandleInstanceDespawned() {
            _beatmapTimeController.SongWasRewoundEvent -= HandleSongWasRewound;
            DisposeTickable();
            if (!_allowProcess) return;
            _allowProcess = false;
            _noteEventsProcessor!.EventDequeuedEvent -= HandleNoteEventDequeued;
            _noteEventsProcessor.EventQueueAdjustStartedEvent -= HandleQueueAdjustStarted;
            _wallEventsProcessor!.EventDequeuedEvent -= HandleWallEventDequeued;
            _wallEventsProcessor.EventQueueAdjustStartedEvent -= HandleQueueAdjustStarted;
            _noteEventsProcessor = null;
            _wallEventsProcessor = null;
        }

        #endregion

        #region Injection

        [Inject] private readonly IBeatmapTimeController _beatmapTimeController = null!;

        #endregion

        #region ReplayEventsProcessor

        public bool QueueIsBeingAdjusted {
            get {
                var notesQueueIsBeingAdjusted = _noteEventsProcessor?.QueueIsBeingAdjusted ?? false;
                var wallsQueueIsBeingAdjusted = _wallEventsProcessor?.QueueIsBeingAdjusted ?? false;
                return notesQueueIsBeingAdjusted || wallsQueueIsBeingAdjusted;
            }
        }

        public bool CurrentEventHasTimeMismatch {
            get {
                var currentNoteEventHasTimeMismatch = _noteEventsProcessor?.CurrentEventHasTimeMismatch ?? false;
                var currentWallEventHasTimeMismatch = _wallEventsProcessor?.CurrentEventHasTimeMismatch ?? false;
                return currentNoteEventHasTimeMismatch || currentWallEventHasTimeMismatch;
            }
        }

        public event Action<LinkedListNode<NoteEvent>>? NoteEventDequeuedEvent;
        public event Action<LinkedListNode<WallEvent>>? WallEventDequeuedEvent;
        public event Action? EventQueueAdjustStartedEvent;

        #endregion

        #region Setup

        private EventsProcessor<NoteEvent>? _noteEventsProcessor;
        private EventsProcessor<WallEvent>? _wallEventsProcessor;
        private bool _allowProcess;
        private bool _canCallQueueAdjustEvent;

        public void Init(IReplay replay) {
            if (_allowProcess) return;
            _noteEventsProcessor = new(replay.NoteEvents, static x => x.CutTime);
            _wallEventsProcessor = new(replay.WallEvents, static x => x.time);
            _noteEventsProcessor.EventDequeuedEvent += HandleNoteEventDequeued;
            _noteEventsProcessor.EventQueueAdjustStartedEvent += HandleQueueAdjustStarted;
            _wallEventsProcessor.EventDequeuedEvent += HandleWallEventDequeued;
            _wallEventsProcessor.EventQueueAdjustStartedEvent += HandleQueueAdjustStarted;
            _allowProcess = true;
        }

        #endregion

        #region Event Handling

        public override void LateTick() {
            if (!_allowProcess) return;
            var time = _beatmapTimeController.SongTime;
            _noteEventsProcessor!.Tick(time);
            _wallEventsProcessor!.Tick(time);
            _canCallQueueAdjustEvent = true;
        }

        #endregion

        #region Callbacks

        private void HandleSongWasRewound(float newTime) {
            _noteEventsProcessor?.AdjustQueue(newTime);
            _wallEventsProcessor?.AdjustQueue(newTime);
        }

        private void HandleNoteEventDequeued(LinkedListNode<NoteEvent> node) {
            NoteEventDequeuedEvent?.Invoke(node);
        }

        private void HandleWallEventDequeued(LinkedListNode<WallEvent> node) {
            WallEventDequeuedEvent?.Invoke(node);
        }

        private void HandleQueueAdjustStarted() {
            if (!_canCallQueueAdjustEvent) return;
            EventQueueAdjustStartedEvent?.Invoke();
            _canCallQueueAdjustEvent = false;
        }

        #endregion
    }
}