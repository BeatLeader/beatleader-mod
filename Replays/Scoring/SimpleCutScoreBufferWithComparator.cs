using System;
using System.Threading.Tasks;
using BeatLeader.Utils;
using BeatLeader.Models;
using BeatLeader.Replays;
using BeatLeader.Replays.Tools;
using UnityEngine;
using ReplayNoteCutInfo = BeatLeader.Models.NoteCutInfo;
using BeatLeader.Replays.Interfaces;

namespace BeatLeader.Replays.Scoring
{
    public class SimpleCutScoreBufferWithComparator : IReadonlyCutScoreBuffer
    {
        protected NoteCutInfo _noteCutInfo;
        protected SimpleNoteCutComparator _noteCutComparator;
        protected ScoreModel.NoteScoreDefinition _noteScoreDefinition;
        protected int _afterCutScore;
        protected int _beforeCutScore;
        protected int _centerDistanceCutScore;
        protected bool _initialized;
        protected bool _isFinished;

        public int maxPossibleCutScore => _noteScoreDefinition.maxCutScore;
        public bool isFinished => _isFinished;
        public int cutScore => _afterCutScore + _beforeCutScore + _centerDistanceCutScore + _noteScoreDefinition.fixedCutScore;
        public int beforeCutScore => _beforeCutScore;
        public int centerDistanceCutScore => _centerDistanceCutScore;
        public int afterCutScore => _afterCutScore;
        public ScoreModel.NoteScoreDefinition noteScoreDefinition => _noteScoreDefinition;
        public SimpleNoteCutComparator noteCutComparator => _noteCutComparator;
        public NoteEvent noteEvent => _noteCutComparator.noteCutEvent;
        public NoteCutInfo noteCutInfo => _noteCutInfo;
        public float beforeCutSwingRating => 0;
        public float afterCutSwingRating => 0;

        public virtual void RefreshScores()
        {
            if (noteEvent != null && noteEvent.noteCutInfo != null)
            {
                _beforeCutScore = (int)Mathf.Clamp((float)Math.Round(70 * noteEvent.noteCutInfo.beforeCutRating), 0, 70);
                _afterCutScore = (int)Mathf.Clamp((float)Math.Round(30 * noteEvent.noteCutInfo.afterCutRating), 0, 30);
                _centerDistanceCutScore = (int)Math.Round(15 * (1 - Mathf.Clamp(noteEvent.noteCutInfo.cutDistanceToCenter / 0.3f, 0.0f, 1.0f)));
            }
        }
        public virtual void Init(SimpleNoteCutComparator comparator)
        {
            _initialized = true;
            _noteCutComparator = comparator;
            _noteCutInfo = comparator.noteController.GetNoteCutInfo(noteEvent);
            _noteScoreDefinition = ScoreModel.GetNoteScoreDefinition(_noteCutInfo.noteData.scoringType);
            if (noteEvent != null)
            {
                RefreshScores();
                _isFinished = true;
                _initialized = false;
            }
        }
        public void RegisterDidChangeReceiver(ICutScoreBufferDidChangeReceiver receiver) { }
        public void RegisterDidFinishReceiver(ICutScoreBufferDidFinishReceiver receiver) { }
        public void UnregisterDidChangeReceiver(ICutScoreBufferDidChangeReceiver receiver) { }
        public void UnregisterDidFinishReceiver(ICutScoreBufferDidFinishReceiver receiver) { }
    }
}
