using ReplayNoteCutInfo = BeatLeader.Models.NoteCutInfo;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation
{
    public class SimpleNoteCutComparator : MonoBehaviour
    {
        public class Pool : MonoMemoryPool<int, NoteController, NoteEvent, SimpleNoteCutComparator>
        {
            protected override void Reinitialize(int id, NoteController controller, NoteEvent noteEvent, SimpleNoteCutComparator item)
            {
                item.IdInOrder = id;
                item.NoteEvent = noteEvent;
                item.NoteController = controller;
                item.IsProcessRequired = false;
                item.IsFinished = false;
            }
        }

        [Inject] private readonly AudioTimeSyncController _timeSyncController;

        public NoteEvent NoteEvent { get; private set; }
        public NoteController NoteController { get; private set; }
        public bool IsProcessRequired { get; private set; }
        public bool IsFinished { get; private set; }
        public int IdInOrder { get; private set; }

        public void Deconstruct()
        {
            //NoteEvent = null;
            //NoteController = null;
            IsProcessRequired = false;
            IdInOrder = -1;
            IsFinished = true;
        }
        private void Update()
        {
            if (_timeSyncController.state.Equals(AudioTimeSyncController.State.Playing)
                && _timeSyncController.songTime >= NoteEvent.eventTime)
            {
                IsProcessRequired = true;
            }
        }
    }
}
