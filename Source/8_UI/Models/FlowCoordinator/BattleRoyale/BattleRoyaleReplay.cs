using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal record BattleRoyaleReplay(IReplayHeaderBase ReplayHeader) : IBattleRoyaleReplay {
        public int ReplayRank { get; set; }

        private IOptionalReplayData? _replayData;

        public async Task<IOptionalReplayData> GetReplayDataAsync() {
            if (_replayData == null) {
                var player = await ReplayHeader.LoadPlayerAsync(false, CancellationToken.None);
                var avatarSettings = await player.GetAvatarAsync();
                //
                _replayData = new BattleRoyaleOptionalReplayData(
                    avatarSettings.ToAvatarData(),
                    GetReplayColor(ReplayHeader.ReplayInfo)
                );
            }
            return _replayData;
        }

        private static Color GetReplayColor(IReplayInfo replayInfo) {
            var colorSeed = $"{replayInfo.Timestamp}{replayInfo.PlayerID}{replayInfo.SongName}".GetHashCode();
            return ColorUtils.RandomColor(rand: new(colorSeed));
        }
    }
}