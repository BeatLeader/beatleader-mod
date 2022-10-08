using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation
{
    public class ReplayerNotesCutter : MonoBehaviour
    {
        #region Injection

        [Inject] protected readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] protected readonly ReplayEventsProcessor _eventsEmitter;

        #endregion

        #region Setup

        private readonly HarmonySilencer _missSilencer = new(typeof(NoteController)
            .GetMethod("HandleNoteDidPassMissedMarkerEvent", ReflectionUtils.DefaultFlags));

        private void Awake()
        {
            _eventsEmitter.NoteProcessRequestedEvent += HandleNoteProcessRequested;

            _beatmapObjectManager.noteWasSpawnedEvent += HandleNoteWasSpawned;
            _beatmapObjectManager.noteWasDespawnedEvent += HandleNoteWasDespawned;
        }
        private void OnDestroy()
        {
            _eventsEmitter.NoteProcessRequestedEvent -= HandleNoteProcessRequested;

            _beatmapObjectManager.noteWasSpawnedEvent -= HandleNoteWasSpawned;
            _beatmapObjectManager.noteWasDespawnedEvent -= HandleNoteWasDespawned;

            _missSilencer.Dispose();
        }

        #endregion

        #region ProcessNote

        protected void ProcessNote(NoteEvent noteEvent)
        {
            if (!TryFindSpawnedNote(noteEvent, out var noteController))
            {
                Plugin.Log.Error("[Replayer] Not found note for id " + noteEvent.noteID);
                return;
            }

            switch (noteEvent.eventType)
            {
                case NoteEventType.miss:
                    var missEvents = ((LazyCopyHashSet<INoteControllerNoteWasMissedEvent>)noteController.noteWasMissedEvent).items;
                    missEvents.ForEach(x => x.HandleNoteControllerNoteWasMissed(noteController));
                    break;

                case NoteEventType.good:
                case NoteEventType.bad:
                case NoteEventType.bomb:
                    var noteCutInfo = Models.NoteCutInfo.Convert(noteEvent.noteCutInfo, noteController);
                    var cutEvents = ((LazyCopyHashSet<INoteControllerNoteWasCutEvent>)noteController.noteWasCutEvent).items;
                    cutEvents.ForEach(x => x.HandleNoteControllerNoteWasCut(noteController, noteCutInfo));
                    break;
            }
        }

        #endregion

        #region NotesHash

        private readonly HashSet<NoteController> _spawnedNotes = new();

        protected bool TryFindSpawnedNote(NoteEvent replayNote, out NoteController noteController, float timeMargin = 0.2f)
        {
            var minTimeDifference = float.MaxValue;
            noteController = null;

            foreach (var item in _spawnedNotes)
            {
                var noteId = item.noteData.ComputeNoteId();

                if (replayNote.noteID != noteId
                    && replayNote.noteID != noteId - 30000) continue;

                var timeDifference = Mathf.Abs(replayNote.spawnTime - item.noteData.time);
                if (timeDifference > minTimeDifference) continue;

                minTimeDifference = timeDifference;
                if (minTimeDifference > timeMargin) continue;

                noteController = item;
            }

            return noteController != null;
        }

        #endregion

        #region Event Handlers

        private void HandleNoteProcessRequested(NoteEvent noteEvent)
        {
            ProcessNote(noteEvent);
        }
        private void HandleNoteWasSpawned(NoteController noteController)
        {
            _spawnedNotes.Add(noteController);
        }
        private void HandleNoteWasDespawned(NoteController noteController)
        {
            _spawnedNotes.Remove(noteController);
        }

        #endregion
    }
}