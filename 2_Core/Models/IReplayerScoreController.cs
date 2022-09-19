using System;

namespace BeatLeader.Models
{
    public interface IReplayerScoreController : IScoreController
    {
        int MaxComboAfterRescoring { get; }
        int ComboAfterRescoring { get; }
        float EnergyAfterRescoring { get; }

        event Action<int, int, bool> OnComboChangedAfterRescoring;
    }
}
