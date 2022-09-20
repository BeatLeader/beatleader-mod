using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Utils;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    [UsedImplicitly]
    internal class ReplayNotesCutter : IInitializable, IDisposable {
        #region Inject / Initialize / Dispose

        private readonly BeatmapObjectManager _beatmapObjectManager;
        private readonly ReplayEventsEmitter _eventsEmitter;

        public ReplayNotesCutter(
            BeatmapObjectManager beatmapObjectManager,
            ReplayEventsEmitter eventsEmitter
        ) {
            _beatmapObjectManager = beatmapObjectManager;
            _eventsEmitter = eventsEmitter;
        }

        public void Initialize() {
            _eventsEmitter.NoteEventAction += OnNoteEvent;
            _beatmapObjectManager.noteWasSpawnedEvent += OnNoteWasSpawned;
            _beatmapObjectManager.noteWasDespawnedEvent += OnNoteWasDespawned;
        }


        public void Dispose() {
            _beatmapObjectManager.noteWasSpawnedEvent -= OnNoteWasSpawned;
            _beatmapObjectManager.noteWasDespawnedEvent -= OnNoteWasDespawned;
        }

        #endregion

        #region Events

        private void OnNoteEvent(NoteEvent noteEvent) {
            switch (noteEvent.eventType) {
                case NoteEventType.good:
                case NoteEventType.bad:
                case NoteEventType.bomb:
                    TryCutNote(noteEvent);
                    break;
            }
        }

        private void TryCutNote(NoteEvent noteEvent) {
            var noteController = FindSpawnedNoteOrNull(noteEvent);
            if (noteController == null) return;

            var noteCutInfo = Models.NoteCutInfo.ToBaseGame(noteEvent.noteCutInfo, noteController);
            var cutEvents = ((LazyCopyHashSet<INoteControllerNoteWasCutEvent>)noteController.noteWasCutEvent).items;
            foreach (var cutEvent in cutEvents) {
                cutEvent.HandleNoteControllerNoteWasCut(noteController, noteCutInfo);
            }
        }

        #endregion

        #region NotesHash

        private readonly Dictionary<NoteController, int> _spawnedNotes = new(10);

        private void OnNoteWasSpawned(NoteController noteController) {
            _spawnedNotes[noteController] = noteController.noteData.ComputeNoteID();
        }

        private void OnNoteWasDespawned(NoteController noteController) {
            if (!_spawnedNotes.ContainsKey(noteController)) return;
            _spawnedNotes.Remove(noteController);
        }

        [CanBeNull]
        private NoteController FindSpawnedNoteOrNull(NoteEvent replayNote, float timeMargin = 0.2f) {
            var minTimeDifference = float.MaxValue;
            NoteController noteController = null;

            foreach (var pair in _spawnedNotes) {
                if (replayNote.noteID != pair.Value) continue;
                var timeDifference = Mathf.Abs(replayNote.spawnTime - pair.Key.noteData.time);
                if (timeDifference > minTimeDifference) continue;
                minTimeDifference = timeDifference;
                if (minTimeDifference > timeMargin) continue;
                noteController = pair.Key;
            }

            return noteController;
        }

        #endregion
    }
}