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

        public event Action<LinkedListNode<NoteEvent>>? NoteProcessRequestedEvent;
        public event Action<LinkedListNode<WallEvent>>? WallProcessRequestedEvent;
        public event Action? ReprocessRequestedEvent;
        public event Action? ReprocessDoneEvent;
        
        private LinkedList<NoteEvent> _notes = null!;
        private LinkedList<WallEvent> _walls = null!;
        
        private LinkedListNode<NoteEvent>? _lastNoteNode;
        private LinkedListNode<WallEvent>? _lastWallNode;
        
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
                var nextNote = _lastNoteNode?.Value;
                var nextWall = _lastWallNode?.Value;

                var hasNextNote = nextNote is not null;
                var hasNextWall = nextWall is not null;
                if (!hasNextNote && !hasNextWall) break;
                
                var nextNoteTime = hasNextNote ? nextNote!.Value.CutTime : float.MaxValue;
                var nextWallTime = hasNextWall ? nextWall!.Value.time : float.MaxValue;

                if (hasNextWall && nextWallTime <= nextNoteTime) {
                    if (nextWallTime > songTime) break;
                    WallProcessRequestedEvent?.Invoke(_lastWallNode!);
                    _lastWallNode = _lastWallNode!.Next;
                } else {
                    if (nextNoteTime > songTime) break;
                    NoteProcessRequestedEvent?.Invoke(_lastNoteNode!);
                    _lastNoteNode = _lastNoteNode!.Next;
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
            _notes = new(player.Replay.NoteEvents);
            _walls = new(player.Replay.WallEvents);
            _lastNoteNode = _notes.First;
            _lastWallNode = _walls.First;
            _allowProcess = true;
        }

        private void HandleSongWasRewound(float newTime) {
            if (newTime < _lastTime) {
                _lastNoteNode = _notes.First;
                _lastWallNode = _walls.First;
                _lastTime = 0f;
                TimeWasSmallerThanActualTime = true;
            }

            ReprocessRequestedEvent?.Invoke();
            IsReprocessingEventsNow = true;
        }
    }
}