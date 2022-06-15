using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Replays.Models
{
    public interface IScoringInterlayer
    {
        T Convert<T>(ScoringElement element) where T : ScoringElement;
    }
}
