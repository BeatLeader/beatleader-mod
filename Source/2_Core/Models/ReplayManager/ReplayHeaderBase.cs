using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Utils;

namespace BeatLeader.Models {
    internal abstract class ReplayHeaderBase {
        protected ReplayHeaderBase(Player? player) {
            _cachedPlayer = player;
        }
        
        public abstract IReplayInfo ReplayInfo { get; }

        private readonly SemaphoreSlim _loadPlayerSemaphore = new(1, 1);
        private Player? _cachedPlayer;

        public async Task<Player> LoadPlayerAsync(bool bypassCache, CancellationToken token) {
            await _loadPlayerSemaphore.WaitAsync(token);

            if (!bypassCache) {
                _cachedPlayer ??= ReplayManagerCache.GetPlayer(ReplayInfo.PlayerID);

                if (_cachedPlayer != null) {
                    _loadPlayerSemaphore.Release();
                    return _cachedPlayer;
                }
            }

            var request = PlayerRequest.SendRequest(ReplayInfo.PlayerID);
            await request.Join();

            if (request.RequestStatusCode is not HttpStatusCode.OK) {
                Plugin.Log.Error($"Failed to load player {ReplayInfo.PlayerID} from the server!");
            } else {
                _cachedPlayer = request.Result;
                ReplayManagerCache.AddPlayer(_cachedPlayer!);
            }

            _loadPlayerSemaphore.Release();

            return _cachedPlayer ?? Player.GuestPlayer;
        }
    }
}