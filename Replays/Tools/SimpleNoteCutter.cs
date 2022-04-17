using System.Collections.Generic;
using BeatLeader.Utils;
using BeatLeader.Models;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class SimpleNoteCutter : MonoBehaviour
    {
        public NoteController noteController;

        public void Start()
        {
            noteController = GetComponent<NoteController>();
        }
        public void Cut(NoteCutInfo noteCutInfo)
        {
            var subscribers = noteController.GetField<LazyCopyHashSet<INoteControllerNoteWasCutEvent>, NoteController>("_noteWasCutEvent");
            if (subscribers != null)
            {
                foreach (var item in subscribers.items)
                {
                    item.HandleNoteControllerNoteWasCut(noteController, noteCutInfo);
                }
            }
        }
    }
}
