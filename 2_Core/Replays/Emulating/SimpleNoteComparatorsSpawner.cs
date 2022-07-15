using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Emulating
{
    public class SimpleNoteComparatorsSpawner : MonoBehaviour
    {
        [Inject] protected readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] protected readonly AudioTimeSyncController _songSyncController;
        [Inject] protected readonly ReplayerManualInstaller.InitData _initData;
        [Inject] protected readonly SimpleNoteCutComparator.Pool _simpleNoteCutComparatorPool;
        [Inject] protected readonly Replay _replay;

        protected List<SimpleNoteCutComparator> _spawnedComparators;
        protected bool _initialized;

        public void Start()
        {
            _spawnedComparators = new List<SimpleNoteCutComparator>();
            _beatmapObjectManager.noteWasSpawnedEvent += AddNoteComparator;
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
            _beatmapObjectManager.noteWasSpawnedEvent -= AddNoteComparator;
        }
        public void DespawnAllComparators()
        {
            List<SimpleNoteCutComparator> comparatorsToRemove = new List<SimpleNoteCutComparator>();
            foreach (var item in _spawnedComparators)
            {
                comparatorsToRemove.Add(item);
            }
            foreach (var item in comparatorsToRemove)
            {
                _spawnedComparators.Remove(item);
                _simpleNoteCutComparatorPool.Despawn(item);
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
                if (item.noteCutEvent.noteID == ID)
                {
                    comparator = item;
                    return true;
                }
            }
            comparator = null;
            return false;
        }
        protected void AddNoteComparator(NoteController controller, NoteEvent noteCutEvent)
        {
            if (noteCutEvent != null && noteCutEvent.eventType != NoteEventType.miss && noteCutEvent.noteCutInfo != null)
            {
                SimpleNoteCutComparator comparator = _simpleNoteCutComparatorPool.Spawn();
                comparator.Init(controller, noteCutEvent);
                comparator.transform.SetParent(controller.transform);
                _spawnedComparators.Add(comparator);
            }
        }
        protected void AddNoteComparator(NoteController controller)
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
