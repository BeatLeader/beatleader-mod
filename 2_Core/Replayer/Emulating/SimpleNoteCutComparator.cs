using ReplayNoteCutInfo = BeatLeader.Models.NoteCutInfo;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulating
{
    public class SimpleNoteCutComparator : MonoBehaviour, INoteControllerNoteWasCutEvent
    {
        public class Pool : MonoMemoryPool<SimpleNoteCutComparator>
        {
            protected override void Reinitialize(SimpleNoteCutComparator item)
            {
                item._noteCutEvent = null;
                item._noteController = null;
                item._availableForCut = false;
                item._initialized = false;
                item._isFinished = false;
            }
        }

        [Inject] protected readonly AudioTimeSyncController _timeSyncController;

        protected NoteEvent _noteCutEvent;
        protected NoteController _noteController;
        protected bool _availableForCut;
        protected bool _initialized;
        protected bool _isFinished;

        public virtual NoteEvent NoteCutEvent => _noteCutEvent;
        public virtual NoteController NoteController => _noteController;
        public virtual bool IsAvailableForCut => _availableForCut;
        public virtual bool IsFinished => _isFinished;

        public virtual void Update()
        {
            if (_initialized && _availableForCut && _timeSyncController.songTime >= NoteCutEvent.eventTime)
            {
                Cut(ReplayNoteCutInfo.Parse(_noteCutEvent.noteCutInfo, _noteController));
            }
        }
        public virtual void Dispose()
        {
            _availableForCut = false;
            _isFinished = true;
        }
        public virtual void Init(NoteController noteController, NoteEvent noteCutEvent)
        {
            _noteController = noteController;
            _noteCutEvent = noteCutEvent;
            _noteController.noteWasCutEvent.Add(this);
            _availableForCut = true;
            _initialized = true;
        }
        public virtual void HandleNoteControllerNoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            _noteController.noteWasCutEvent.Remove(this);
            _availableForCut = false;
        }
        private void Cut(NoteCutInfo noteCutInfo)
        {
            if (NoteController == null) return;
            foreach (var item in ((LazyCopyHashSet<INoteControllerNoteWasCutEvent>)NoteController.noteWasCutEvent).items)
            {
                item.HandleNoteControllerNoteWasCut(NoteController, noteCutInfo);
            }
        }
    }
}
