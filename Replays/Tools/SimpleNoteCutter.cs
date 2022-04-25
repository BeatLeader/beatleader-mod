using System.Collections.Generic;
using BeatLeader.Utils;
using BeatLeader.Models;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.MapEmitating
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
            foreach (var item in ((LazyCopyHashSet<INoteControllerNoteWasCutEvent>)noteController.noteWasCutEvent).items)
            {
                item.HandleNoteControllerNoteWasCut(noteController, noteCutInfo);
            }
        }
    }
}
