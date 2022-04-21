using System;
using System.Threading.Tasks;
using BeatLeader.Utils;
using BeatLeader.Models;
using BeatLeader.Replays;
using UnityEngine;
using ReplayNoteCutInfo = BeatLeader.Models.NoteCutInfo;

namespace BeatLeader.Replays.Scoring
{
    public class NoteEventCutScoreBuffer : IReadonlyCutScoreBuffer
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
        public NoteCutInfo noteCutInfo => _noteCutInfo;
        public float beforeCutSwingRating => 0;
        public float afterCutSwingRating => 0;

        public virtual void StartNoteEventCalculating()
        {
            if (_noteEvent != null && _noteEvent.noteCutInfo != null)
            {
                _noteCutInfo = ReplayNoteCutInfo.Parse(_noteEvent.noteCutInfo, noteCutInfo.noteData, noteCutInfo.worldRotation, noteCutInfo.inverseWorldRotation, noteCutInfo.noteRotation, noteCutInfo.notePosition);
                RefreshScores();
            }
        }
        public virtual void RefreshScores()
        {
            _beforeCutScore = (int)Mathf.Clamp((float)Math.Round(70 * _noteEvent.noteCutInfo.beforeCutRating), 0, 70);
            _afterCutScore = (int)Mathf.Clamp((float)Math.Round(30 * _noteEvent.noteCutInfo.afterCutRating), 0, 30);
            _centerDistanceCutScore = (int)Math.Round(15 * (1 - Mathf.Clamp(_noteEvent.noteCutInfo.cutDistanceToCenter / 0.3f, 0.0f, 1.0f)));
        }
        public virtual bool Init(in NoteCutInfo noteCutInfo, NoteEvent noteEvent)
        {
            _initialized = true;
            _noteScoreDefinition = ScoreModel.GetNoteScoreDefinition(noteCutInfo.noteData.scoringType);
            _noteEvent = noteEvent;
            _noteCutInfo = ReplayNoteCutInfo.Parse(_noteEvent.noteCutInfo, noteCutInfo.noteData, noteCutInfo.worldRotation, noteCutInfo.inverseWorldRotation, noteCutInfo.noteRotation, noteCutInfo.notePosition);
            StartNoteEventCalculating();
            _isFinished = true;
            return false;

        }
        public void RegisterDidChangeReceiver(ICutScoreBufferDidChangeReceiver receiver) { }
        public void RegisterDidFinishReceiver(ICutScoreBufferDidFinishReceiver receiver) { }
        public void UnregisterDidChangeReceiver(ICutScoreBufferDidChangeReceiver receiver) { }
        public void UnregisterDidFinishReceiver(ICutScoreBufferDidFinishReceiver receiver) { }
    }
}
