using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Models
{
    public interface IReplayerScoreController : IScoreController
    {
        int MaxComboAfterRescoring { get; }
        int ComboAfterRescoring { get; }

        event Action<int, int, bool> OnComboChangedAfterRescoring;
    }
}
