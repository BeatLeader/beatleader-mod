using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Replays.Interfaces
{
    public interface ISimpleScoringInterlayer
    {
        ScoringElement scoringElement { get; }
        IReadonlyCutScoreBuffer cutScoreBuffer { get; }

        void ConvertScoringElement(ScoringElement element, IReadonlyCutScoreBuffer buffer);
        void ConvertBuffer(IReadonlyCutScoreBuffer buffer);
    }
}
