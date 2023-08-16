using System;
using System.Collections.Generic;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Models;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    [UsedImplicitly]
    public class ReplayEventsProcessor : IInitializable, ILateTickable, IDisposable {
        [Inject] private readonly IBeatmapTimeController _beatmapTimeController = null!;
        [Inject] private readonly IVirtualPlayersManager _virtualPlayersManager = null!;

        public bool IsReprocessingEventsNow { get; private set; }
        public bool TimeWasSmallerThanActualTime { get; private set; }

        public event Action<NoteEvent>? NoteProcessRequestedEvent;
        public event Action<WallEvent>? WallProcessRequestedEvent;
        public event Action? ReprocessRequestedEvent;
        public event Action? ReprocessDoneEvent;

        private IReadOnlyList<NoteEvent> _notes = null!;
        private IReadOnlyList<WallEvent> _walls = null!;
        private int _nextNoteIndex;
        private int _nextWallIndex;
        private float _lastTime;
        private bool _allowProcess;

        public void Initialize() {
            _beatmapTimeController.SongWasRewoundEvent += HandleSongWasRewound;
            _virtualPlayersManager.PriorityPlayerWasChangedEvent += HandlePriorityPlayerChanged;
            HandlePriorityPlayerChanged(_virtualPlayersManager.PriorityPlayer!);
        }
        public void Dispose() {
            _beatmapTimeController.SongWasRewoundEvent -= HandleSongWasRewound;
            _virtualPlayersManager.PriorityPlayerWasChangedEvent -= HandlePriorityPlayerChanged;
        }
        public void LateTick() {
            if (!_allowProcess) return;
            var songTime = _beatmapTimeController.SongTime;

            do {
                var hasNextNote = _nextNoteIndex < _notes.Count;
                var hasNextWall = _nextWallIndex < _walls.Count;
                if (!hasNextNote && !hasNextWall) break;

                var nextNote = hasNextNote ? _notes[_nextNoteIndex] : default;
                var nextWall = hasNextWall ? _walls[_nextWallIndex] : default;

                var nextNoteTime = hasNextNote ? nextNote.CutTime : float.MaxValue;
                var nextWallTime = hasNextWall ? nextWall.time : float.MaxValue;

                if (hasNextWall && nextWallTime <= nextNoteTime) {
                    if (nextWall!.time > songTime) break;
                    try {
                        WallProcessRequestedEvent?.Invoke(nextWall);
                    } finally {
                        _nextWallIndex += 1;
                    }
                } else {
                    if (nextNote!.eventTime > songTime) break;
                    try {
                        NoteProcessRequestedEvent?.Invoke(nextNote);
                    } finally {
                        _nextNoteIndex += 1;
                    }
                }
            } while (true);

            _lastTime = songTime;
            if (IsReprocessingEventsNow) {
                IsReprocessingEventsNow = false;
                TimeWasSmallerThanActualTime = false;
                ReprocessDoneEvent?.Invoke();
            }
        }

        private void HandlePriorityPlayerChanged(VirtualPlayer player) {
            if (player.Replay is null) {
                _allowProcess = false;
                return;
            }
            _notes = player.Replay.NoteEvents;
            _walls = player.Replay.WallEvents;
            _nextNoteIndex = 0;
            _nextWallIndex = 0;
            _allowProcess = true;
        }
        
        private void HandleSongWasRewound(float newTime) {
            if (newTime < _lastTime) {
                _nextNoteIndex = 0;
                _nextWallIndex = 0;
                _lastTime = 0f;
                TimeWasSmallerThanActualTime = true;
            }

            ReprocessRequestedEvent?.Invoke();
            IsReprocessingEventsNow = true;
        }
    }
}