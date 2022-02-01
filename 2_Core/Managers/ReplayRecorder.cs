using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader {
    [UsedImplicitly]
    public class ReplayRecorder : IInitializable, IDisposable {
        #region Constructor

        private readonly BeatmapObjectManager _beatmapObjectManager;

        public ReplayRecorder(BeatmapObjectManager beatmapObjectManager) {
            _beatmapObjectManager = beatmapObjectManager;
        }

        #endregion

        #region NoteIdHandling

        private readonly Dictionary<int, NoteCutInfo> _cutInfoCache = new();
        private readonly Dictionary<int, SwingRatingCounterDidFinishReceiver> _finishReceiversCache = new();
        private readonly Dictionary<int, SwingRatingCounterDidChangeReceiver> _changeReceiversCache = new();
        private readonly Dictionary<NoteController, int> _noteIdCache = new();
        private int _noteId;

        #endregion

        #region Events Subscription

        public void Initialize() {
            _beatmapObjectManager.noteWasSpawnedEvent += OnNoteWasSpawned;
            _beatmapObjectManager.noteWasMissedEvent += OnNoteWasMissed;
            _beatmapObjectManager.noteWasCutEvent += OnNoteWasCut;
            _beatmapObjectManager.noteWasDespawnedEvent += OnNoteWasDespawned;
        }

        public void Dispose() {
            _beatmapObjectManager.noteWasSpawnedEvent -= OnNoteWasSpawned;
            _beatmapObjectManager.noteWasMissedEvent -= OnNoteWasMissed;
            _beatmapObjectManager.noteWasCutEvent -= OnNoteWasCut;
            _beatmapObjectManager.noteWasDespawnedEvent -= OnNoteWasDespawned;
        }

        #endregion

        #region OnNoteWasSpawned

        private void OnNoteWasSpawned(NoteController noteController) {
            var noteId = _noteId++;
            _noteIdCache[noteController] = noteId;

            Plugin.Log.Info($"Note_{noteId} was spawned!");
        }

        #endregion

        #region OnNoteWasMissed

        private void OnNoteWasMissed(NoteController noteController) {
            var noteId = _noteIdCache[noteController];
            Plugin.Log.Info($"Note_{noteId} was missed!");
        }

        #endregion

        #region OnNoteWasCut

        private void OnNoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo) {
            var noteId = _noteIdCache[noteController];

            Plugin.Log.Info($"Note_{noteId} was cut!");

            if (noteCutInfo.swingRatingCounter == null) return;
            _cutInfoCache[noteId] = noteCutInfo;

            var counterDidChangeReceiver = new SwingRatingCounterDidChangeReceiver(noteId, OnSwingRatingCounterDidChange);
            noteCutInfo.swingRatingCounter.RegisterDidChangeReceiver(counterDidChangeReceiver);
            _changeReceiversCache[noteId] = counterDidChangeReceiver;

            var counterDidFinishReceiver = new SwingRatingCounterDidFinishReceiver(noteId, OnSwingRatingCounterDidFinish);
            noteCutInfo.swingRatingCounter.RegisterDidFinishReceiver(counterDidFinishReceiver);
            _finishReceiversCache[noteId] = counterDidFinishReceiver;
        }

        #endregion

        #region OnSwingRatingCounterDidChange

        private void OnSwingRatingCounterDidChange(ISaberSwingRatingCounter swingRatingCounter, int noteId, float rating) {
            Plugin.Log.Info($"Note_{noteId} post-swing rating changed to {rating}!");

            var cutInfo = _cutInfoCache[noteId];

            ScoreModel.RawScoreWithoutMultiplier(swingRatingCounter, cutInfo.cutDistanceToCenter,
                out var beforeCutRawScore,
                out var afterCutRawScore,
                out var cutDistanceRawScore
            );

            Plugin.Log.Info($"pre: {beforeCutRawScore} acc: {cutDistanceRawScore} post: {afterCutRawScore}");
        }

        #endregion

        #region OnSwingRatingCounterDidFinish

        private void OnSwingRatingCounterDidFinish(ISaberSwingRatingCounter swingRatingCounter, int noteId) {
            swingRatingCounter.UnregisterDidChangeReceiver(_changeReceiversCache[noteId]);
            swingRatingCounter.UnregisterDidFinishReceiver(_finishReceiversCache[noteId]);

            Plugin.Log.Info($"Note_{noteId} post-swing rating finished!");

            var cutInfo = _cutInfoCache[noteId];

            ScoreModel.RawScoreWithoutMultiplier(swingRatingCounter, cutInfo.cutDistanceToCenter,
                out var beforeCutRawScore,
                out var afterCutRawScore,
                out var cutDistanceRawScore
            );

            Plugin.Log.Info($"pre: {beforeCutRawScore} acc: {cutDistanceRawScore} post: {afterCutRawScore}");
        }

        #endregion

        #region OnNoteWasDespawned

        private void OnNoteWasDespawned(NoteController noteController) {
            var noteId = _noteId++;
            _noteIdCache[noteController] = noteId;

            Plugin.Log.Info($"Note_{noteId} was Despawned!");
        }

        #endregion

        #region SwingRatingCounterDidChangeReceiver

        private class SwingRatingCounterDidChangeReceiver : ISaberSwingRatingCounterDidChangeReceiver {
            private readonly Action<ISaberSwingRatingCounter, int, float> _finishEvent;
            private readonly int _noteId;

            public SwingRatingCounterDidChangeReceiver(int noteId, Action<ISaberSwingRatingCounter, int, float> finishEvent) {
                _finishEvent = finishEvent;
                _noteId = noteId;
            }

            public void HandleSaberSwingRatingCounterDidChange(ISaberSwingRatingCounter saberSwingRatingCounter, float rating) {
                _finishEvent.Invoke(saberSwingRatingCounter, _noteId, rating);
            }
        }

        #endregion

        #region SwingRatingCounterDidFinishReceiver

        private class SwingRatingCounterDidFinishReceiver : ISaberSwingRatingCounterDidFinishReceiver {
            private readonly Action<ISaberSwingRatingCounter, int> _finishEvent;
            private readonly int _noteId;

            public SwingRatingCounterDidFinishReceiver(int noteId, Action<ISaberSwingRatingCounter, int> finishEvent) {
                _finishEvent = finishEvent;
                _noteId = noteId;
            }

            public void HandleSaberSwingRatingCounterDidFinish(ISaberSwingRatingCounter saberSwingRatingCounter) {
                _finishEvent.Invoke(saberSwingRatingCounter, _noteId);
            }
        }

        #endregion
    }
}