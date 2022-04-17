using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Utils;
using BeatLeader.Models;
using BeatLeader.Replays;
using UnityEngine;

namespace BeatLeader.Replays.Scoring
{
    public class SimpleCutScoreBuffer : CutScoreBuffer
    {
        protected SimpleSwingRatingCounter _swingRatingCounter;
        public new int cutScore;

        public override bool Init(in NoteCutInfo noteCutInfo)
        {
            _noteCutInfo = noteCutInfo;
            NoteEvent noteEvent = noteCutInfo.noteData.GetNoteEvent(ReplayMenuUI.replay);
            var eventNoteCutInfo = noteEvent.noteCutInfo;
            cutScore = noteEvent.ComputeNoteScore();
            _swingRatingCounter = new SimpleSwingRatingCounter();
            _noteScoreDefinition = ScoreModel.GetNoteScoreDefinition(noteCutInfo.noteData.scoringType);
            _swingRatingCounter.Init(eventNoteCutInfo.beforeCutRating, eventNoteCutInfo.afterCutRating);
            _centerDistanceCutScore = Mathf.RoundToInt(_noteScoreDefinition.maxCenterDistanceCutScore * (1f - Mathf.Clamp01(noteCutInfo.cutDistanceToCenter / 0.3f)));
            bool flag = _noteScoreDefinition.maxBeforeCutScore > 0 && _noteScoreDefinition.minBeforeCutScore != _noteScoreDefinition.maxBeforeCutScore;
            bool flag2 = _noteScoreDefinition.maxAfterCutScore > 0 && _noteScoreDefinition.minAfterCutScore != _noteScoreDefinition.maxAfterCutScore;
            RefreshScores();
            if (flag || flag2)
            {
                _swingRatingCounter.RegisterDidChangeReceiver(this);
                _swingRatingCounter.RegisterDidFinishReceiver(this);
                _swingRatingCounter.Refresh();
                return true;
            }

            _initialized = false;
            _isFinished = true;
            _swingRatingCounter.Finish();
            return false;
        }
        public void Refresh()
        {
            HandleSaberSwingRatingCounterDidChange(_swingRatingCounter, _swingRatingCounter.afterCutRating);
            HandleSaberSwingRatingCounterDidFinish(_swingRatingCounter);
        }
        public override void RefreshScores()
        {
            _beforeCutScore = Mathf.RoundToInt(Mathf.LerpUnclamped(_noteScoreDefinition.minBeforeCutScore, _noteScoreDefinition.maxBeforeCutScore, _swingRatingCounter.beforeCutRating));
            _afterCutScore = Mathf.RoundToInt(Mathf.LerpUnclamped(_noteScoreDefinition.minAfterCutScore, _noteScoreDefinition.maxAfterCutScore, _swingRatingCounter.afterCutRating));
        }
        public override void HandleSaberSwingRatingCounterDidChange(ISaberSwingRatingCounter swingRatingCounter, float rating)
        {
            RefreshScores();
            foreach (ICutScoreBufferDidChangeReceiver item in _didChangeEvent.items)
            {
                item.HandleCutScoreBufferDidChange(this);
            }
        }
        public override void HandleSaberSwingRatingCounterDidFinish(ISaberSwingRatingCounter swingRatingCounter)
        {
            RefreshScores();
            _initialized = false;
            _isFinished = true;
            swingRatingCounter.UnregisterDidChangeReceiver(this);
            swingRatingCounter.UnregisterDidFinishReceiver(this);
            foreach (ICutScoreBufferDidFinishReceiver item in _didFinishEvent.items)
            {
                item.HandleCutScoreBufferDidFinish(this);
            }
        }
    }
}
