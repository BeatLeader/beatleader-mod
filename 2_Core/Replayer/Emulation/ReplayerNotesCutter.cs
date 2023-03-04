using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    public class ReplayerNotesCutter : MonoBehaviour {
        #region Injection

        [Inject] protected readonly BeatmapObjectManager _beatmapObjectManager = null!;
        [Inject] protected readonly ReplayEventsProcessor _eventsEmitter = null!;

        #endregion

        #region Setup

        private void Awake() {
            _eventsEmitter.NoteProcessRequestedEvent += HandleNoteProcessRequested;

            _beatmapObjectManager.noteWasSpawnedEvent += HandleNoteWasSpawned;
            _beatmapObjectManager.noteWasDespawnedEvent += HandleNoteWasDespawned;
        }
        private void OnDestroy() {
            _eventsEmitter.NoteProcessRequestedEvent -= HandleNoteProcessRequested;

            _beatmapObjectManager.noteWasSpawnedEvent -= HandleNoteWasSpawned;
            _beatmapObjectManager.noteWasDespawnedEvent -= HandleNoteWasDespawned;
        }

        #endregion

        #region ProcessNote

        protected void ProcessNote(NoteEvent noteEvent) {
            if (!TryFindSpawnedNote(noteEvent, out var noteController)) {
                if (!_eventsEmitter.IsReprocessingEventsNow)
                    Plugin.Log.Error("[Replayer] Not found NoteController for id " + noteEvent.noteID);
                return;
            }

            if (noteEvent.eventType != NoteEventType.miss) {
                var noteCutInfo = noteEvent.eventType == NoteEventType.bomb ? Models.NoteCutInfo.ConvertToBomb(noteController!) : Models.NoteCutInfo.Convert(noteEvent.noteCutInfo, noteController!);
                var cutEvents = ((LazyCopyHashSet<INoteControllerNoteWasCutEvent>)noteController!.noteWasCutEvent).items;
                cutEvents.ForEach(x => x.HandleNoteControllerNoteWasCut(noteController, noteCutInfo));
            }
        }

        #endregion

        #region NotesHash

        private readonly HashSet<NoteController> _spawnedNotes = new();

        protected bool TryFindSpawnedNote(NoteEvent replayNote, out NoteController? noteController, float timeMargin = 0.2f) {
            var minTimeDifference = float.MaxValue;
            noteController = null;

            foreach (var item in _spawnedNotes) {
                if (!replayNote.IsMatch(item.noteData)) continue;

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

        private void HandleNoteProcessRequested(NoteEvent noteEvent) {
            ProcessNote(noteEvent);
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