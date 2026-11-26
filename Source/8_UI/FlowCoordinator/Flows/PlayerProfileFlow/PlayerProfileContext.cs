using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models;
using BeatLeader.Utils;
using Reactive;

namespace BeatLeader.UI.Hub {
    internal class PlayerProfileContext : StateBase<PlayerProfileContext>, IState<PlayerProfileContext> {
        #region Props

        public string? PlayerId { get; private set; }
        public Player? Player { get; private set; }
        public PlayerFollowersInfo? Followers { get; private set; }
        public bool IsLoading { get; private set; }
        public bool IsFailed { get; private set; }

        PlayerProfileContext IState<PlayerProfileContext>.Value => this;

        #endregion

        #region Load

        private int _loadVersion;

        public void Load(string playerId) {
            _ = LoadAsync(playerId).RunCatching();
        }

        private async Task LoadAsync(string playerId) {
            var version = ++_loadVersion;

            PlayerId = playerId;
            Player = null;
            Followers = null;
            IsLoading = true;
            IsFailed = false;
            NotifyValueInvalidated();

            var playerTask = PlayerProfileRequest.Send(playerId).Join();
            var followersTask = PlayerFollowersInfoRequest.Send(playerId).Join();

            await Task.WhenAll(playerTask, followersTask);

            if (version != _loadVersion) {
                return;
            }

            Player = playerTask.Result.Result;
            Followers = followersTask.Result.Result;
            IsLoading = false;
            IsFailed = Player == null;
            NotifyValueInvalidated();
        }

        #endregion
    }
}
