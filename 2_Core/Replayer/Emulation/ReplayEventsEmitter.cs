using System;
using System.Collections.Generic;
using BeatLeader.Models;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.Replayer {
    [UsedImplicitly]
    internal class ReplayEventsEmitter : IInitializable, IDisposable, ILateTickable {
        #region Events

        public event Action<NoteEvent> NoteEventAction;
        public event Action<WallEvent> WallEventAction;
        public event Action FullResetAction;

        #endregion

        #region Inject / Initialize / Dispose

        private readonly AudioTimeSyncController _audioTimeSyncController;
        private readonly BeatmapTimeController _beatmapTimeController;
        private readonly IReadOnlyList<NoteEvent> _notes;
        private readonly IReadOnlyList<WallEvent> _walls;

        public ReplayEventsEmitter(
            AudioTimeSyncController audioTimeSyncController,
            BeatmapTimeController beatmapTimeController,
            ReplayLaunchData launchData
        ) {
            _audioTimeSyncController = audioTimeSyncController;
            _beatmapTimeController = beatmapTimeController;
            _notes = launchData.replay.notes;
            _walls = launchData.replay.walls;
        }

        public void Initialize() {
            _beatmapTimeController.OnSongRewind += OnRewind;
        }


        public void Dispose() {
            _beatmapTimeController.OnSongRewind -= OnRewind;
        }

        #endregion

        #region Tick

        public static bool IsRewinding;
        private int _nextNoteIndex;
        private int _nextWallIndex;
        private float _lastTime;

        private void OnRewind(float newTime) {
            IsRewinding = true;
            if (newTime > _lastTime) return;
            _nextNoteIndex = 0;
            _nextWallIndex = 0;
            _lastTime = 0.0f;
            FullResetAction?.Invoke();
        }

        public void LateTick() {
            var songTime = _audioTimeSyncController.songTime;

            do {
                var hasNextNote = _nextNoteIndex < _notes.Count;
                var hasNextWall = _nextWallIndex < _walls.Count;
                if (!hasNextNote && !hasNextWall) break;

                var nextNote = hasNextNote ? _notes[_nextNoteIndex] : null;
                var nextWall = hasNextWall ? _walls[_nextWallIndex] : null;

                var nextNoteTime = hasNextNote ? nextNote.eventTime : float.MaxValue;
                var nextWallTime = hasNextWall ? nextWall.time : float.MaxValue;

                if (hasNextWall && nextWallTime <= nextNoteTime) {
                    if (nextWall!.time > songTime) break;
                    try {
                        WallEventAction?.Invoke(nextWall);
                    } finally {
                        _nextWallIndex += 1;
                    }
                } else {
                    if (nextNote!.eventTime > songTime) break;
                    try {
                        NoteEventAction?.Invoke(nextNote);
                    } finally {
                        _nextNoteIndex += 1;
                    }
                }
            } while (true);

            _lastTime = songTime;
            IsRewinding = false;
        }

        #endregion
    }
}