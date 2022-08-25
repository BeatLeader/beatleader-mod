using ReplayNoteCutInfo = BeatLeader.Models.NoteCutInfo;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulating
{
    public class SimpleNoteCutComparator : MonoBehaviour, INoteControllerNoteWasCutEvent
    {
        public class Pool : MonoMemoryPool<NoteController, NoteEvent, SimpleNoteCutComparator>
        {
            protected override void Reinitialize(NoteController controller, NoteEvent noteEvent, SimpleNoteCutComparator item)
            {
                item.Construct(controller, noteEvent);
            }
        }

        [Inject] protected readonly AudioTimeSyncController _timeSyncController;

        public NoteEvent NoteEvent { get; private set; }
        public NoteController NoteController { get; private set; }
        public bool IsAvailableForCut { get; private set; }
        public bool IsFinished { get; private set; }

        public void Construct(NoteController controller, NoteEvent noteEvent)
        {
            NoteController = controller;
            NoteEvent = noteEvent;
            NoteController.noteWasCutEvent.Add(this);
            IsAvailableForCut = true;
            IsFinished = false;
        }
        public void Dispose()
        {
            //NoteEvent = null;
            //NoteController = null;
            IsAvailableForCut = false;
            IsFinished = true;
        }
        private void Update()
        {
            if (IsAvailableForCut && _timeSyncController.songTime >= NoteEvent.eventTime)
            {
                Cut(ReplayNoteCutInfo.Parse(NoteEvent.noteCutInfo, NoteController));
            }
        }

        public void HandleNoteControllerNoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            NoteController.noteWasCutEvent.Remove(this);
            IsAvailableForCut = false;
        }
        private void Cut(NoteCutInfo noteCutInfo)
        {
            foreach (var item in ((LazyCopyHashSet<INoteControllerNoteWasCutEvent>)NoteController.noteWasCutEvent).items)
            {
                item.HandleNoteControllerNoteWasCut(NoteController, noteCutInfo);
            }
        }
    }
}
