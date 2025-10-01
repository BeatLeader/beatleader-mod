using System;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleReplay : IBattleRoyaleReplay {
        public BattleRoyaleReplay(IReplayHeader header) {
            ReplayHeader = header;
        }

        public int ReplayRank { get; set; }
        public IReplayHeader ReplayHeader { get; }

        private BattleRoyaleReplayData? _replayData;
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        public async Task<BattleRoyaleReplayData> GetBattleRoyaleDataAsync(bool bypassCache, CancellationToken token) {
            if (_replayData == null || bypassCache) {
                await _semaphoreSlim.WaitAsync(token);

                var player = await ReplayHeader.LoadPlayerAsync(false, token);
                var avatarData = await player.GetBeatAvatarAsync(false, token);
                
                var info = ReplayHeader.ReplayInfo;
                var accentColor = GetReplayColor(info);

                _replayData = new BattleRoyaleReplayData(avatarData, accentColor);

                _semaphoreSlim.Release();
            }

            return _replayData.Value;
        }

        private static Color GetReplayColor(IReplayInfo info) {
            var colorSeed = $"{info.Timestamp}{info.PlayerID}{info.SongName}".GetHashCode();
            return ColorUtils.RandomColor(rand: new(colorSeed));
        }
    }
}