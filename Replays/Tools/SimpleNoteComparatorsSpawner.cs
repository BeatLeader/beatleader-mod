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
        [Inject] protected readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] protected readonly AudioTimeSyncController _songSyncController;
        [Inject] protected readonly ReplayManualInstaller.InitData _initData;
        [Inject] protected readonly SimpleNoteCutComparator.Factory _simpleNoteCutComparatorFactory;
        [Inject] protected readonly Replay _replay;

        protected List<SimpleNoteCutComparator> _spawnedComparators;
        protected bool _initialized;

        protected bool _spawnInRealTime;
        protected bool _spawnAsync;

        public void Start()
        {
            _spawnInRealTime = _initData.generateInRealTime;
            _spawnAsync = _initData.generateAsync;
            _spawnedComparators = new List<SimpleNoteCutComparator>();
            if (!_spawnInRealTime)
            {
                _spawnAsync = false;
                PreloadComparators();
            }
            else if (_spawnAsync)
                _beatmapObjectManager.noteWasSpawnedEvent += AddNoteComparatorAsync;
            else
                _beatmapObjectManager.noteWasSpawnedEvent += AddNoteComparator;
            _initialized = true;
        }
        public void OnDestroy()
        {
            Debug.LogWarning("OnDestroy");
            if (_spawnInRealTime)
            {
                if (_spawnAsync)
                    _beatmapObjectManager.noteWasSpawnedEvent -= AddNoteComparatorAsync;
                else 
                    _beatmapObjectManager.noteWasSpawnedEvent -= AddNoteComparator; 
            }
        }
        private void PreloadComparators()
        {
            if (!_initialized)
            {
                var controllers = Resources.FindObjectsOfTypeAll<NoteController>();
                foreach (var item in controllers)
                {
                    var noteEvents = item.GetAllNoteEventsForNoteController(_replay);
                    foreach (var item2 in noteEvents)
                    {
                        if (item2 != null && item2.eventType != NoteEventType.miss && item2.noteCutInfo != null)
                        {
                            AddNoteComparator(item, item2);
                        }
                    }
                }
            }
        }
        private void AddNoteComparator(NoteController controller, NoteEvent noteCutEvent)
        {
            if (noteCutEvent != null && noteCutEvent.eventType != NoteEventType.miss && noteCutEvent.noteCutInfo != null)
            {
                SimpleNoteCutComparator comparator = _simpleNoteCutComparatorFactory.Create();
                comparator.Init(controller, noteCutEvent);
                comparator.transform.SetParent(controller.transform);
                _spawnedComparators.Add(comparator);
            }
        }
        private void AddNoteComparator(NoteController controller)
        {
            var noteCutEvent = controller.GetNoteEvent(_replay);
            if (noteCutEvent != null && noteCutEvent.eventType != NoteEventType.miss && noteCutEvent.noteCutInfo != null)
            {
                SimpleNoteCutComparator comparator = _simpleNoteCutComparatorFactory.Create();
                comparator.Init(controller, noteCutEvent);
                comparator.transform.SetParent(controller.transform);
                _spawnedComparators.Add(comparator);
            }
        }
        private async void AddNoteComparatorAsync(NoteController controller)
        {
            var noteCutEvent = await controller.GetNoteEventAsync(_replay);
            if (noteCutEvent != null && noteCutEvent.eventType != NoteEventType.miss && noteCutEvent.noteCutInfo != null)
            {
                SimpleNoteCutComparator comparator = _simpleNoteCutComparatorFactory.Create();
                comparator.Init(controller, noteCutEvent);
                _spawnedComparators.Add(comparator);
            }
        }
    }
}
