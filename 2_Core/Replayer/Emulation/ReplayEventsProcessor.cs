using System;
using System.Collections.Generic;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer
{
    public class ReplayEventsProcessor : MonoBehaviour
    {
        [Inject] private readonly AudioTimeSyncController _audioTimeSyncController;
        [Inject] private readonly BeatmapTimeController _beatmapTimeController;
        [Inject] private readonly ReplayLaunchData _launchData;

        public bool IsReprocessingEventsNow => _isRewinding;

        public event Action<NoteEvent> OnNoteCutRequested;
        public event Action<WallEvent> OnWallInteractionRequested;
        public event Action OnReprocessRequested;
        public event Action OnReprocessDone;

        private IReadOnlyList<NoteEvent> _notes;
        private IReadOnlyList<WallEvent> _walls;
        private bool _isRewinding;
        private int _nextNoteIndex;
        private int _nextWallIndex;
        private float _lastTime;

        protected virtual void Awake()
        {
            _beatmapTimeController.OnSongRewind += NotifySongWasRewinded;

            _notes = _launchData.replay.notes;
            _walls = _launchData.replay.walls;
        }
        protected virtual void OnDestroy()
        {
            _beatmapTimeController.OnSongRewind -= NotifySongWasRewinded;
        }
        protected virtual void LateUpdate()
        {
            var songTime = _audioTimeSyncController.songTime;

            do
            {
                var hasNextNote = _nextNoteIndex < _notes.Count;
                var hasNextWall = _nextWallIndex < _walls.Count;
                if (!hasNextNote && !hasNextWall) break;

                var nextNote = hasNextNote ? _notes[_nextNoteIndex] : null;
                var nextWall = hasNextWall ? _walls[_nextWallIndex] : null;

                var nextNoteTime = hasNextNote ? nextNote.eventTime : float.MaxValue;
                var nextWallTime = hasNextWall ? nextWall.time : float.MaxValue;

                if (hasNextWall && nextWallTime <= nextNoteTime)
                {
                    if (nextWall!.time > songTime) break;
                    try
                    {
                        OnWallInteractionRequested?.Invoke(nextWall);
                    }
                    finally
                    {
                        _nextWallIndex += 1;
                    }
                }
                else
                {
                    if (nextNote!.eventTime > songTime) break;
                    try
                    {
                        OnNoteCutRequested?.Invoke(nextNote);
                    }
                    finally
                    {
                        _nextNoteIndex += 1;
                    }
                }
            } while (true);

            _lastTime = songTime;
            if (_isRewinding)
            {
                _isRewinding = false;
                OnReprocessDone?.Invoke();
            }
        }

        private void NotifySongWasRewinded(float newTime)
        {
            _isRewinding = true;
            if (newTime > _lastTime) return;
            _nextNoteIndex = 0;
            _nextWallIndex = 0;
            _lastTime = 0f;
            OnReprocessRequested?.Invoke();
        }
    }
}