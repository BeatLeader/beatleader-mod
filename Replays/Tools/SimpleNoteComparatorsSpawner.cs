using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Tools
{
    public class SimpleNoteComparatorsSpawner : MonoBehaviour
    {
        [Inject] private protected BeatmapObjectManager _beatmapObjectManager;
        [Inject] private protected AudioTimeSyncController _songSyncController;
        [Inject] private protected Replay _replay;

        public void Start()
        {
            _beatmapObjectManager.noteWasSpawnedEvent += AddNoteComparator;
        }
        public void OnDestroy()
        {
            _beatmapObjectManager.noteWasSpawnedEvent -= AddNoteComparator;
        }
        private async void AddNoteComparator(NoteController controller)
        {
            NoteEvent noteCutEvent = await controller.GetNoteEventAsync(_replay);
            if (noteCutEvent != null && noteCutEvent.eventType != NoteEventType.miss && noteCutEvent.noteCutInfo != null)
            {
                SimpleNoteCutComparator comparator = controller.gameObject.AddComponent<SimpleNoteCutComparator>();
                comparator.timeSyncController = _songSyncController;
                comparator.noteCutEvent = noteCutEvent;
            }
        }
    }
}
