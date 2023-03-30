using System.Collections.Generic;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Models;
using BeatLeader.Utils;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    public class ReplayerNotesCutter : MonoBehaviour {
        #region Injection

        [Inject, UsedImplicitly] protected readonly BeatmapObjectManager beatmapObjectManager = null!;
        [Inject, UsedImplicitly] protected readonly ReplayEventsProcessor eventsEmitter = null!;
        [Inject, UsedImplicitly] protected readonly ReplayLaunchData launchData = null!;

        #endregion

        #region Setup

        private void Awake() {
            _comparator = launchData.ReplayComparator;
            eventsEmitter.NoteProcessRequestedEvent += HandleNoteProcessRequested;

            beatmapObjectManager.noteWasSpawnedEvent += HandleNoteWasSpawned;
            beatmapObjectManager.noteWasDespawnedEvent += HandleNoteWasDespawned;
        }
        private void OnDestroy() {
            eventsEmitter.NoteProcessRequestedEvent -= HandleNoteProcessRequested;

            beatmapObjectManager.noteWasSpawnedEvent -= HandleNoteWasSpawned;
            beatmapObjectManager.noteWasDespawnedEvent -= HandleNoteWasDespawned;
        }

        #endregion

        #region ProcessNote

        [UsedImplicitly]
        protected void ProcessNote(NoteEvent noteEvent) {
            if (!TryFindSpawnedNote(noteEvent, out var noteController)) {
                if (!eventsEmitter.IsReprocessingEventsNow)
                    Plugin.Log.Error("[Replayer] Not found NoteController for id " + noteEvent.noteId);
                return;
            }

            if (noteEvent.eventType == NoteEvent.NoteEventType.Miss) return;
            
            var noteCutInfo = noteEvent.noteCutInfo.SaturateNoteCutInfo(noteController!.noteData);
            var cutEvents = ((LazyCopyHashSet<INoteControllerNoteWasCutEvent>)noteController!.noteWasCutEvent).items;
            cutEvents.ForEach(x => x.HandleNoteControllerNoteWasCut(noteController, noteCutInfo));
        }

        #endregion

        #region NotesHash

        private readonly HashSet<NoteController> _spawnedNotes = new();
        private IReplayComparator _comparator = null!;

        [UsedImplicitly]
        protected bool TryFindSpawnedNote(NoteEvent replayNote, out NoteController? noteController, float timeMargin = 0.2f) {
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