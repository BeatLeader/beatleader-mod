using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Utils;
using BeatLeader.Models;
using UnityEngine;
using ReplayNoteCutInfo = BeatLeader.Models.NoteCutInfo;

namespace BeatLeader.Replays
{
    public class SimpleNoteCutComparator : MonoBehaviour, INoteControllerNoteWasCutEvent
    {
        public AudioTimeSyncController timeSyncController;
        public SimpleNoteCutter noteCutter;
        public NoteController noteController;
        public NoteEvent noteCutEvent;

        public bool availableForCut;
        public float cutTime => noteCutEvent.eventTime;
        public int noteID => noteCutEvent.noteID;

        public void Start()
        {
            noteController = GetComponent<NoteController>();
            noteCutter = gameObject.AddComponent<SimpleNoteCutter>();
            noteController.noteWasCutEvent.Add(this);
            availableForCut = true;
        }
        public void Update()
        {
            if (timeSyncController != null && timeSyncController.songTime >= cutTime && availableForCut)
            {
                noteCutter.Cut(ReplayNoteCutInfo.Parse(noteCutEvent.noteCutInfo, noteController));
                HandleNoteControllerNoteWasCut();
            }
        }
        public void HandleNoteControllerNoteWasCut()
        {
            noteController.noteWasCutEvent.Remove(this);
            Destroy(this);
        }
        public void HandleNoteControllerNoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            noteController.noteWasCutEvent.Remove(this);
            Destroy(this);
        }
    }
}
