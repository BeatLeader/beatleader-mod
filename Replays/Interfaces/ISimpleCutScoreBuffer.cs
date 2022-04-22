using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Replays.Interfaces
{
    public interface ISimpleCutScoreBuffer
    {
        int maxPossibleCutScore
        {
            get;
        }
        int cutScore
        {
            get;
        }
        int beforeCutScore
        {
            get;
        }
        int centerDistanceCutScore
        {
            get;
        }
        int afterCutScore
        {
            get;
        }
        bool isFinished
        {
            get;
        }
        ScoreModel.NoteScoreDefinition noteScoreDefinition
        {
            get;
        }
        NoteCutInfo noteCutInfo
        {
            get;
        }
        float beforeCutSwingRating
        {
            get;
        }
        float afterCutSwingRating
        {
            get;
        }
    }
}
