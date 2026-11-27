using System.Collections.Generic;
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
        public Player? PreviousPlayer => _history.Count > 0 ? _history.Peek() : null;

        PlayerProfileContext IState<PlayerProfileContext>.Value => this;

        #endregion

        #region Load

        private readonly Stack<Player> _history = new();
        private int _loadVersion;

        public void Open(string playerId, bool keepHistory) {
            if (!keepHistory) {
                _history.Clear();
            } else if (Player != null && Player.id != playerId) {
                _history.Push(Player);
            }

            Load(playerId);
        }

        public void Back() {
            if (_history.Count > 0) {
                Load(_history.Pop().id);
            }
        }

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
