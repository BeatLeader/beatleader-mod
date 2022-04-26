using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Replays.Scoring
{
    public class CutScoreBufferWithConstructor : CutScoreBuffer
    {
        public CutScoreBufferWithConstructor(NoteCutInfo info, ScoreModel.NoteScoreDefinition definition, int afterCutScore, int beforeCutScore, int centerDistanceCutScore)
        {
            this._noteCutInfo = info;
            this._noteScoreDefinition = definition;
            this._afterCutScore = afterCutScore;
            this._beforeCutScore = beforeCutScore;
            this._centerDistanceCutScore = centerDistanceCutScore;
            this._initialized = true;
            this._isFinished = true;
        }
    }
}
