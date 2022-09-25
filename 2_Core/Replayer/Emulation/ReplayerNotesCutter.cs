using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation
{
    public class ReplayerNotesCutter : MonoBehaviour
    {
        [Inject] protected readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] protected readonly ReplayEventsProcessor _eventsEmitter;

        protected virtual void Awake()
        {
            _eventsEmitter.NoteCutRequestedEvent += HandleNoteCutRequested;

            _beatmapObjectManager.noteWasSpawnedEvent += HandleNoteWasSpawned;
            _beatmapObjectManager.noteWasDespawnedEvent += HandleNoteWasDespawned;
        }
        protected virtual void OnDestroy()
        {
            _eventsEmitter.NoteCutRequestedEvent -= HandleNoteCutRequested;

            _beatmapObjectManager.noteWasSpawnedEvent -= HandleNoteWasSpawned;
            _beatmapObjectManager.noteWasDespawnedEvent -= HandleNoteWasDespawned;
        }

        protected void TryCutNote(NoteEvent noteEvent)
        {
            var noteController = FindSpawnedNoteOrNull(noteEvent);
            if (noteController == null) return;

            var noteCutInfo = Models.NoteCutInfo.Convert(noteEvent.noteCutInfo, noteController);
            var cutEvents = ((LazyCopyHashSet<INoteControllerNoteWasCutEvent>)noteController.noteWasCutEvent).items;
            foreach (var cutEvent in cutEvents)
            {
                cutEvent.HandleNoteControllerNoteWasCut(noteController, noteCutInfo);
            }
        }

        #region NotesHash

        private readonly Dictionary<NoteController, int> _spawnedNotes = new(10);

        protected void HandleNoteCutRequested(NoteEvent noteEvent)
        {
            switch (noteEvent.eventType)
            {
                case NoteEventType.good:
                case NoteEventType.bad:
                case NoteEventType.bomb:
                    TryCutNote(noteEvent);
                    break;
            }
        }
        private void HandleNoteWasSpawned(NoteController noteController)
        {
            _spawnedNotes[noteController] = noteController.noteData.ComputeNoteID();
        }
        private void HandleNoteWasDespawned(NoteController noteController)
        {
            if (!_spawnedNotes.ContainsKey(noteController)) return;
            _spawnedNotes.Remove(noteController);
        }
        protected NoteController FindSpawnedNoteOrNull(NoteEvent replayNote, float timeMargin = 0.2f)
        {
            var minTimeDifference = float.MaxValue;
            NoteController noteController = null;

            foreach (var pair in _spawnedNotes)
            {
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