using BeatLeader.Models;
using BeatLeader.Models.Replay;

namespace BeatLeader {
    internal interface IReplayerStarter {
        void StartReplay(Replay replay, Player player);
    }
}