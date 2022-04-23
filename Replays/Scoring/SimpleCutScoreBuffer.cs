using System;
using System.Threading.Tasks;
using BeatLeader.Models;
using UnityEngine;
using ReplayNoteCutInfo = BeatLeader.Models.NoteCutInfo;

namespace BeatLeader.Replays.Scoring
{
    public class SimpleCutScoreBuffer : IReadonlyCutScoreBuffer
    {
        protected NoteCutInfo _noteCutInfo;
        protected NoteEvent _noteEvent;
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
        public NoteEvent noteEvent => _noteEvent;
        public NoteCutInfo noteCutInfo => _noteCutInfo;
        public float beforeCutSwingRating => 0;
        public float afterCutSwingRating => 0;

        public virtual void RefreshScores()
        {
            if (_noteEvent != null && noteEvent.noteCutInfo != null)
            {
                _beforeCutScore = (int)Mathf.Clamp((float)Math.Round(70 * _noteEvent.noteCutInfo.beforeCutRating), 0, 70);
                _afterCutScore = (int)Mathf.Clamp((float)Math.Round(30 * _noteEvent.noteCutInfo.afterCutRating), 0, 30);
                _centerDistanceCutScore = (int)Math.Round(15 * (1 - Mathf.Clamp(_noteEvent.noteCutInfo.cutDistanceToCenter / 0.3f, 0.0f, 1.0f)));
            }
        }
        public virtual void Init(SimpleScoringData data)
        {
            _initialized = true;
            _noteEvent = data.noteEvent;
            if (data.noteEvent.noteCutInfo != null)
                _noteCutInfo = ReplayNoteCutInfo.Parse(data.noteEvent.noteCutInfo, data.noteData, data.worldRotation, data.inverseWorldRotation, data.noteRotation, data.notePosition);
            _noteScoreDefinition = ScoreModel.GetNoteScoreDefinition(data.noteData.scoringType);
            RefreshScores();
            _isFinished = true;
            _initialized = false;
        }
        public void RegisterDidChangeReceiver(ICutScoreBufferDidChangeReceiver receiver) { }
        public void RegisterDidFinishReceiver(ICutScoreBufferDidFinishReceiver receiver) { }
        public void UnregisterDidChangeReceiver(ICutScoreBufferDidChangeReceiver receiver) { }
        public void UnregisterDidFinishReceiver(ICutScoreBufferDidFinishReceiver receiver) { }
    }
}
