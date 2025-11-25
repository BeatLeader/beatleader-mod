using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;

namespace BeatLeader {
    public class EventsProcessor<T> : IEventsProcessor where T : struct {
        public delegate float TimeSelectorDelegate(T item);

        public EventsProcessor(IEnumerable<T> events, IReplayNoteComparator noteComparator, TimeSelectorDelegate timeSelector) {
            _events = new(events);
            _timeSelectorDelegate = timeSelector;
            _noteComparator = noteComparator;
            ResetNode();
        }

        public bool CurrentEventHasTimeMismatch { get; private set; }
        public bool QueueIsBeingAdjusted { get; private set; }

        public event Action<LinkedListNode<T>, IReplayNoteComparator>? EventDequeuedEvent;
        public event Action? EventQueueAdjustStartedEvent;
        public event Action? EventQueueAdjustFinishedEvent;

        private readonly LinkedList<T> _events;
        private readonly TimeSelectorDelegate _timeSelectorDelegate;
        private LinkedListNode<T>? _currentEvent;
        private float _lastTime;
        private IReplayNoteComparator _noteComparator;

        public void Tick(float time) {
            do {
                var nextEvent = _currentEvent?.Value;
                if (nextEvent is null) break;

                var nextEventTime = _timeSelectorDelegate(nextEvent.Value);
                if (nextEventTime > time) break;

                EventDequeuedEvent?.Invoke(_currentEvent!, _noteComparator);
                _currentEvent = _currentEvent!.Next;
            } while (true);
            
            _lastTime = time;
            if (!QueueIsBeingAdjusted) return;
            
            QueueIsBeingAdjusted = false;
            CurrentEventHasTimeMismatch = false;
            EventQueueAdjustFinishedEvent?.Invoke();
        }

        public void AdjustQueue(float newTimeMilestone) {
            if (newTimeMilestone < _lastTime) {
                ResetNode();
                _lastTime = 0f;
                CurrentEventHasTimeMismatch = true;
            }

            QueueIsBeingAdjusted = true;
            EventQueueAdjustStartedEvent?.Invoke();
        }

        private void ResetNode() {
            _currentEvent = _events.First;
        }
    }
}