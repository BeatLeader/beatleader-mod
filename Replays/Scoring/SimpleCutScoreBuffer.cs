using System;
using BeatLeader.Utils;
using BeatLeader.Models;
using BeatLeader.Replays;
using UnityEngine;
using ReplayNoteCutInfo = BeatLeader.Models.NoteCutInfo;

namespace BeatLeader.Replays.Scoring
{
    public class SimpleCutScoreBuffer : CutScoreBuffer
    {
        #region If using IReadonlyCutScoreBuffer
        /*protected NoteCutInfo _noteCutInfo;
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
        public float afterCutSwingRating => 0;*/
        #endregion

        protected NoteEvent _noteEvent;

        private void Change()
        {
            if (_didChangeEvent.items != null && _didChangeEvent.items.Count > 0)
            {
                foreach (var item in _didChangeEvent.items)
                {
                    item.HandleCutScoreBufferDidChange(this);
                }
            }
        }
        private void Finish()
        {
            if (_didFinishEvent.items != null && _didFinishEvent.items.Count > 0)
            {
                _initialized = false;
                _isFinished = true;
                foreach (var item in _didFinishEvent.items)
                {
                    item.HandleCutScoreBufferDidFinish(this);
                }
            }
        }
        private async void StartNoteEventCalculating(NoteCutInfo noteCutInfo)
        {
            _noteEvent = await noteCutInfo.noteData.GetNoteEventAsync(ReplayMenuUI.replay);
            if (_noteEvent != null)
            {
                RefreshScores();
                _noteCutInfo = ReplayNoteCutInfo.Parse(_noteEvent.noteCutInfo, noteCutInfo.noteData, noteCutInfo.worldRotation, noteCutInfo.inverseWorldRotation, noteCutInfo.noteRotation, noteCutInfo.notePosition);
                Finish();
            }
        }
        public override void RefreshScores()
        {
            if (_noteEvent != null)
            {
                _beforeCutScore = (int)Mathf.Clamp((float)Math.Round(70 * _noteEvent.noteCutInfo.beforeCutRating), 0, 70);
                _afterCutScore = (int)Mathf.Clamp((float)Math.Round(30 * _noteEvent.noteCutInfo.afterCutRating), 0, 30);
                _centerDistanceCutScore = (int)Math.Round(15 * (1 - Mathf.Clamp(_noteEvent.noteCutInfo.cutDistanceToCenter / 0.3f, 0.0f, 1.0f)));
            }
        }
        public override bool Init(in NoteCutInfo noteCutInfo)
        {
            _initialized = true;
            _noteScoreDefinition = ScoreModel.GetNoteScoreDefinition(noteCutInfo.noteData.scoringType);
            if (_noteEvent != null)
            {
                RefreshScores();
                Finish();
                _isFinished = true;
                _initialized = false;
                return false;
            }
            else
            {
                StartNoteEventCalculating(noteCutInfo);
                return true;
            }
        }
    }
}
