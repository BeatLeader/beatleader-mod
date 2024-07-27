using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal record BattleRoyaleReplay(IReplayHeaderBase ReplayHeader) : IBattleRoyaleReplay {
        public int ReplayRank { get; set; }

        private IOptionalReplayData? _replayData;
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        public async Task<IOptionalReplayData> GetReplayDataAsync(bool bypassCache) {
            if (_replayData == null || bypassCache) {
                await _semaphoreSlim.WaitAsync();
                var player = await ReplayHeader.LoadPlayerAsync(false, CancellationToken.None);
                _replayData = await ReplayerMenuLoader.LoadOptionalDataAsync(ReplayHeader.ReplayInfo, player);
                _semaphoreSlim.Release();
            }
            return _replayData;
        }

        private static Color GetReplayColor(IReplayInfo replayInfo) {
            var colorSeed = $"{replayInfo.Timestamp}{replayInfo.PlayerID}{replayInfo.SongName}".GetHashCode();
            return ColorUtils.RandomColor(rand: new(colorSeed));
        }
    }
}