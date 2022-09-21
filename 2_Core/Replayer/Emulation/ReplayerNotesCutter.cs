using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer
{
    public class ReplayerNotesCutter : MonoBehaviour
    {
        [Inject] private readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] private readonly ReplayEventsProcessor _eventsEmitter;

        protected virtual void Awake()
        {
            _eventsEmitter.OnNoteCutRequested += NotifyNoteCutRequested;
            _beatmapObjectManager.noteWasSpawnedEvent += NotifyNoteWasSpawned;
            _beatmapObjectManager.noteWasDespawnedEvent += NotifyNoteWasDespawned;
        }
        protected virtual void OnDestroy()
        {
            _beatmapObjectManager.noteWasSpawnedEvent -= NotifyNoteWasSpawned;
            _beatmapObjectManager.noteWasDespawnedEvent -= NotifyNoteWasDespawned;
        }
        protected virtual void NotifyNoteCutRequested(NoteEvent noteEvent)
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

        private void NotifyNoteWasSpawned(NoteController noteController)
        {
            _spawnedNotes[noteController] = noteController.noteData.ComputeNoteID();
        }
        private void NotifyNoteWasDespawned(NoteController noteController)
        {
            if (!_spawnedNotes.ContainsKey(noteController)) return;
            _spawnedNotes.Remove(noteController);
        }
        private NoteController FindSpawnedNoteOrNull(NoteEvent replayNote, float timeMargin = 0.2f)
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