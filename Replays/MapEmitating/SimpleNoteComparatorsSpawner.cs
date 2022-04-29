using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.MapEmitating
{
    public class SimpleNoteComparatorsSpawner : MonoBehaviour
    {
        [Inject] protected readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] protected readonly AudioTimeSyncController _songSyncController;
        [Inject] protected readonly ReplayManualInstaller.InitData _initData;
        [Inject] protected readonly SimpleNoteCutComparator.Pool _simpleNoteCutComparatorPool;
        [Inject] protected readonly Replay _replay;

        protected List<SimpleNoteCutComparator> _spawnedComparators;
        protected bool _initialized;

        protected bool _spawnInRealTime;
        protected bool _spawnAsync;

        public void Start()
        {
            _spawnInRealTime = _initData.generateInRealTime;
            _spawnedComparators = new List<SimpleNoteCutComparator>();
            if (!_spawnInRealTime)
            {
                _spawnAsync = false;
                PreloadComparators();
            }
            else _beatmapObjectManager.noteWasSpawnedEvent += AddNoteComparator;
            _initialized = true;
        }
        public void Update()
        {
            if (_spawnedComparators.Count != 0)
            {
                List<SimpleNoteCutComparator> comparatorsToRemove = new List<SimpleNoteCutComparator>();
                foreach (var item in _spawnedComparators)
                {
                    if (item.isFinished)
                    {
                        comparatorsToRemove.Add(item);
                    }
                }
                foreach (var item in comparatorsToRemove)
                {
                    _spawnedComparators.Remove(item);
                    _simpleNoteCutComparatorPool.Despawn(item);
                }
            }
        }
        public void OnDestroy()
        {
            if (_spawnInRealTime)
            {
                _beatmapObjectManager.noteWasSpawnedEvent -= AddNoteComparator;
            }
        }
        public bool TryGetLoadedComparator(NoteController noteController, out SimpleNoteCutComparator comparator)
        {
            foreach (var item in _spawnedComparators)
            {
                if (item.noteController == noteController)
                {
                    comparator = item;
                    return true;
                }
            }
            comparator = null;
            return false;
        }
        public bool TryGetLoadedComparator(int ID, out SimpleNoteCutComparator comparator)
        {
            foreach (var item in _spawnedComparators)
            {
                if (item.noteID == ID)
                {
                    comparator = item;
                    return true;
                }
            }
            comparator = null;
            return false;
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
                SimpleNoteCutComparator comparator = _simpleNoteCutComparatorPool.Spawn();
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
                SimpleNoteCutComparator comparator = _simpleNoteCutComparatorPool.Spawn();
                comparator.Init(controller, noteCutEvent);
                comparator.transform.SetParent(controller.transform);
                _spawnedComparators.Add(comparator);
            }
        }
    }
}
