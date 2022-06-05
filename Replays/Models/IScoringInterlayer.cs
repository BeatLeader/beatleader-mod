using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Replays.Models
{
    public interface IScoringInterlayer
    {
        ScoringElement Convert(ScoringElement element, Type type); //where T - conversion type
    }
}
