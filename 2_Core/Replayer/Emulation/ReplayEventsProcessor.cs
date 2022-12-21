using System;
using System.Collections.Generic;
using BeatLeader.Models;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    public class ReplayEventsProcessor : IInitializable, ILateTickable, IDisposable {
        [Inject] private readonly AudioTimeSyncController _audioTimeSyncController = null!;
        [Inject] private readonly IBeatmapTimeController _beatmapTimeController = null!;
        [Inject] private readonly IVirtualPlayersManager _virtualPlayersManager = null!;

        public bool IsReprocessingEventsNow => _isReprocessing;
        public bool TimeWasSmallerThanActualTime => _timeWasSmallerThanActualTime;

        public event Action<NoteEvent>? NoteProcessRequestedEvent;
        public event Action<WallEvent>? WallProcessRequestedEvent;
        public event Action? ReprocessRequestedEvent;
        public event Action? ReprocessDoneEvent;

        private IReadOnlyList<NoteEvent> _notes = null!;
        private IReadOnlyList<WallEvent> _walls = null!;
        private bool _timeWasSmallerThanActualTime;
        private bool _isReprocessing;
        private int _nextNoteIndex;
        private int _nextWallIndex;
        private float _lastTime;

        public void Initialize() {
            _beatmapTimeController.SongWasRewoundEvent += HandleSongWasRewinded;
            _virtualPlayersManager.PriorityPlayerWasChangedEvent += HandlePriorityPlayerChanged;
            HandlePriorityPlayerChanged(_virtualPlayersManager.PriorityPlayer);
        }
        public void Dispose() {
            _beatmapTimeController.SongWasRewoundEvent -= HandleSongWasRewinded;
            _virtualPlayersManager.PriorityPlayerWasChangedEvent -= HandlePriorityPlayerChanged;
        }
        public void LateTick() {
            var songTime = _audioTimeSyncController.songTime;

            do {
                var hasNextNote = _nextNoteIndex < _notes.Count;
                var hasNextWall = _nextWallIndex < _walls.Count;
                if (!hasNextNote && !hasNextWall) break;

                var nextNote = hasNextNote ? _notes[_nextNoteIndex] : null;
                var nextWall = hasNextWall ? _walls[_nextWallIndex] : null;

                var nextNoteTime = hasNextNote ? nextNote!.eventTime : float.MaxValue;
                var nextWallTime = hasNextWall ? nextWall!.time : float.MaxValue;

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
            if (_isReprocessing) {
                _isReprocessing = false;
                _timeWasSmallerThanActualTime = false;
                ReprocessDoneEvent?.Invoke();
            }
        }

        private void HandlePriorityPlayerChanged(VirtualPlayer player) {
            _notes = player.Replay!.notes;
            _walls = player.Replay.walls;
            //_nextNoteIndex = 0;
            //_nextWallIndex = 0;
        }
        private void HandleSongWasRewinded(float newTime) {
            if (newTime < _lastTime) {
                _nextNoteIndex = 0;
                _nextWallIndex = 0;
                _lastTime = 0f;
                _timeWasSmallerThanActualTime = true;
            }

            ReprocessRequestedEvent?.Invoke();
            _isReprocessing = true;
        }
    }
}