using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    internal class ReplayBeatmapData : IInitializable, IReplayBeatmapData {
        #region Injection

        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly IReadonlyBeatmapData _beatmapData = null!;

        #endregion

        #region ReplayBeatmapData

        public IReadOnlyCollection<NoteData> NoteDatas {
            get {
                Initialize();
                return _generatedNoteDatas;
            }
        }

        public int FindNoteDataForEvent(NoteEvent noteEvent, int startIndex, out NoteData? noteData) {
            Initialize();
            var index = 0;
            var lastNoteTime = 0f;
            for (var i = startIndex; i < _generatedNoteDatas.Count; i++) {
                var data = _generatedNoteDatas[i];

                //TODO: current algorithm does not work properly. rework
                var notesDoMatchInTime = Mathf.Abs(lastNoteTime - data.time) < 1e-3;
                if (notesDoMatchInTime) {
                    lastNoteTime = data.time;
                    index = i;
                }

                var timeDoesMatch = Mathf.Abs(noteEvent.spawnTime - data.time) < 1e-3;
                if (!timeDoesMatch) continue;

                var idDoesMatch = _comparator.Compare(noteEvent, data);
                if (!idDoesMatch) continue;

                noteData = data;
                return index;
            }
            Plugin.Log.Error("[Replayer] Failed to acquire NoteData with id: " + noteEvent.noteId);
            Plugin.Log.Warn("[Replayer] The replay seems to be broken!");

            noteData = null;
            return index;
        }

        #endregion

        #region Setup

        private bool _isInitialized;
        
        public void Initialize() {
            if (_isInitialized) return;
            _comparator = _launchData.ReplayComparator;
            var beatmapItems = _beatmapData.allBeatmapDataItems;
            var noteDataList = CreateSortedNoteDataList(beatmapItems);
            _generatedNoteDatas.AddRange(noteDataList);
            _isInitialized = true;
        }

        #endregion

        #region MoteData Handling

        private class BeatmapDataItemsComparer : IComparer<BeatmapDataItem> {
            public int Compare(BeatmapDataItem left, BeatmapDataItem right) {
                return left.time > right.time ? 1 : left.time < right.time ? -1 : 0;
            }
        }

        private static readonly BeatmapDataItemsComparer beatmapItemsComparer = new();
        private readonly List<NoteData> _generatedNoteDatas = new();
        private IReplayComparator _comparator = null!;
        
        private static IEnumerable<NoteData> CreateSortedNoteDataList(IEnumerable<BeatmapDataItem> items) {
            return items
                .Select(
                    static x => x switch {
                        NoteData data => data,
                        SliderData sliderData => NoteData.CreateBurstSliderNoteData(
                            sliderData.time, sliderData.headLineIndex, sliderData.headLineLayer,
                            sliderData.headBeforeJumpLineLayer, sliderData.colorType, NoteCutDirection.Any, 1f
                        ),
                        _ => null
                    }
                )
                .OfType<NoteData>()
                .OrderBy(static x => x, beatmapItemsComparer);
        }

        #endregion
    }
}