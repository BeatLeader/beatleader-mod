using ReplayNoteCutInfo = BeatLeader.Models.NoteCutInfo;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation
{
    public class SimpleNoteCutComparator : MonoBehaviour, INoteControllerNoteWasCutEvent
    {
        public class Pool : MonoMemoryPool<int, NoteController, NoteEvent, SimpleNoteCutComparator>
        {
            protected override void Reinitialize(int id, NoteController controller, NoteEvent noteEvent, SimpleNoteCutComparator item)
            {
                item.IdInOrder = id;
                item.Construct(controller, noteEvent);
            }
        }

        [Inject] private readonly AudioTimeSyncController _timeSyncController;
        //[Inject] private readonly SimpleNoteComparatorsSpawner _spawner;

        public NoteEvent NoteEvent { get; private set; }
        public NoteController NoteController { get; private set; }
        public bool IsAvailableToCut { get; private set; }
        public bool IsFinished { get; private set; }
        public int IdInOrder { get; private set; }

        private SimpleNoteCutComparator _previousInOrderComparator;

        private void Construct(NoteController controller, NoteEvent noteEvent)
        {
            NoteController = controller;
            NoteEvent = noteEvent;
            NoteController.noteWasCutEvent.Add(this);
            //_spawner.TryGetLoadedComparator(x => x.IdInOrder == IdInOrder - 1, out _previousInOrderComparator);
            //Debug.Log($"Initialized comparator {IdInOrder} with {_previousInOrderComparator != null}");
            IsAvailableToCut = true;
            IsFinished = false;
        }
        public void Dispose()
        {
            //NoteEvent = null;
            //NoteController = null;
            //_previousInOrderComparator = null;
            IsAvailableToCut = false;
            IsFinished = true;
        }
        private void Update()
        {
            if (IsAvailableToCut && _timeSyncController.state.Equals(AudioTimeSyncController.State.Playing)
                && _timeSyncController.songTime >= NoteEvent.eventTime)
            {
                //Debug.Log("Processing comparator " + IdInOrder);
                Cut(ReplayNoteCutInfo.ToBaseGame(NoteEvent.noteCutInfo, NoteController));
            }
        }

        public void HandleNoteControllerNoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            NoteController.noteWasCutEvent.Remove(this);
            IsAvailableToCut = false;
        }
        private void Cut(NoteCutInfo noteCutInfo)
        {
            foreach (var item in ((LazyCopyHashSet<INoteControllerNoteWasCutEvent>)NoteController.noteWasCutEvent).items)
            {
                item.HandleNoteControllerNoteWasCut(NoteController, noteCutInfo);
            }
        }
        private bool OrderIsCorrectNow()
        {
            return (_previousInOrderComparator != null && _previousInOrderComparator.IsAvailableToCut)
                || _previousInOrderComparator == null;
        }
    }
}
