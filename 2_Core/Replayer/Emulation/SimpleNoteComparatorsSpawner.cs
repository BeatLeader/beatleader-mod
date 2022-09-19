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
        protected HashSet<SimpleNoteCutComparator> _cachedComparators = new();

        protected virtual void Start()
        {
            _beatmapObjectManager.noteWasSpawnedEvent += AddNoteComparator;
        }
        protected virtual void Update()
        {
            if (_spawnedComparators.Count == 0) return;

            while (true)
            {
                int minIndex = int.MaxValue;
                SimpleNoteCutComparator comparator = null;
                bool comparatorIsNotNull = false;

                foreach (var item in _spawnedComparators)
                {
                    if (item.IdInOrder < minIndex && item.IsProcessRequired)
                    {
                        minIndex = item.IdInOrder;
                        comparator = item;
                        comparatorIsNotNull = true;
                    }
                }

                if (comparatorIsNotNull)
                {
                    if (comparator.NoteEvent.eventType != NoteEventType.miss)
                        CutNoteController(comparator.NoteController,
                            Models.NoteCutInfo.ToBaseGame(comparator.NoteEvent.noteCutInfo, comparator.NoteController));
                    else MissNoteController(comparator.NoteController);
                    _spawnedComparators.Remove(comparator);
                }
                else break;
            }

            foreach (var item in _cachedComparators.Where(x => x.IsFinished))
            {
                _cachedComparators.Remove(item);
                _simpleNoteCutComparatorPool.Despawn(item);
            }
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
            comparator ??= _cachedComparators.Where(filter).FirstOrDefault();
            return comparator != null;
        }
        protected void AddNoteComparator(NoteController controller)
        {
            var noteEvent = controller.GetNoteEventInOrder(_replayData.replay);
            AddNoteComparator(controller, noteEvent.Item1, controller.GetNoteEvent(_replayData.replay));
        }
        protected void AddNoteComparator(NoteController controller, int id, NoteEvent noteCutEvent)
        {
            if (noteCutEvent == null || noteCutEvent.noteCutInfo == null) return;

            SimpleNoteCutComparator comparator = _simpleNoteCutComparatorPool.Spawn(id, controller, noteCutEvent);
            comparator.transform.SetParent(controller.transform);
            _spawnedComparators.Add(comparator);
        }

        protected void CutNoteController(NoteController controller, NoteCutInfo noteCutInfo)
        {
            foreach (var item in ((LazyCopyHashSet<INoteControllerNoteWasCutEvent>)controller.noteWasCutEvent).items)
            {
                item.HandleNoteControllerNoteWasCut(controller, noteCutInfo);
            }
        }
        protected void MissNoteController(NoteController controller)
        {
            _beatmapObjectManager.HandleNoteControllerNoteWasMissed(controller);
        }
    }
}
