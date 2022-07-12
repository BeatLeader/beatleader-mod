using System.Collections.Generic;
using BeatLeader.Utils;
using BeatLeader.Models;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Emulating
{
    public class SimpleNoteCutter : MonoBehaviour
    {
        public void Cut(NoteCutInfo noteCutInfo)
        {
            var noteController = GetComponent<NoteController>();
            if (noteController != null)
            {
                foreach (var item in ((LazyCopyHashSet<INoteControllerNoteWasCutEvent>)noteController.noteWasCutEvent).items)
                {
                    item.HandleNoteControllerNoteWasCut(noteController, noteCutInfo);
                }
            }
        }
    }
}
