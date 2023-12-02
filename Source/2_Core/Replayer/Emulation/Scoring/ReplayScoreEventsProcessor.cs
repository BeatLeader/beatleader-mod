using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;
using static NoteData;
using static ScoreMultiplierCounter;
using static BeatLeader.Utils.AbstractReplayUtils;

namespace BeatLeader.Replayer.Emulation {
    internal class ReplayScoreEventsProcessor : TickablePoolItem, IReplayScoreEventsProcessor {
        #region Pool

        public class Pool : MemoryPool<ReplayScoreEventsProcessor> {
            protected override void OnSpawned(ReplayScoreEventsProcessor item) => item.HandleInstanceSpawned();

            protected override void OnDespawned(ReplayScoreEventsProcessor item) => item.HandleInstanceDespawned();
        }

        private void HandleInstanceSpawned() {
            _beatmapTimeController.SongWasRewoundEvent += HandleSongWasRewound;
            InitializeTickable();
        }

        private void HandleInstanceDespawned() {
            DisposeTickable();
            _beatmapTimeController.SongWasRewoundEvent -= HandleSongWasRewound;
            if (_eventsProcessor is not null) {
                _eventsProcessor.EventDequeuedEvent -= HandleScoreEventDequeued;
                _eventsProcessor.EventQueueAdjustStartedEvent -= HandleQueueAdjustStarted;
            }
            _eventsProcessor = null;
        }

        #endregion

        #region Injection

        [Inject] private readonly IBeatmapTimeController _beatmapTimeController = null!;
        [Inject] private readonly IReplayBeatmapData _replayBeatmapData = null!;

        #endregion

        #region ReplayScoreEventsProcessor

        public bool CurrentEventHasTimeMismatch => _eventsProcessor?.CurrentEventHasTimeMismatch ?? false;
        public bool QueueIsBeingAdjusted => _eventsProcessor?.QueueIsBeingAdjusted ?? false;
        public LinkedListNode<ScoreEvent> CurrentScoreEvent { get; private set; } = null!;

        public event Action<LinkedListNode<ScoreEvent>>? ScoreEventDequeuedEvent;
        public event Action? EventQueueAdjustStartedEvent;

        #endregion

        #region Setup

        private EventsProcessor<ScoreEvent>? _eventsProcessor;

        public void Init(IReplay replay) {
            var scoreEvents = CalculateScoreEvents(replay);
            _scoreEvents.AddRange(scoreEvents);
            _eventsProcessor = new(scoreEvents, static x => x.time);
            _eventsProcessor.EventDequeuedEvent += HandleScoreEventDequeued;
            _eventsProcessor.EventQueueAdjustStartedEvent += HandleQueueAdjustStarted;
        }

        #endregion

        #region Score Handling

        private readonly ScoreMultiplierCounter _multiplierCounter = new();
        private readonly LinkedList<ScoreEvent> _scoreEvents = new();

        public override void Tick() {
            var time = _beatmapTimeController.SongTime;
            _eventsProcessor!.Tick(time);
        }

        private ScoreEvent[] CalculateScoreEvents(IReplay replay) {
            var noteEvents = replay.NoteEvents;
            var array = new ScoreEvent[noteEvents.Count];

            var startIndex = 0;
            var totalScore = 0;
            for (var index = 0; index < noteEvents.Count; index++) {
                var noteEvent = noteEvents[index];

                startIndex = _replayBeatmapData.FindNoteDataForEvent(noteEvent, startIndex, out var noteData);
                if (noteData is null) continue;

                totalScore += CalculateScoreForNote(noteEvent, noteData.scoringType) * _multiplierCounter.multiplier;
                var multiplierEvent = noteEvent.eventType switch {
                    NoteEvent.NoteEventType.GoodCut => MultiplierEventType.Positive,
                    NoteEvent.NoteEventType.Unknown => MultiplierEventType.Neutral,
                    _ => MultiplierEventType.Negative
                };
                _multiplierCounter.ProcessMultiplierEvent(multiplierEvent);

                var scoreEvent = new ScoreEvent(totalScore, noteEvent.CutTime);
                array[index] = scoreEvent;
            }

            return array;
        }

        #endregion

        #region Callbacks

        private void HandleSongWasRewound(float time) {
            _eventsProcessor?.AdjustQueue(time);
        } 
        
        private void HandleScoreEventDequeued(LinkedListNode<ScoreEvent> node) {
            CurrentScoreEvent = node;
            ScoreEventDequeuedEvent?.Invoke(node);
        }
        
        private void HandleQueueAdjustStarted() {
            EventQueueAdjustStartedEvent?.Invoke();
        }

        #endregion
    }
}