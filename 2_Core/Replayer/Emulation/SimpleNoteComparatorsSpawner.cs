using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation
{
    public class SimpleNoteComparatorsSpawner : MonoBehaviour
    {
        [Inject] protected readonly BeatmapObjectManager _beatmapObjectManager;
        [Inject] protected readonly AudioTimeSyncController _songSyncController;
        [Inject] protected readonly SimpleNoteCutComparator.Pool _simpleNoteCutComparatorPool;
        [Inject] protected readonly ReplayLaunchData _replayData;

        protected HashSet<SimpleNoteCutComparator> _spawnedComparators = new();
        protected HashSet<SimpleNoteCutComparator> _modifiedCache = new();

        protected virtual void Start()
        {
            _beatmapObjectManager.noteWasSpawnedEvent += AddNoteComparator;
        }
        protected virtual void Update()
        {
            if (_spawnedComparators.Count == 0) return;
            foreach (var item in _spawnedComparators.Where(x => x.IsFinished))
            {
                _modifiedCache.Add(item);
            }
            foreach (var item in _modifiedCache)
            {
                _spawnedComparators.Remove(item);
                _simpleNoteCutComparatorPool.Despawn(item);
            }
            _modifiedCache.Clear();
        }
        protected virtual void OnDestroy()
        {
            _beatmapObjectManager.noteWasSpawnedEvent -= AddNoteComparator;
        }
        public void DespawnAllComparators()
        {
            _spawnedComparators.ToList().ForEach(x => _simpleNoteCutComparatorPool.Despawn(x));
            _spawnedComparators.Clear();
        }
        public bool TryGetLoadedComparator(NoteController noteController, out SimpleNoteCutComparator comparator)
        {
            return TryGetLoadedComparator(x => x.NoteController == noteController, out comparator);
        }
        public bool TryGetLoadedComparator(Func<SimpleNoteCutComparator, bool> filter, out SimpleNoteCutComparator comparator)
        {
            comparator = _spawnedComparators.Where(filter).FirstOrDefault();
            return comparator != null;
        }
        protected void AddNoteComparator(NoteController controller)
        {
            var noteEvent = controller.GetNoteEventInOrder(_replayData.replay);
            AddNoteComparator(controller, noteEvent.Item1, controller.GetNoteEvent(_replayData.replay));
        }
        protected void AddNoteComparator(NoteController controller, int id, NoteEvent noteCutEvent)
        {
            if (noteCutEvent == null || noteCutEvent.eventType == NoteEventType.miss
                || noteCutEvent.noteCutInfo == null) return;

            SimpleNoteCutComparator comparator = _simpleNoteCutComparatorPool.Spawn(id, controller, noteCutEvent);
            comparator.transform.SetParent(controller.transform);
            _spawnedComparators.Add(comparator);
        }
    }
}
