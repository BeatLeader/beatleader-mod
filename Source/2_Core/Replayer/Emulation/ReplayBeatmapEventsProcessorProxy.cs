using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    /// <summary>
    /// The ReplayBeatmapEventsProcessor proxy class. Useful for things that don't need to take additional info from players.
    /// </summary>
    internal class ReplayBeatmapEventsProcessorProxy : IInitializable, IDisposable, IReplayBeatmapEventsProcessor {
        #region Injection

        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;

        #endregion

        #region Proxy Impl

        public bool CurrentEventHasTimeMismatch {
            get {
                //Zenject calls Initialize too lately, so we forced to do such shenanigans if we want to avoid Behaviour usage
                Initialize();
                return _beatmapEventsProcessor!.CurrentEventHasTimeMismatch;
            }
        }

        public bool QueueIsBeingAdjusted {
            get {
                Initialize();
                return _beatmapEventsProcessor!.QueueIsBeingAdjusted;
            }
        }

        public event Action<LinkedListNode<NoteEvent>, IReplayNoteComparator>? NoteEventDequeuedEvent;
        public event Action<LinkedListNode<WallEvent>>? WallEventDequeuedEvent;
        public event Action? EventQueueAdjustStartedEvent;
        public event Action? EventQueueAdjustFinishedEvent;
        
        #endregion

        #region Setup

        private IReplayBeatmapEventsProcessor? _beatmapEventsProcessor;
        private bool _isInitialized;
        
        public void Initialize() {
            if (_isInitialized) return;
            _playersManager.PrimaryPlayerWasChangedEvent += HandlePrimaryPlayerChanged;
            HandlePrimaryPlayerChanged(_playersManager.PrimaryPlayer);
            _isInitialized = true;
        }

        public void Dispose() {
            _playersManager.PrimaryPlayerWasChangedEvent -= HandlePrimaryPlayerChanged;
            if (_beatmapEventsProcessor is null) return;
            _beatmapEventsProcessor.NoteEventDequeuedEvent -= HandleNoteEventDequeued;
            _beatmapEventsProcessor.WallEventDequeuedEvent -= HandleWallEventDequeued;
            _beatmapEventsProcessor.EventQueueAdjustStartedEvent -= HandleEventQueueAdjustStartedEvent;
            _beatmapEventsProcessor.EventQueueAdjustFinishedEvent -= HandleEventQueueAdjustFinishedEvent;
        }

        #endregion

        #region Callbacks

        private void HandleNoteEventDequeued(LinkedListNode<NoteEvent> node, IReplayNoteComparator noteComparator) {
            NoteEventDequeuedEvent?.Invoke(node, noteComparator);
        }
        
        private void HandleWallEventDequeued(LinkedListNode<WallEvent> node) {
            WallEventDequeuedEvent?.Invoke(node);
        }

        private void HandleEventQueueAdjustStartedEvent() {
            EventQueueAdjustStartedEvent?.Invoke();
        }
        
        private void HandleEventQueueAdjustFinishedEvent() {
            EventQueueAdjustFinishedEvent?.Invoke();
        }
        
        private void HandlePrimaryPlayerChanged(IVirtualPlayer player) {
            if (_beatmapEventsProcessor is not null) {
                _beatmapEventsProcessor.NoteEventDequeuedEvent -= HandleNoteEventDequeued;
                _beatmapEventsProcessor.WallEventDequeuedEvent -= HandleWallEventDequeued;
                _beatmapEventsProcessor.EventQueueAdjustStartedEvent -= HandleEventQueueAdjustStartedEvent;
                _beatmapEventsProcessor.EventQueueAdjustFinishedEvent -= HandleEventQueueAdjustFinishedEvent;
            }
            _beatmapEventsProcessor = player.ReplayBeatmapEventsProcessor;
            _beatmapEventsProcessor.NoteEventDequeuedEvent += HandleNoteEventDequeued;
            _beatmapEventsProcessor.WallEventDequeuedEvent += HandleWallEventDequeued;
            _beatmapEventsProcessor.EventQueueAdjustStartedEvent += HandleEventQueueAdjustStartedEvent;
            _beatmapEventsProcessor.EventQueueAdjustFinishedEvent += HandleEventQueueAdjustFinishedEvent;
        }
        
        #endregion
    }
}