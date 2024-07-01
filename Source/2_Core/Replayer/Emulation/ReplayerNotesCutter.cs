using System.Collections.Generic;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    public class ReplayerNotesCutter : MonoBehaviour {
        #region Injection

        [Inject] private readonly BeatmapObjectManager _beatmapObjectManager = null!;
        [Inject] private readonly IReplayBeatmapEventsProcessor _beatmapEventsProcessor = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        #endregion

        #region Setup
        
        private void Awake() {
            _comparator = _launchData.ReplayComparator;
            _beatmapObjectManager.noteWasSpawnedEvent += HandleNoteWasSpawned;
            _beatmapObjectManager.noteWasDespawnedEvent += HandleNoteWasDespawned;
            _beatmapEventsProcessor.NoteEventDequeuedEvent += HandleNoteBeatmapEventDequeued;
        }

        private void OnDestroy() {
            _beatmapObjectManager.noteWasSpawnedEvent -= HandleNoteWasSpawned;
            _beatmapObjectManager.noteWasDespawnedEvent -= HandleNoteWasDespawned;
            _beatmapEventsProcessor.NoteEventDequeuedEvent -= HandleNoteBeatmapEventDequeued;
        }

        #endregion

        #region ProcessNote

        private void ProcessNote(NoteEvent noteEvent) {
            if (!TryFindSpawnedNote(noteEvent, out var noteController)) {
                if (!_beatmapEventsProcessor.QueueIsBeingAdjusted) {
                    Plugin.Log.Error("[Replayer] Not found NoteController for id " + noteEvent.noteId);
                }
                return;
            }
            //no need to cut if event has miss type
            if (noteEvent.eventType == NoteEvent.NoteEventType.Miss) return;
            //invoking the note cut listeners
            var noteCutInfo = noteEvent.noteCutInfo.SaturateNoteCutInfo(noteController!.noteData);
            var cutEvents = ((LazyCopyHashSet<INoteControllerNoteWasCutEvent>)noteController!.noteWasCutEvent).items;
            cutEvents.ForEach(x => x.HandleNoteControllerNoteWasCut(noteController, noteCutInfo));
        }

        #endregion

        #region NotesHash

        private readonly HashSet<NoteController> _spawnedNotes = new();
        private IReplayComparator _comparator = null!;

        private bool TryFindSpawnedNote(NoteEvent replayNote, out NoteController? noteController, float timeMargin = 0.2f) {
            var minTimeDifference = float.MaxValue;
            noteController = null;
            foreach (var item in _spawnedNotes) {
                if (!_comparator.Compare(replayNote, item.noteData)) continue;
                var timeDifference = Mathf.Abs(replayNote.spawnTime - item.noteData.time);
                if (timeDifference > minTimeDifference) continue;
                
                minTimeDifference = timeDifference;
                if (minTimeDifference > timeMargin) continue;
                noteController = item;
            }

            return noteController != null;
        }

        #endregion

        #region Callbacks
        
        private void HandleNoteBeatmapEventDequeued(LinkedListNode<NoteEvent> noteEvent) {
            ProcessNote(noteEvent.Value);
        }

        private void HandleNoteWasSpawned(NoteController noteController) {
            _spawnedNotes.Add(noteController);
        }

        private void HandleNoteWasDespawned(NoteController noteController) {
            _spawnedNotes.Remove(noteController);
        }

        #endregion
    }
}